namespace FilesystemBackup.Service.Compression
{
    public interface ICompressionService
    {
        byte[] Compress(byte[] bytes);
        byte[] Decompress(byte[] bytes);
    }
}