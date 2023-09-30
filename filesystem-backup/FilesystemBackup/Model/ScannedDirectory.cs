using System.Collections.Generic;
using System.Linq;

namespace FilesystemBackup.Model;

public record ScannedDirectory(string Path, ScannedDirectory[] Subdirectories, ScannedFile[] Files)
{
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
}