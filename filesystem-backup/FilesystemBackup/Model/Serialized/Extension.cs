using System.Collections.Generic;

namespace FilesystemBackup.Model.Serialized;

internal static class Extension
{
    public static byte[] Serialize(this SerializedString[] serializedStringArray)
    {
        List<byte> byteList = [];

        foreach (SerializedString serializedString in serializedStringArray)
        {
            byteList.AddRange(serializedString.Serialize());
        }

        return [.. byteList];
    }

    public static SerializedString[] Deserialize(this byte[] bytes, int amount)
    {
        List<SerializedString> serializedStrings = [];
        int offset = 0;

        for (int i = 0; i < amount; i++)
        {
            SerializedString serializedString = SerializedString.Deserialize(bytes[offset..]);
            offset += serializedString.ByteLength;

            serializedStrings.Add(serializedString);
        }

        return [.. serializedStrings];
    }
}
