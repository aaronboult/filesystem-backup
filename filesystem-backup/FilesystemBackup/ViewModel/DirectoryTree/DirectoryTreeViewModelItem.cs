using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace FilesystemBackup.ViewModel.DirectoryTree;

public class DirectoryTreeViewModelItem(string name, DirectoryTreeViewModelItemType type)
    : ObservableObject, IDirectoryTreeViewModelItem
{
    public string Name { get; } = name;
    public DirectoryTreeViewModelItemType Type { get; } = type;
    public ObservableCollection<IDirectoryTreeViewModelItem> Items { get; private set; } = new();


    public void AddItem(IDirectoryTreeViewModelItem item)
    {
        Items.Add(item);
    }

    public void ClearItems()
    {
        Items.Clear();
    }
}
