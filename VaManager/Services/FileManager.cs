using System.IO;
using Microsoft.VisualBasic.FileIO;
using Serilog;
using VaManager.Data;
using VaManager.Data.Files;
using VaManager.Data.Mods;
using VaManager.Models;
using VaManager.Resources;
using SearchOption = System.IO.SearchOption;

namespace VaManager.Services;

public class FileManager
{
    public const string RootPathName = "@root";
    public static FileManager Instance { get; } = new();

    private readonly FolderDescriptor _rootFolder = new(RootPathName);
    private readonly List<FileDescriptor> _fileDescriptors = [];
    private readonly List<ModDescriptor> _modDescriptors = [];

    public IReadOnlyList<ModDescriptor> ModDescriptors => _modDescriptors;
    public IReadOnlyList<FileDescriptor> FileDescriptors => _fileDescriptors;

    #region Analyze Method

    private void AddMod(string filePath)
    {
        if (_modDescriptors
            .Any(u => u.ModPath == filePath))
        {
            Log.Warning($"Duplicate mod: {filePath}, not load repeatedly.");
            return;
        }

        try
        {
            var mod = ModDescriptor.CreateFromLocalPath(filePath, _rootFolder);
            if (mod is null)
            {
                Log.Warning($"Mod load file: {filePath}.");
                return;
            }

            var modGroup = _modDescriptors
                .FirstOrDefault(u => u.Guid == mod.Guid)
                ?.ModGroup ?? [];
            mod.ModGroup = modGroup;

            foreach (var fileDescriptor in mod.Files)
            {
                _fileDescriptors.Add(fileDescriptor);
            }

            _modDescriptors.Add(mod);
        }
        catch (Exception e)
        {
            Log.Error(e, $"Mod load file: {filePath}.");
        }
    }

    private static readonly string[] SearchPattern = [".var", ".rar"];

    private void AddModsFromFolder(string folderPath)
    {
        var files = Directory.GetFiles(folderPath,
            "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var suffix = Path.GetExtension(file);
            if (SearchPattern.Contains(suffix, StringComparer.OrdinalIgnoreCase))
                AddMod(file);
        }
    }

    private void AddFile(string filePath, int prefixLength)
    {
        var fileInfo = new FileInfo(filePath);
        var fileDescriptor = FileDescriptor.CreateFromFileInfo(fileInfo);
        if (fileDescriptor is null || fileInfo.DirectoryName is null)
        {
            Log.Warning($"File {filePath} was not found.");
            return;
        }

        var folderPath = fileInfo.DirectoryName[prefixLength..];
        var folder = _rootFolder.GetChildByFolderPath(folderPath, true);
        fileDescriptor.Folder = folder;
        _fileDescriptors.Add(fileDescriptor);
    }

    private void AddFilesFromFolder(string folderPath, int prefixLength)
    {
        var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            AddFile(file, prefixLength);
        }
    }

    private void ClearAllFiles()
    {
        _fileDescriptors.Clear();
        _modDescriptors.Clear();
        _rootFolder.Clear();
    }

    #endregion

    public FolderDescriptor? GetFolderDescriptor(string path, bool createIfNotExists = false)
    {
        var startIndex = path.IndexOf(RootPathName, StringComparison.OrdinalIgnoreCase);
        var folderPath = startIndex < 0 ? path : path[(startIndex + RootPathName.Length)..];
        var folder = _rootFolder.GetChildByFolderPath(folderPath, createIfNotExists);
        return folder;
    }

    public void DeleteMod(ModDescriptor modDescriptor)
    {
        modDescriptor.ModGroup = null;
        foreach (var file in modDescriptor.Files)
        {
            file.Folder = null;
            _fileDescriptors.Remove(file);
        }

        _modDescriptors.Remove(modDescriptor);

        FileSystem.DeleteFile(modDescriptor.ModPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        ModModel.Instance.RefreshModList();
        FileModel.Instance.RefreshFileList();
    }

    public void ReanalyzeFromConfig()
    {
        ClearAllFiles();

        var config = ConfigModel.ConfigInstance;
        var folder = Path.Combine(config.MainFolderPath, "Custom");

        if (Directory.Exists(folder))
        {
            var len = ConfigModel.ConfigInstance.MainFolderPath.Length;
            AddFilesFromFolder(folder, len);
        }

        foreach (var path in config.ExtraModPaths)
        {
            AddModsFromFolder(path);
        }

        ModModel.Instance.RefreshModList();
        FileModel.Instance.RefreshFileList();
    }

    #region Static Function

    public static void ClearLocalFileCache()
    {
        var cachePath = GlobalResources.GetFileCacheFolder();
        Directory.Delete(cachePath, true);
    }

    public static void OpenFileOrFolder(string path)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
        {
            Arguments = path.Replace('/', '\\')
        };
        System.Diagnostics.Process.Start(psi);
    }

    public static void OpenFolderAndSelectFile(string path)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
        {
            Arguments = "/e,/select," + path.Replace('/', '\\')
        };
        System.Diagnostics.Process.Start(psi);
    }

    #endregion
}