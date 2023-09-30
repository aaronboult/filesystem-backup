using System;
using System.Collections.Generic;
using System.Linq;

namespace FilesystemBackup.Model.Serialized;

internal struct SerializedScannedDirectory : ISerializable
{
    public SerializedString Path;
    public readonly int DirectoryCount => SubdirectoryPaths.Length;
    public SerializedString[] SubdirectoryPaths;
    public readonly int FileCount => FilePaths.Length;
    public SerializedString[] FilePaths;

    public int ByteLength
    {
        get
        {
            int byteCount = Path.ByteLength;

            byteCount += sizeof(int);

            byteCount += SubdirectoryPaths.Sum(subdirectoryPath => subdirectoryPath.ByteLength);

            byteCount += sizeof(int);

            byteCount += FilePaths.Sum(filePath => filePath.ByteLength);

            return byteCount;
        }
    }


    public SerializedScannedDirectory(ScannedDirectory directory)
    {
        Path = new SerializedString(directory.Path);
        SubdirectoryPaths = directory.Subdirectories.Select(
            subdirectory => new SerializedString(subdirectory.Path)
        ).ToArray();

        FilePaths = directory.Files.Select(
            file => new SerializedString(file.Path)
        ).ToArray();
    }

    public SerializedScannedDirectory(SerializedString path,
        SerializedString[] subdirectoryPaths, SerializedString[] filePaths)
    {
        Path = path;
        SubdirectoryPaths = subdirectoryPaths;
        FilePaths = filePaths;
    }


    public readonly byte[] Serialize()
    {
        List<byte> bytes =
        [
            .. Path.Serialize(),
            .. BitConverter.GetBytes(DirectoryCount),
            .. SubdirectoryPaths.Serialize(),
            .. BitConverter.GetBytes(FileCount),
            .. FilePaths.Serialize(),
        ];

        return [.. bytes];
    }

    public static SerializedScannedDirectory Deserialize(byte[] bytes)
    {
        int offset = 0;


        SerializedString path = SerializedString.Deserialize(bytes);
        offset += path.ByteLength;

        var slice = bytes.AsSpan()[offset..(offset + sizeof(int))];


        int directoryCount = BitConverter.ToInt32(slice);
        offset += sizeof(int);


        SerializedString[] subdirectoryPaths = bytes[offset..].Deserialize(directoryCount);
        offset += subdirectoryPaths.Sum(subdirectoryPath => subdirectoryPath.ByteLength);


        int fileCount = BitConverter.ToInt32(bytes.AsSpan()[offset..(offset + sizeof(int))]);
        offset += sizeof(int);


        SerializedString[] filePaths = bytes[offset..].Deserialize(fileCount);
        offset += filePaths.Sum(filePath => filePath.ByteLength);


        return new SerializedScannedDirectory(
            path,
            subdirectoryPaths,
            filePaths
        );
    }
}
