namespace FilesystemBackup.Service.Dialog
{
    public interface IFileDialogService
    {
        string? GetPathFromDialog(OpenFileDialogOptions options);
        string? GetPathFromDialog(OpenFolderDialogOptions options);
        string? GetPathFromDialog(SaveFileDialogOptions options);
    }
}