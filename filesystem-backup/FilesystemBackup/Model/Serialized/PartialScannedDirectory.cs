namespace FilesystemBackup.Model.Serialized;

internal record PartialScannedDirectory(string Path, PartialScannedDirectory[] Subdirectories,
    ScannedFile[] ScannedFiles, string[] MissingPaths)
{
}
