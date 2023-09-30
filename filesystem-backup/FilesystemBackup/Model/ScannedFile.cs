namespace FilesystemBackup.Model;

public readonly struct ScannedFile
{
    public readonly string Path { get; }
    public readonly string Name => System.IO.Path.GetFileName(Path);


    public ScannedFile(string path)
    {
        Path = path;
    }


    public byte[] Serialize()
    {
        return System.Text.Encoding.UTF8.GetBytes(Path);
    }
}