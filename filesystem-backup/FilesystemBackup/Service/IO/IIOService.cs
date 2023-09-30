namespace FilesystemBackup.Service.IO;

public interface IIOService
{
    byte[]? ReadAllFileBytes(string path);
}