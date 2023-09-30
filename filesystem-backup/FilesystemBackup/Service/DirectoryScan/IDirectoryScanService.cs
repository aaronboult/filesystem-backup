using FilesystemBackup.Model;
using FilesystemBackup.Service.Dialog;

namespace FilesystemBackup.Service.DirectoryScan;

public interface IDirectoryScanService
{
    ScannedDirectory? ScanDirectory(string path, IProgressDialogService? progressDialogService = null);
    void RestoreScan(string path, ScannedDirectory directory, IProgressDialogService? progressDialogService = null);
    byte[] SerializeScan(ScannedDirectory directory, IProgressDialogService? progressDialogService = null);
    ScannedDirectory? DeserializeScan(byte[] bytes, IProgressDialogService? progressDialog = null);
}