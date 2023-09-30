using Ookii.Dialogs.Wpf;
using System;
using System.Reflection;

namespace FilesystemBackup.Service.Dialog;

public class FileDialogService : IFileDialogService
{
    private readonly IDialogService DialogService;


    public FileDialogService(IDialogService selfDialogService)
    {
        DialogService = selfDialogService;
    }


    public string? GetPathFromDialog(OpenFileDialogOptions options)
    {
        var dialog = CloneProperties(new VistaOpenFileDialog(), options);


        if (dialog == null)
            return null;


        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }

        return null;
    }

    public string? GetPathFromDialog(SaveFileDialogOptions options)
    {
        var dialog = CloneProperties(new VistaSaveFileDialog(), options);


        if (dialog == null)
            return null;


        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }

        return null;
    }

    public string? GetPathFromDialog(OpenFolderDialogOptions options)
    {
        var dialog = CloneProperties(new VistaFolderBrowserDialog(), options);


        if (dialog == null)
            return null;


        try
        {
            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedPath;
            }
        }
        catch (Exception exception)
        {
            HandleError(exception);
        }

        return null;
    }

    private T? CloneProperties<T, F>(T destination, F propertySource)
        where T : class
        where F : struct
    {
        try
        {
            foreach (PropertyInfo property in propertySource.GetType().GetProperties())
            {
                object? propertyValue = property.GetValue(propertySource, null);

                if (propertyValue != null)
                {
                    PropertyInfo? destinationProperty = destination.GetType().GetProperty(property.Name);

                    if (destinationProperty != null && destinationProperty.CanWrite)
                        destinationProperty.SetValue(destination, propertyValue, null);
                }
            }

            return destination;
        }
        catch (Exception exception)
        {
            HandleError(exception);

            return null;
        }
    }

    private void HandleError(Exception exception)
    {
        DialogService.ShowError($"An unknown error occurred. Please report this:\n{exception.Message}");
    }
}
