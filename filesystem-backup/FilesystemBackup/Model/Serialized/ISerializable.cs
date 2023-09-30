using System;

namespace FilesystemBackup.Model.Serialized;

internal interface ISerializable
{
    int ByteLength { get; }
    byte[] Serialize();
    static ISerializable Deserialize(byte[] bytes)
    {
        throw new NotImplementedException($"Static Deserialize method not implemented for {nameof(ISerializable)}");
    }
}
