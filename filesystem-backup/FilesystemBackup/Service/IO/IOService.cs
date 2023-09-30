


using FilesystemBackup.Service.Dialog;
using System;
using System.IO;

namespace FilesystemBackup.Service.IO;

public class IOService : IIOService
{
    private readonly IDialogService DialogService;


    public IOService(IDialogService dialogService)
    {
        DialogService = dialogService;
    }


    public byte[]? ReadAllFileBytes(string path)
    {
        if (!File.Exists(path))
        {
            DialogService.ShowError($"File {path} does not exist");

            return null;
        }


        try
        {

            return File.ReadAllBytes(path);

        }
        catch (UnauthorizedAccessException e)
        {
            DialogService.ShowError($"Unauthorized to read file {path}:\n{e.Message}");
        }
        catch (IOException e)
        {
            DialogService.ShowError($"Failed to read file {path}: {e.Message}");
        }
        catch (Exception ex)
        {
            string message = $"Failed to read file {path}\n" +
                "An unknown error occurred, please report this:\n" +
                $"{ex.Message}";

            DialogService.ShowError($"Failed to read file {path}: {ex.Message}");
        }


        return null;
    }
}
