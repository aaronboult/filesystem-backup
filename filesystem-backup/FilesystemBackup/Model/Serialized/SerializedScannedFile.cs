namespace FilesystemBackup.Model.Serialized;

internal readonly struct SerializedScannedFile : ISerializable
{
    public readonly int ByteLength => Path.ByteLength;

    public readonly SerializedString Path;

    // TODO Look into storing contents


    public SerializedScannedFile(ScannedFile file)
    {
        Path = new SerializedString(file.Path);
    }

    public SerializedScannedFile(SerializedString path)
    {
        Path = path;
    }


    public readonly byte[] Serialize()
    {
        return Path.Serialize();
    }

    public static SerializedScannedFile Deserialize(byte[] bytes)
    {
        SerializedString path = SerializedString.Deserialize(bytes);

        return new SerializedScannedFile(
            path
        );
    }
}
