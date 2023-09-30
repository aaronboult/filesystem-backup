using Ookii.Dialogs.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FilesystemBackup.Service.Dialog;

public class ProgressDialogService : IProgressDialogService
{
    private ProgressDialog ProgressDialog;
    private readonly IDialogService DialogService;

    private int CurrentProgress = 0;
    private bool DialogIsRunning = false;

    private Action<IProgressDialogService>? WorkerSync = null;
    private Func<IProgressDialogService, Task>? WorkerAsync = null;
    private Action? Finalizer = null;

    public bool IsCancelled => ProgressDialog.CancellationPending;


    public ProgressDialogService(IDialogService dialogService)
    {
        ProgressDialog = new ProgressDialog();
        DialogService = dialogService;


        ProgressDialog.DoWork += DoWork;
        ProgressDialog.RunWorkerCompleted += RunWorkerCompleted;
    }


    public void SetTitle(string title)
    {
        ProgressDialog.WindowTitle = title;
    }

    public void SetAllText(string title, string message, string description)
    {
        SetTitle(title);
        SetInfoText(message, description);
    }

    public void SetInfoText(string message, string description)
    {
        if (DialogIsRunning)
        {
            ProgressDialog.ReportProgress(CurrentProgress, message, description);
        }
        else
        {
            ProgressDialog.Text = message;
            ProgressDialog.Description = description;
        }
    }

    public void SetFinalizer(Action finalizer)
    {
        this.Finalizer = finalizer;
    }

    public void ReportProgress(int percentage)
    {
        CurrentProgress = Math.Clamp(percentage, 0, 100);
        Debug.WriteLine($"Reporting progress: {CurrentProgress}");
        ProgressDialog.ReportProgress(CurrentProgress);
    }

    public async Task Show(ProgressDialogSettings settings)
    {
        if (settings.Worker == null && settings.WorkerAsync == null)
            throw new ArgumentException("No worker defined in settings", nameof(settings));

        EnsureDialogNotRunning();
        SetAllText(settings.Title ?? string.Empty, settings.Message ?? string.Empty, settings.Description ?? string.Empty);
        SetFinalizer(settings.Finalizer ?? (() => { }));

        DialogIsRunning = true;

        if (settings.Worker != null)
            WorkerSync = settings.Worker;

        else
            WorkerAsync = settings.WorkerAsync;

        ProgressDialog.ShowDialog();

        // Allows us to wait for the dialog to close
        async Task Waiter()
        {
            Debug.WriteLine("Waiting begins");
            while (DialogIsRunning)
                await Task.Delay(100);
            Debug.WriteLine("Waiting over");
        }

        if (WorkerSync != null)
            await Waiter();
    }

    private void EnsureDialogNotRunning()
    {
        if (DialogIsRunning)
            throw new InvalidOperationException("A dialog is already running");

        if (ProgressDialog.IsBusy)
            throw new InvalidOperationException("A dialog is busy");
    }

    private void DoWork(object? sender, DoWorkEventArgs args)
    {
        if (WorkerSync == null && WorkerAsync == null)
            throw new InvalidOperationException($"No worker defined in {nameof(ProgressDialogService)} instance");
        Debug.WriteLine("Passed exception");

        try
        {
            ProgressDialog.ReportProgress(0);

            if (WorkerSync != null)
                WorkerSync(this);

            else if (WorkerAsync != null)
                WorkerAsync(this).Wait();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            DialogService.ShowError($"An error occurred while processing.\n{ex.Message}");

            return;
        }
    }

    private void RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs args)
    {
        Debug.WriteLine("ProgressDialog.RunWorkerCompleted called");

        Action? localFinalizer = Finalizer;

        DialogIsRunning = false;
        WorkerSync = null;
        WorkerAsync = null;
        Finalizer = null;

        SetAllText(string.Empty, string.Empty, string.Empty);

        localFinalizer?.Invoke();
    }
}
