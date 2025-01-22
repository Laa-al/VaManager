using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using Serilog;
using VaManager.Data.Mods;
using VaManager.Extensions;
using VaManager.Models;
using VaManager.Resources;
using VaManager.Services;

namespace VaManager.Data.Files;

public class FileDescriptor : ItemDescriptor
{
    private FileDescriptor(string name, FileInfo fileInfo)
        : base(name)
    {
        Length = fileInfo.Length;
        CompressedLength = fileInfo.Length;
    }

    private FileDescriptor(string name, ZipArchiveEntry entry)
        : base(name)
    {
        Length = entry.Length;
        CompressedLength = entry.CompressedLength;
    }

    #region Properties

    public long Length { get; set; }
    public long CompressedLength { get; set; }

    #endregion

    #region Calculated properties

    private string? _type;
    public bool IsModFile => Mod is not null;
    public string PathWithoutRoot => Path[(FileManager.RootPathName.Length + 1)..];
    public override string Path => $"{Folder?.Path}{Name}";
    public override string Type => _type ??= System.IO.Path.GetExtension(Name).ToLower();
    public override string Description => Mod?.PackageName ?? "未知";
    public override string LengthDesc => NumberExtensions.GetFileLengthDesc(Length);
    public override string CompressedLengthDesc => NumberExtensions.GetFileLengthDesc(CompressedLength);
    public override bool DefaultVisibility => FileModel.Instance.IgnoreUserFilter.Filter(this);

    #endregion

    #region Connect properties

    private FolderDescriptor? _folder;
    private ModDescriptor? _mod;

    public FolderDescriptor? Folder
    {
        get => _folder;
        set
        {
            FolderDescriptor.MoveFile(_folder, value, this);
            _folder = value;
        }
    }

    public ModDescriptor? Mod
    {
        get => _mod;
        set
        {
            ModDescriptor.MoveFile(_mod, value, this);
            _mod = value;
        }
    }

    #endregion

    #region Cached properties

    private byte[]? _data;

    public byte[]? Data
    {
        get
        {
            if (Length > ConfigModel.ConfigInstance.MaxMemoryLength * 1024)
                return null;

            if (_data is null)
            {
                _data = [];
                var path = EnsureFileExist();
                if (path is not null)
                {
                    using var stream = File.OpenRead(path);
                    _data = new byte[stream.Length];
                    stream.ReadExactly(_data, 0, _data.Length);
                }
            }

            return _data;
        }
    }

    public override byte[]? Preview
    {
        get
        {
            return Type switch
            {
                ".png" or ".jpg" or ".jpeg" or ".bmp" or ".tiff" or ".tif" or ".gif" or ".ico" => Data,
                _ => null
            };
        }
    }

    #endregion

    #region Function

    public void OpenLocalFile()
    {
        var file = EnsureFileExist();

        if (file is not null)
        {
            FileManager.OpenFileOrFolder(file);
        }
    }

    private static readonly ConcurrentDictionary<string, Task<string?>> ZipTasks = new();

    /// <summary>
    /// ensure file or cache (for mod) exist,
    /// if not, create cache or return null.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public string? EnsureFileExist()
    {
        if (_mod is null)
        {
            var mainPath = ConfigModel.ConfigInstance.MainFolderPath;
            var filePath = PathWithoutRoot;
            return System.IO.Path.Combine(mainPath, filePath).Replace('/', '\\');
        }

        var path = System.IO.Path.Combine(
            GlobalResources.GetFileCacheFolder(),
            _mod.FileName, PathWithoutRoot).Replace('/', '\\');
        if (File.Exists(path)) return path;

        if (ZipTasks.TryGetValue(path, out var task))
        {
            task.Wait();
            return task.Result;
        }

        task = new Task<string?>(() =>
        {
            using var archive = ZipFile.OpenRead(_mod.ModPath);
            var entry = archive.GetEntry(PathWithoutRoot);
            if (entry is null) return null;
            using var stream = entry.Open();
            var bytes = stream.ReadAllBytes();
            var folder = System.IO.Path.GetDirectoryName(path);
            if (folder is null) return null;
            Directory.CreateDirectory(folder);
            using var fs = File.OpenWrite(path);
            fs.Write(bytes, 0, bytes.Length);
            ZipTasks.Remove(path, out _);
            return path;
        });

        if (!ZipTasks.TryAdd(path, task))
        {
            // task confilict
            Log.Error($"File {path} already exists.");
            return null;
        }
        
        task.Start();
        task.Wait();


        return path;
    }


    public static FileDescriptor? CreateFromZipArchiveEntry(ZipArchiveEntry? entry)
    {
        if (entry is null) return null;
        if (entry.Length == 0) return null;
        var fileDescriptor = new FileDescriptor(entry.Name, entry);
        return fileDescriptor;
    }

    public static FileDescriptor? CreateFromFileInfo(FileInfo? fileInfo)
    {
        if (fileInfo is null) return null;
        if (fileInfo.Length == 0) return null;
        var fileDescriptor = new FileDescriptor(fileInfo.Name, fileInfo);
        return fileDescriptor;
    }

    #endregion
}