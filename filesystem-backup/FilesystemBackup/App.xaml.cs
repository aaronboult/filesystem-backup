using FilesystemBackup.Service;
using FilesystemBackup.View;
using FilesystemBackup.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace FilesystemBackup;

public partial class App : Application
{
    private IHost ServiceHost;


    public App()
    {
        ServiceHost = ConfigureServiceHost();
    }


    private IHost ConfigureServiceHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddViews();
                services.AddViewModels();
                services.AddServices();
            }).Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceHost.Start();

        var window = ServiceHost.Services.GetRequiredService<MainWindow>();

        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }
}
