using FilesystemBackup.ViewModel.DirectoryTree;
using FilesystemBackup.ViewModel.MainWindow;
using Microsoft.Extensions.DependencyInjection;

namespace FilesystemBackup.ViewModel;

public static class Bootstrapper
{
    public static void AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<IDirectoryTreeViewModel, DirectoryTreeViewModel>();
    }
}
