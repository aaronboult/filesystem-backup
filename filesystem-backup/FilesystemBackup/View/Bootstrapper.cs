using FilesystemBackup.ViewModel.MainWindow;
using Microsoft.Extensions.DependencyInjection;

namespace FilesystemBackup.View;

public static class Bootstrapper
{
    public static void AddViews(this IServiceCollection services)
    {
        services.AddSingleton((services) =>
        {
            return new MainWindow()
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };
        });
    }
}
