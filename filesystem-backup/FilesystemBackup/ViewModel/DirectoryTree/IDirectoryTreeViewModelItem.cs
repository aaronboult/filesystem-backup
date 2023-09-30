using System.Collections.ObjectModel;

namespace FilesystemBackup.ViewModel.DirectoryTree;

public enum DirectoryTreeViewModelItemType
{
    Drive,
    Directory,
    File
}

public interface IDirectoryTreeViewModelItem
{
    ObservableCollection<IDirectoryTreeViewModelItem> Items { get; }
    string Name { get; }
    DirectoryTreeViewModelItemType Type { get; }
    public string ImageSource => string.Empty;

    void AddItem(IDirectoryTreeViewModelItem item);
    void ClearItems();
}