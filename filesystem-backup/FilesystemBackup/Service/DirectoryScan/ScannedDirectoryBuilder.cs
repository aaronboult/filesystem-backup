using FilesystemBackup.Model;
using FilesystemBackup.Service.Dialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FilesystemBackup.Service.DirectoryScan;

public class ScannedDirectoryBuilder
{
    private readonly string Path;
    private readonly List<ScannedDirectoryBuilder> Subdirectories;
    private readonly List<ScannedFile> Files;


    public ScannedDirectoryBuilder(string path)
    {
        Path = path;
        Subdirectories = [];
        Files = [];
    }


    public ScannedDirectory? Build(IProgressDialogService? progressDialog = null)
    {
        progressDialog?.SetInfoText(
            "Scanning directory...",
            Path
        );

        ScanDirectories(progressDialog);
        List<ScannedDirectory?> builtSubdirectories = [];

        for (int i = 0; i < Subdirectories.Count; i++)
        {
            builtSubdirectories.Add(Subdirectories[i].Build());

            int progress = (int)Math.Round((double)(i + 1) / Subdirectories.Count * 100);
            progressDialog?.ReportProgress(progress);

            if (progressDialog?.IsCancelled ?? false)
                return null;
        }


        return new ScannedDirectory(
            Path,
            [.. builtSubdirectories],
            [.. Files]
        );
    }

    private void ScanDirectories(IProgressDialogService? progressDialog)
    {
        try
        {
            foreach (string subdirectoryPath in Directory.EnumerateDirectories(Path))
            {
                AddSubdirectory(subdirectoryPath);

                if (progressDialog?.IsCancelled ?? false)
                    return;
            }

            foreach (string filePath in Directory.EnumerateFiles(Path))
            {
                AddFile(filePath);

                if (progressDialog?.IsCancelled ?? false)
                    return;
            }
        }
        catch (UnauthorizedAccessException exception)
        {
            // Maybe try get authorization?
            Debug.WriteLine(exception);
        }
    }

    private void AddSubdirectory(string path)
    {
        try
        {
            var subdirectory = new ScannedDirectoryBuilder(path);

            Subdirectories.Add(subdirectory);
        }
        catch
        {
        }
    }

    private void AddFile(string path)
    {
        Files.Add(new ScannedFile(path));
    }
}