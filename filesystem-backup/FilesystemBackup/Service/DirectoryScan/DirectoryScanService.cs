using FilesystemBackup.Model;
using FilesystemBackup.Model.Serialized;
using FilesystemBackup.Service.Compression;
using FilesystemBackup.Service.Dialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FilesystemBackup.Service.DirectoryScan;

public class DirectoryScanService : IDirectoryScanService
{
    private const int SerializeChunkingPercentage = 70;
    public const int DeserializeBuildingPercentage = 30;

    private readonly ICompressionService CompressionService;


    public DirectoryScanService(ICompressionService compressionService)
    {
        CompressionService = compressionService;
    }


    public ScannedDirectory? ScanDirectory(string path, IProgressDialogService? progressDialogService = null)
    {
        // Ensure path exists
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        return new ScannedDirectoryBuilder(path).Build(progressDialogService);
    }

    public void RestoreScan(string path, ScannedDirectory directory, IProgressDialogService? progressDialogService = null)
    {
        // Check if path is empty before we restore. If it isn't we can't restore
        if (Directory.EnumerateFileSystemEntries(path).Any())
        {
            throw new Exception($"Directory is not empty: {path}");
        }

        RestoreScannedDirectory(path, directory, progressDialogService);
    }

    private void RestoreScannedDirectory(string path, ScannedDirectory directory, IProgressDialogService? progressDialogService,
        bool canReport = true)
    {
        string[] fileNames = directory.Files.Select(file => file.Name).ToArray();

        int percentage = 0;

        // Create directories
        foreach (ScannedDirectory subdirectory in directory.Subdirectories)
        {
            string subdirectoryPath = Path.Combine(path, subdirectory.Name);

            Directory.CreateDirectory(subdirectoryPath);
            RestoreScannedDirectory(subdirectoryPath, subdirectory, progressDialogService, false);

            if (progressDialogService?.IsCancelled ?? false)
                return;

            if (canReport)
                progressDialogService?.ReportProgress(percentage += 100 / directory.Subdirectories.Length);
        }

        // Write contents.txt file containing file names
        if (fileNames.Length > 0)
            File.WriteAllLines(Path.Combine(path, "contents.txt"), fileNames);
    }

    public byte[] SerializeScan(ScannedDirectory directory, IProgressDialogService? progressDialogService = null)
    {
        Debug.WriteLine("Serializing directory");
        List<SerializedScannedDirectory> serializedDirectories = SerializeScannedDirectory(directory, progressDialogService);
        Debug.WriteLine("Serializing directory done");

        // Chunk bytes by SerializedScanChunk.MaxSerializedScanChunkDirectoryCount
        List<SerializedScanChunk> chunks = [];


        for (int index = 0; index < serializedDirectories.Count; index += SerializedScanChunk.MaxSerializedScanChunkDirectoryCount)
        {
            int endIndex = Math.Min(
                index + SerializedScanChunk.MaxSerializedScanChunkDirectoryCount,
                serializedDirectories.Count
            );

            chunks.Add(
                new SerializedScanChunk(serializedDirectories[index..endIndex])
            );
        }

        Debug.WriteLine($"Reporting progress: {SerializeChunkingPercentage + 10}");
        progressDialogService?.ReportProgress(SerializeChunkingPercentage + 10);


        int progress = 0;

        byte[] result = chunks.SelectMany(chunk =>
        {
            Debug.WriteLine($"Reporting progress in chunks.SelectMany: {progress + SerializeChunkingPercentage / chunks.Count}");
            progressDialogService?.ReportProgress(progress += SerializeChunkingPercentage / chunks.Count);

            return chunk.Serialize();
        }).ToArray();

        // Return compressed
        return CompressionService.Compress(result);
    }

    private List<SerializedScannedDirectory> SerializeScannedDirectory(ScannedDirectory directory, IProgressDialogService? progressDialogService)
    {
        List<SerializedScannedDirectory> scannedDirectoryBytes = [
            new SerializedScannedDirectory(directory)
        ];

        int progress = 0;

        scannedDirectoryBytes.AddRange(
            directory.Subdirectories.SelectMany((subdirectory) =>
            {
                Debug.WriteLine($"SerializeScannedDirectory report progress: {progress + SerializeChunkingPercentage / directory.Subdirectories.Length}");
                progressDialogService?.ReportProgress(progress += SerializeChunkingPercentage / directory.Subdirectories.Length);

                return SerializeScannedDirectory(subdirectory, null);
            })
        );

        return scannedDirectoryBytes;
    }

    public ScannedDirectory? DeserializeScan(byte[] data, IProgressDialogService? progressDialog = null)
    {
        progressDialog?.SetInfoText("Deserializing chunk data", string.Empty);


        List<byte[]> chunkOffsets = [];
        int offset = 0;

        // Decompress data
        data = CompressionService.Decompress(data);

        while (offset < data.Length - 1)
        {
            int chunkLength = SerializedScanChunk.GetByteLength(data[offset..]);

            chunkOffsets.Add(
                data[offset..(offset + chunkLength)]
            );

            offset += chunkLength;


            if (progressDialog?.IsCancelled ?? false)
                return null;
        }


        Dictionary<string, PartialScannedDirectory> manifest = [];
        int progress = 0;

        progressDialog?.SetInfoText("Building partial directories", string.Empty);

        chunkOffsets.AsParallel().ForAll((data) =>
        {
            PartialScannedDirectory[] partialDirectories = SerializedScanChunk.Deserialize(data).Build();

            lock (manifest)
                foreach (var partialDirectory in partialDirectories)
                {
                    manifest.Add(partialDirectory.Path, partialDirectory);

                    if (progressDialog?.IsCancelled ?? false)
                        return;
                }

            progressDialog?.ReportProgress(progress += DeserializeBuildingPercentage / chunkOffsets.Count);
        });


        if (progressDialog?.IsCancelled ?? false)
            return null;


        // Find the shortest path length to find the root
        PartialScannedDirectory root = manifest[
            manifest.Keys.OrderBy(
                path => path
            ).First()
        ];


        return FromPartialScannedDirectory(root, ref manifest, progressDialog);
    }

    private ScannedDirectory? FromPartialScannedDirectory(PartialScannedDirectory directory,
        ref Dictionary<string, PartialScannedDirectory> manifest, IProgressDialogService? progressDialog)
    {
        List<ScannedDirectory?> subdirectories = [];

        int progress = DeserializeBuildingPercentage;
        int totalCount = directory.Subdirectories.Length + directory.MissingPaths.Length;


        foreach (PartialScannedDirectory subdirectory in directory.Subdirectories)
        {
            progressDialog?.SetInfoText($"Building partial directory", subdirectory.Path);

            subdirectories.Add(FromPartialScannedDirectory(subdirectory, ref manifest, null));
            progressDialog?.ReportProgress(progress += DeserializeBuildingPercentage / totalCount);

            if (progressDialog?.IsCancelled ?? false)
                return null;
        }


        foreach (string missingSubdirectory in directory.MissingPaths)
        {
            progressDialog?.SetInfoText($"Resolving missing partial directory's subdirectory", missingSubdirectory);

            if (manifest.TryGetValue(missingSubdirectory, out PartialScannedDirectory? value))
                subdirectories.Add(
                    FromPartialScannedDirectory(value, ref manifest, null)
                );

            else
                throw new Exception($"Missing subdirectory: {missingSubdirectory}");

            progressDialog?.ReportProgress(progress += DeserializeBuildingPercentage / totalCount);

            if (progressDialog?.IsCancelled ?? false)
                return null;
        }


        return new ScannedDirectory(
            directory.Path,
            [.. subdirectories],
            directory.ScannedFiles
        );
    }
}