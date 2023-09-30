using System;

namespace FilesystemBackup.Model.Serialized;

internal readonly struct SerializedString : ISerializable
{
    public string StringValue { get; }
    public readonly int ByteLength => Bytes.Length + sizeof(int);

    public readonly byte[] Bytes;

    public SerializedString(string stringValue)
    {
        Bytes = System.Text.Encoding.UTF8.GetBytes(stringValue);
        StringValue = stringValue;
    }


    public readonly byte[] Serialize()
    {
        // First get Length as byte[] then combine with Bytes to get final byte[]
        var lengthBytes = BitConverter.GetBytes(ByteLength);

        return [.. lengthBytes, .. Bytes];
    }

    public static SerializedString Deserialize(byte[] bytes)
    {
        int length = BitConverter.ToInt32(bytes.AsSpan()[..sizeof(int)]);
        byte[] stringBytes = bytes[sizeof(int)..length];

        return new SerializedString(System.Text.Encoding.UTF8.GetString(stringBytes));
    }
}
