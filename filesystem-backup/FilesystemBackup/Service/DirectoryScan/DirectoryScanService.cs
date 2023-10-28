using FilesystemBackup.Model;
using FilesystemBackup.Service.Dialog;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace FilesystemBackup.Service.DirectoryScan;

public class DirectoryScanService : IDirectoryScanService
{
    private const int SerializeChunkingPercentage = 70;
    public const int DeserializeBuildingPercentage = 30;


    public DirectoryScanService()
    {
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

        string[] fileNames = directory.Files.Where(file => file.IncludeContents == false).Select(file => file.Name).ToArray();

        // Write contents.txt file containing file names
        if (fileNames.Length > 0)
            File.WriteAllLines(Path.Combine(path, "contents.txt"), fileNames);

        foreach (ScannedFile file in directory.Files.Where(file => file.IncludeContents))
            File.WriteAllBytes(Path.Combine(path, file.Name), file.ByteContents);
    }

    public void SerializeScan(Stream outputStream, ScannedDirectory directory, IProgressDialogService? progressDialogService = null)
    {
        // progressDialogService?.SetInfoText("Deserializing chunk data", string.Empty);
        progressDialogService?.ReportProgress(0);

        using GZipStream compressionStream = new(outputStream, CompressionMode.Compress);

        directory.Serialize(compressionStream);

        progressDialogService?.ReportProgress(100);
    }

    public ScannedDirectory? DeserializeScan(Stream inputStream, IProgressDialogService? progressDialogService = null)
    {
        progressDialogService?.ReportProgress(0);

        using MemoryStream memoryStream = new();
        using (GZipStream compressionStream = new(inputStream, CompressionMode.Decompress))
        {
            compressionStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
        }

        ScannedDirectory directory = new(memoryStream);

        progressDialogService?.ReportProgress(100);

        return directory;
    }
}