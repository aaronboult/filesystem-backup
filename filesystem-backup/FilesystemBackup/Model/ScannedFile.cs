using SimpleSerializer;
using SimpleSerializer.Object;
using System.IO;

namespace FilesystemBackup.Model;

public class ScannedFile : SerializableFile
{
    public bool IncludeContents { get; set; } = false;


    public ScannedFile() : base()
    {
    }

    public ScannedFile(Stream stream) : base(stream)
    {
    }

    public ScannedFile(string path) : base(path)
    {
    }


    public override void Serialize(Serializer serializer)
    {
        serializer.Add(IncludeContents);

        if (IncludeContents)
            base.Serialize(serializer);

        else
            serializer.Add(Path);
    }

    public override void Deserialize(Deserializer deserializer)
    {
        IncludeContents = deserializer.ReadBool();

        if (IncludeContents)
            base.Deserialize(deserializer);

        else
            Path = deserializer.ReadString();
    }
}