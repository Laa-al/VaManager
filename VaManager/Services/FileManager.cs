using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.VisualBasic.FileIO;
using Serilog;
using VaManager.Data;
using VaManager.Data.Files;
using VaManager.Data.Mods;
using VaManager.Models;
using VaManager.Resources;

namespace VaManager.Services;

public class FileManager
{
    public const string RootPathName = "root";
    public static FileManager Instance { get; } = new();

    private readonly FolderDescriptor _rootFolder = new(RootPathName);
    private readonly List<FileDescriptor> _fileDescriptors = [];
    private readonly List<ModDescriptor> _modDescriptors = [];

    public IReadOnlyList<ModDescriptor> ModDescriptors => _modDescriptors;
    public IReadOnlyList<FileDescriptor> FileDescriptors => _fileDescriptors;


    private Queue<string> GetFolderNames(string path)
    {
        var index = -1;
        Queue<string> folderNames = [];
        for (var i = 0; i < path.Length; i++)
        {
            if (path[i] is not ('/' or '\\')) continue;
            Enqueue(index + 1, i);
            index = i;
        }

        Enqueue(index + 1, path.Length);

        if (folderNames.FirstOrDefault() == RootPathName)
        {
            folderNames.Dequeue();
        }

        return folderNames;

        void Enqueue(int start, int end)
        {
            if (end - start > 0)
            {
                folderNames.Enqueue(path[start..end]);
            }
        }
    }

    private FileDescriptor? AddFile(string path, ModDescriptor? mod = null)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory is null) return null;
        var file = Path.GetFileName(path);
        if (file == "meta.json") return null;
        var folderNames = GetFolderNames(directory);
        var folder = _rootFolder.GetByPath(folderNames, true)!;
        var fileDescriptor = new FileDescriptor(file)
        {
            Folder = folder,
            Mod = mod
        };
        _fileDescriptors.Add(fileDescriptor);

        return fileDescriptor;
    }

    public void AddFromMod(string modPath)
    {
        if (_modDescriptors.Any(u => u.ModPath == modPath))
        {
            return;
        }


        try
        {
            if (!File.Exists(modPath)) return;
            var fileInfo = new FileInfo(modPath);
            using var archive = ZipFile.OpenRead(modPath);
            var meta = archive.GetEntry("meta.json");
            if (meta is null)
            {
                Log.Warning($"Not find meta.json in mod: {modPath}.");
                return;
            }

            using var stream = meta.Open();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var jsonNode = JsonNode.Parse(json, null, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
            });
            var entries = archive.Entries
                .Where(u => u.Length > 0)
                .ToArray();

            if (jsonNode is not JsonObject metadata)
            {
                Log.Warning($"meta.json deserialized failed: {modPath}.");
                return;
            }

            var modDescriptor = new ModDescriptor(modPath, metadata, entries.Select(u => u.FullName).ToArray())
            {
                Length = fileInfo.Length,
            };

            foreach (var entry in entries)
            {
                var file = AddFile(entry.FullName, modDescriptor);
                if (file is null) continue;
                file.Length = entry.Length;
                file.CompressedLength = entry.CompressedLength;
            }

            var modGroup = _modDescriptors
                .FirstOrDefault(u => u.Guid == modDescriptor.Guid)?.ModGroup ?? [];

            modDescriptor.ModGroup = modGroup;
            _modDescriptors.Add(modDescriptor);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load mod.");
        }
    }

    public void AddFromFolderWithMod(string folderPath)
    {
        var files = Directory.GetFiles(folderPath);
        foreach (var file in files)
        {
            var suffix = Path.GetExtension(file);
            if (suffix.Length > 1 &&
                GlobalResources.Instance.ModSuffix.Contains(suffix[1..]))
            {
                AddFromMod(file);
            }
        }

        var folders = Directory.GetDirectories(folderPath);
        foreach (var folder in folders)
        {
            AddFromFolderWithMod(folder);
        }
    }

    public void AddFromFolderDirectly(string folderPath)
    {
        var files = Directory.GetFiles(folderPath);
        var len = ConfigModel.ConfigInstance.MainFolderPath.Length;
        foreach (var file in files)
        {
            var descriptor = AddFile(file[len..]);
            if (descriptor is null) continue;
            var fileInfo = new FileInfo(file);
            descriptor.Length = fileInfo.Length;
            descriptor.CompressedLength = fileInfo.Length;
        }

        var folders = Directory.GetDirectories(folderPath);
        foreach (var folder in folders)
        {
            AddFromFolderDirectly(folder);
        }
    }

    public void ClearAllFiles()
    {
        _fileDescriptors.Clear();
        _modDescriptors.Clear();
        _rootFolder.Clear();
    }

    public FolderDescriptor? GetFolderDescriptor(string path)
    {
        var folderNames = GetFolderNames(path);
        return _rootFolder.GetByPath(folderNames);
    }

    public void DeleteMod(ModDescriptor modDescriptor)
    {
        _modDescriptors.Remove(modDescriptor);
        foreach (var file in modDescriptor.Files)
        {
            file.Folder = null;
            _fileDescriptors.Remove(file);
        }

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
            AddFromFolderDirectly(folder);
        }

        foreach (var path in config.ExtraModPaths)
        {
            AddFromFolderWithMod(path);
        }

        ModModel.Instance.RefreshModList();
        FileModel.Instance.RefreshFileList();
    }

    #region Static Function

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

    public static bool EnsureFolderExist(string folderPath)
    {
        if (Directory.Exists(folderPath)) return true;
        var parentFolderPath = Path.GetDirectoryName(folderPath);
        if (parentFolderPath is null) return false;
        if (!EnsureFolderExist(parentFolderPath)) return false;
        EnsureFolderExist(parentFolderPath);
        Directory.CreateDirectory(folderPath);
        return true;
    }

    #endregion
}