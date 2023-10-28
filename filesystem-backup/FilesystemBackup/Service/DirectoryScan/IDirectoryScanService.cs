using FilesystemBackup.Model;
using FilesystemBackup.Service.Dialog;
using System.IO;

namespace FilesystemBackup.Service.DirectoryScan;

public interface IDirectoryScanService
{
    ScannedDirectory? ScanDirectory(string path, IProgressDialogService? progressDialogService = null);
    void RestoreScan(string path, ScannedDirectory directory, IProgressDialogService? progressDialogService = null);
    void SerializeScan(Stream outputStream, ScannedDirectory directory, IProgressDialogService? progressDialogService = null);
    ScannedDirectory? DeserializeScan(Stream inputStream, IProgressDialogService? progressDialog = null);
}