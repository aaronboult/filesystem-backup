using CommunityToolkit.Mvvm.ComponentModel;
using FilesystemBackup.Model;
using FilesystemBackup.Service.Dialog;
using System.Collections.ObjectModel;

namespace FilesystemBackup.ViewModel.DirectoryTree;

public class DirectoryTreeViewModel : ObservableObject, IDirectoryTreeViewModel
{
    public ObservableCollection<IDirectoryTreeViewModelItem> Items
    {
        get => items;
        set => SetProperty(ref items, value);
    }
    private ObservableCollection<IDirectoryTreeViewModelItem> items = new();


    public void Rebuild(ScannedDirectory root, IProgressDialogService? dialog)
    {
        IDirectoryTreeViewModelItem? rootItem = BuildDirectoryTree(root, dialog, true);

        if (rootItem != null)
            Items = [rootItem];
    }

    private static DirectoryTreeViewModelItem? BuildDirectoryTree(ScannedDirectory directory,
        IProgressDialogService? progressDialog, bool isDrive)
    {
        var item = new DirectoryTreeViewModelItem(
            isDrive ? directory.Path : directory.Name,
            isDrive ? DirectoryTreeViewModelItemType.Drive : DirectoryTreeViewModelItemType.Directory
        );

        int progress = 0;
        int itemTotal = directory.Subdirectories.Length + directory.Files.Length;

        foreach (var subdirectory in directory.Subdirectories)
        {
            IDirectoryTreeViewModelItem? subItem = BuildDirectoryTree(subdirectory, null, false);


            if (subItem == null || (progressDialog?.IsCancelled ?? false))
                return null;


            item.AddItem(subItem);


            progressDialog?.SetInfoText(
                "Building directory...",
                subdirectory.Path
            );
            progressDialog?.ReportProgress(progress += 100 / itemTotal);
        }

        for (int i = 0; i < directory.Files.Length; i++)
        {
            ScannedFile file = directory.Files[i];

            item.AddItem(new DirectoryTreeViewModelItem(
                file.Name,
                DirectoryTreeViewModelItemType.File,
                ref file
            ));


            progressDialog?.SetInfoText(
                "Building file...",
                file.Path
            );
            progressDialog?.ReportProgress(progress += 100 / itemTotal);

            if (progressDialog?.IsCancelled ?? false)
                return null;
        }

        progressDialog?.ReportProgress(100);

        return item;
    }
}
