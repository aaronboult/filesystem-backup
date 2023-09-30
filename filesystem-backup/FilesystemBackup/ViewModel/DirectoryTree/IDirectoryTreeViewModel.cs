using FilesystemBackup.Model;
using FilesystemBackup.Service.Dialog;
using System.Collections.ObjectModel;

namespace FilesystemBackup.ViewModel.DirectoryTree
{
    public interface IDirectoryTreeViewModel
    {
        ObservableCollection<IDirectoryTreeViewModelItem> Items { get; }

        void Rebuild(ScannedDirectory root, IProgressDialogService? dialog);
    }
}