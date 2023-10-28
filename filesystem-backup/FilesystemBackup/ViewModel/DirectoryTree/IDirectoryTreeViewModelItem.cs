using System;
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
    string Name { get; }
    DirectoryTreeViewModelItemType Type { get; }
    ObservableCollection<IDirectoryTreeViewModelItem> Items { get; }

    event Action? OnIncludeContentsChangedEvent;

    bool? IncludeContents { get; set; }


    void AddItem(IDirectoryTreeViewModelItem item);
    void ClearItems();
}