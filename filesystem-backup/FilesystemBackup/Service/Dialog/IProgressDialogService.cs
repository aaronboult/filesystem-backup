using System;
using System.Threading.Tasks;

namespace FilesystemBackup.Service.Dialog;

public interface IProgressDialogSettings
{
    public string? Title { get; }
    public string? Message { get; }
    public string? Description { get; }
    public Action? Finalizer { get; }
    public Action<IProgressDialogService>? Worker { get; }
    public Func<IProgressDialogService, Task>? WorkerAsync { get; }
}

public record struct ProgressDialogSettings : IProgressDialogSettings
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Description { get; set; }
    public Action? Finalizer { get; set; }
    public Action<IProgressDialogService>? Worker { get; set; }
    public Func<IProgressDialogService, Task>? WorkerAsync { get; set; }
}

public interface IProgressDialogService
{
    public bool IsCancelled { get; }
    void ReportProgress(int percentage);
    void SetAllText(string title, string message, string description);
    void SetInfoText(string message, string description);
    void SetTitle(string title);
    Task Show(ProgressDialogSettings settings);
}