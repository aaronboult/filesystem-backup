using System;
using System.Collections.Generic;
using System.Linq;

namespace FilesystemBackup.Model.Serialized;

internal class SerializedScanChunk : ISerializable
{
    public int DirectoryCount => SerializedDirectories.Length;
    public readonly SerializedScannedDirectory[] SerializedDirectories;

    public int ByteLength => SerializedDirectories.Sum(directory => directory.ByteLength) + sizeof(int);


    public const int MaxSerializedScanChunkDirectoryCount = 1000;


    public SerializedScanChunk(ScannedDirectory[] directories)
    {
        if (directories.Length > MaxSerializedScanChunkDirectoryCount)
            throw new ArgumentException(
                $"SerializedScanChunk cannot contain more than {MaxSerializedScanChunkDirectoryCount} directories"
            );


        SerializedDirectories = directories.Select(directory => new SerializedScannedDirectory(directory)).ToArray();
    }

    public SerializedScanChunk(IEnumerable<SerializedScannedDirectory> SerializedDirectories)
    {
        this.SerializedDirectories = SerializedDirectories.ToArray();
    }


    public byte[] Serialize()
    {
        byte[] data = SerializedDirectories.SelectMany(directory => directory.Serialize()).ToArray();

        return [.. BitConverter.GetBytes(ByteLength), .. data];
    }

    public static SerializedScanChunk Deserialize(byte[] data)
    {
        List<SerializedScannedDirectory> directories = [];

        int offset = sizeof(int);

        while (offset < data.Length - 1)
        {
            SerializedScannedDirectory deserializedScannedDirectory = SerializedScannedDirectory.Deserialize(data[offset..]);
            offset += deserializedScannedDirectory.ByteLength;

            directories.Add(deserializedScannedDirectory);
        }

        return new SerializedScanChunk(directories.ToArray());
    }

    public PartialScannedDirectory[] Build()
    {
        List<PartialScannedDirectory> scannedDirectories = [];

        Dictionary<string, SerializedScannedDirectory> manifest = SerializedDirectories.ToDictionary(
            directory => directory.Path.StringValue,
            directory => directory
        );

        int lastManifestCount = manifest.Count;

        while (manifest.Count > 0)
        {
            scannedDirectories.Add(BuildDirectory(manifest.Values.First(), ref manifest));

            if (manifest.Count == lastManifestCount)
                throw new Exception("Manifest count did not decrease");

            lastManifestCount = manifest.Count;
        }

        return [.. scannedDirectories];
    }

    private PartialScannedDirectory BuildDirectory(SerializedScannedDirectory directory,
        ref Dictionary<string, SerializedScannedDirectory> manifest)
    {
        List<PartialScannedDirectory> subdirectories = [];
        List<string> missingPaths = directory.SubdirectoryPaths.Select(subdirectoryPath => subdirectoryPath.StringValue).ToList();

        foreach (SerializedString subdirectoryPath in directory.SubdirectoryPaths)
        {
            if (manifest.TryGetValue(subdirectoryPath.StringValue, out SerializedScannedDirectory value))
            {
                subdirectories.Add(BuildDirectory(value, ref manifest));
                missingPaths.Remove(subdirectoryPath.StringValue);
            }
        }

        List<ScannedFile> files = [];

        foreach (SerializedString filePath in directory.FilePaths)
        {
            files.Add(new ScannedFile(filePath.StringValue));
        }

        manifest.Remove(directory.Path.StringValue);

        return new PartialScannedDirectory(
            directory.Path.StringValue,
            [.. subdirectories],
            [.. files],
            [.. missingPaths]
        );
    }

    public static int GetByteLength(byte[] data)
    {
        return BitConverter.ToInt32(data.AsSpan()[..sizeof(int)]);
    }
}
