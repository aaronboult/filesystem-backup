using System.IO;
using System.IO.Compression;

namespace FilesystemBackup.Service.Compression;

public class CompressionService : ICompressionService
{
    public byte[] Compress(byte[] bytes)
    {
        using MemoryStream memoryStream = new();
        using GZipStream gZipStream = new(memoryStream, CompressionMode.Compress);

        gZipStream.Write(bytes, 0, bytes.Length);
        gZipStream.Close();

        return memoryStream.ToArray();
    }

    public byte[] Decompress(byte[] bytes)
    {
        using MemoryStream memoryStream = new(bytes);
        using GZipStream gZipStream = new(memoryStream, CompressionMode.Decompress);

        using MemoryStream decompressedMemoryStream = new();
        gZipStream.CopyTo(decompressedMemoryStream);

        return decompressedMemoryStream.ToArray();
    }
}
