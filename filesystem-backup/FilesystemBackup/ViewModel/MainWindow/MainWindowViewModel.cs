
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FilesystemBackup.Model;
using FilesystemBackup.Service.Dialog;
using FilesystemBackup.Service.DirectoryScan;
using FilesystemBackup.Service.IO;
using FilesystemBackup.ViewModel.DirectoryTree;
using FilesystemBackup.ViewModel.MainWindow.Message;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FilesystemBackup.ViewModel.MainWindow;

public class MainWindowViewModel : ObservableObject
{
    public IDirectoryTreeViewModel DirectoryTreeViewModel { get; }

    public ICommand RefreshDirectoryTreeMenuItemsCommand { get; }
    public ICommand ScanDirectoryCommand { get; }
    public ICommand SaveScanCommand { get; }
    public ICommand OpenScanCommand { get; }
    public ICommand RestoreScanCommand { get; }

    public bool CanScanDirectory => true;
    public bool CanSaveScan => true;
    public bool CanOpenScan => true;
    public bool CanRestoreScan => true;


    private readonly IMessenger Messenger;
    private readonly IFileDialogService FileDialogService;
    private readonly IDirectoryScanService DirectoryScanService;
    private readonly IIOService FileLoadService;
    private readonly IProgressDialogService ProgressDialogService;
    private ScannedDirectory? ScannedDirectory;


    public MainWindowViewModel(IMessenger messenger, IDirectoryTreeViewModel directoryTreeViewModel, IFileDialogService fileDialogService,
        IDirectoryScanService directoryScanService, IIOService fileLoadService,
        IProgressDialogService progressDialogService)
    {
        DirectoryTreeViewModel = directoryTreeViewModel;

        RefreshDirectoryTreeMenuItemsCommand = new RelayCommand(RebuildDirectoryTreeMenuItems);
        ScanDirectoryCommand = new AsyncRelayCommand(ScanDirectory);
        SaveScanCommand = new AsyncRelayCommand(SaveScan);
        OpenScanCommand = new AsyncRelayCommand(OpenScan);
        RestoreScanCommand = new AsyncRelayCommand(RestoreScan);

        Messenger = messenger;
        FileDialogService = fileDialogService;
        DirectoryScanService = directoryScanService;
        FileLoadService = fileLoadService;
        ProgressDialogService = progressDialogService;


        Messenger.Register<RebuildDirectoryTreeMessage>(this, (recipient, message) =>
        {
            RebuildDirectoryTreeMenuItems();
        });
    }


    private void RebuildDirectoryTreeMenuItems()
    {
        if (ScannedDirectory == null)
            return;


        ProgressDialogService.Show(new ProgressDialogSettings
        {
            Title = "Rebuilding directory tree",
            Worker = (dialog) =>
            {
                DirectoryTreeViewModel.Rebuild(ScannedDirectory, dialog);
            },
        });
    }

    public async Task ScanDirectory()
    {
        await ScanDirectory(true);
    }

    public async Task ScanDirectory(bool refreshTree)
    {
        if (!CanScanDirectory)
            return;

        string? path = FileDialogService.GetPathFromDialog(new OpenFolderDialogOptions()
        {
            Description = "Select a directory to scan",
            UseDescriptionForTitle = true,
        });

        if (path == null)
        {
            return;
        }


        Debug.WriteLine("Before dialog show");
        await ProgressDialogService.Show(new ProgressDialogSettings
        {
            Title = "Scanning directory",
            Worker = (dialog) =>
            {
                ScannedDirectory = DirectoryScanService.ScanDirectory(path, dialog);
            },
            Finalizer = () => Messenger.Send(new RebuildDirectoryTreeMessage()),
        });
        Debug.WriteLine("After dialog show");
    }

    public async Task SaveScan()
    {
        if (!CanSaveScan)
            return;

        if (ScannedDirectory == null)
            await ScanDirectory();

        if (ScannedDirectory == null)
            return;


        // Filename as Scan-DD-MM-YYYY-HH-MM-SS.fbscan
        string filename = $"Scan-{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";

        string? path = FileDialogService.GetPathFromDialog(new SaveFileDialogOptions
        {
            Title = "Save scan",
            DefaultExt = "fbscan",
            AddExtension = true,
            FileName = $"{filename}.fbscan",
            OverwritePrompt = true,
            Filter = "Filesystem Backup Scan (*.fbscan)|*.fbscan",
        });


        if (path == null)
            return;


        await ProgressDialogService.Show(new ProgressDialogSettings
        {
            Title = "Saving scan",
            Worker = (dialog) =>
            {
                using FileStream fileStream = File.Create(path);

                DirectoryScanService.SerializeScan(fileStream, ScannedDirectory, dialog);
            },
            Finalizer = () => Messenger.Send(new RebuildDirectoryTreeMessage()),
        });
    }

    public async Task OpenScan()
    {
        if (!CanOpenScan)
            return;

        string? path = FileDialogService.GetPathFromDialog(new OpenFileDialogOptions()
        {
            Title = "Open scan",
            Filter = "Filesystem Backup Scan (*.fbscan)|*.fbscan",
            FilterIndex = 0,
        });


        if (path == null || !File.Exists(path))
            return;


        await ProgressDialogService.Show(new ProgressDialogSettings
        {
            Title = "Opening scan",
            Worker = (dialog) =>
            {
                using FileStream fileStream = File.OpenRead(path);
                using MemoryStream memoryStream = new();

                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);

                memoryStream.Write(buffer, 0, buffer.Length);
                memoryStream.Position = 0;

                ScannedDirectory = DirectoryScanService.DeserializeScan(memoryStream, dialog);
            },
            Finalizer = () => Messenger.Send(new RebuildDirectoryTreeMessage()),
        });
    }

    public async Task RestoreScan()
    {
        if (ScannedDirectory == null)
            await ScanDirectory();

        if (ScannedDirectory == null)
            return;


        string? path = FileDialogService.GetPathFromDialog(new OpenFolderDialogOptions()
        {
            Description = "Select a directory to restore the scan to",
            UseDescriptionForTitle = true,
        });

        if (path == null)
            return;

        await ProgressDialogService.Show(new ProgressDialogSettings
        {
            Title = "Restoring scan",
            Worker = (dialog) =>
            {
                DirectoryScanService.RestoreScan(path, ScannedDirectory, dialog);
            },
        });
    }
}
