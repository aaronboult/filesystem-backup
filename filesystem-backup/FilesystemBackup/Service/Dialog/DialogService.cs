using System.Windows;

namespace FilesystemBackup.Service.Dialog;

public class DialogService : IDialogService
{
    public void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}