using SimpleSerializer;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FilesystemBackup.Model;

public class ScannedDirectory : Serializable
{
    public string Path { get; private set; }
    public ScannedDirectory[] Subdirectories { get; private set; }
    public ScannedFile[] Files { get; private set; }
    public string Name => System.IO.Path.GetFileName(Path);
    public string[] SubdirectoryNames
    {
        get
        {
            List<string> subdirectoryList = [Path];

            Subdirectories.Select(
                subdirectory => subdirectory.SubdirectoryNames
            ).ToList().ForEach(subdirectoryList.AddRange);

            return subdirectoryList.ToArray();
        }
    }


    public ScannedDirectory() : base()
    {
        Path ??= string.Empty;
        Subdirectories ??= [];
        Files ??= [];
    }

    public ScannedDirectory(Stream stream) : base(stream)
    {
        Path ??= string.Empty;
        Subdirectories ??= [];
        Files ??= [];
    }

    public ScannedDirectory(string path, ScannedDirectory[] subdirectories, ScannedFile[] files)
    {
        Path = path;
        Subdirectories = subdirectories;
        Files = files;
    }


    public override void Serialize(Serializer serializer)
    {
        serializer.Add(Path);
        serializer.AddEnumerable(Subdirectories);
        serializer.AddEnumerable(Files);
    }

    public override void Deserialize(Deserializer deserializer)
    {
        Path = deserializer.ReadString();
        Subdirectories = deserializer.ReadObjectEnumerable<ScannedDirectory>().ToArray();
        Files = deserializer.ReadObjectEnumerable<ScannedFile>().ToArray();
    }
}