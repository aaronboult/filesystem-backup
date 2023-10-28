using CommunityToolkit.Mvvm.Messaging;
using FilesystemBackup.Service.Dialog;
using FilesystemBackup.Service.DirectoryScan;
using FilesystemBackup.Service.IO;
using Microsoft.Extensions.DependencyInjection;

namespace FilesystemBackup.Service;

public static class Bootstrapper
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessenger>(s =>
        {
            return WeakReferenceMessenger.Default;
        });
        services.AddTransient<IDialogService, DialogService>();
        services.AddTransient<IFileDialogService, FileDialogService>();
        services.AddTransient<IProgressDialogService, ProgressDialogService>();
        services.AddTransient<IIOService, IOService>();
        services.AddTransient<IDirectoryScanService, DirectoryScanService>();
    }
}
