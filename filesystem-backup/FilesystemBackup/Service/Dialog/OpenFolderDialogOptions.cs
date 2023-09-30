namespace FilesystemBackup.Service.Dialog;

// For Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
public struct OpenFolderDialogOptions
{
    public string Description { get; set; }
    public bool IsVistaFolderDialogSupported { get; set; }
    public string RootFolder { get; set; }
    public string SelectedPath { get; set; }
    public bool ShowNewFolderButton { get; set; }
    public bool UseDescriptionForTitle { get; set; }
}
