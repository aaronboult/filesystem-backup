using CommunityToolkit.Mvvm.ComponentModel;
using FilesystemBackup.Model;
using System;
using System.Collections.ObjectModel;

namespace FilesystemBackup.ViewModel.DirectoryTree;

public class DirectoryTreeViewModelItem
    : ObservableObject, IDirectoryTreeViewModelItem
{
    public string Name { get; }
    public DirectoryTreeViewModelItemType Type { get; }
    public ObservableCollection<IDirectoryTreeViewModelItem> Items { get; private set; } = new();

    public event Action? OnIncludeContentsChangedEvent;

    private ScannedFile? ScannedFile = null;


    public bool? IncludeContents
    {
        get => includeContents;
        set => SetProperty(includeContents, value, OnIncludeContentsChanged);
    }
    private bool? includeContents = false;


    public DirectoryTreeViewModelItem(string name, DirectoryTreeViewModelItemType type)
    {
        Name = name;
        Type = type;
    }

    public DirectoryTreeViewModelItem(string name, DirectoryTreeViewModelItemType type, ref ScannedFile scannedFile) : this(name, type)
    {
        ScannedFile = scannedFile;
        IncludeContents = scannedFile.IncludeContents;
    }


    private void OnIncludeContentsChanged(bool? includeContentsChangedNow)
    {
        includeContents = includeContentsChangedNow;

        if (ScannedFile != null)
            ScannedFile.IncludeContents = includeContentsChangedNow == true;

        if (includeContentsChangedNow != null)
            foreach (IDirectoryTreeViewModelItem item in Items)
                item.IncludeContents = includeContentsChangedNow;

        OnIncludeContentsChangedEvent?.Invoke();
    }

    private void CheckIncludeContentsThirdState()
    {
        bool? includeContents = Items[0].IncludeContents;

        for (int i = 1; i < Items.Count; i++)
        {
            if (Items[i].IncludeContents != includeContents)
            {
                IncludeContents = null;
                return;
            }
        }

        IncludeContents = includeContents;
    }

    public void AddItem(IDirectoryTreeViewModelItem item)
    {
        Items.Add(item);

        item.OnIncludeContentsChangedEvent += CheckIncludeContentsThirdState;
    }

    public void ClearItems()
    {
        foreach (IDirectoryTreeViewModelItem item in Items)
            item.OnIncludeContentsChangedEvent -= CheckIncludeContentsThirdState;

        Items.Clear();
    }
}
