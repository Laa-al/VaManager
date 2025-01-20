using System.IO;
using System.IO.Compression;
using VaManager.Data.Mods;
using VaManager.Extensions;
using VaManager.Models;
using VaManager.Resources;
using VaManager.Services;

namespace VaManager.Data.Files;

public class FileDescriptor(string name) : ItemDescriptor(name)
{
    private FolderDescriptor? _folder;
    private ModDescriptor? _mod;
    private byte[]? _data;

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

    public long Length { get; set; }
    public override string LengthDesc => NumberExtensions.GetFileLengthDesc(Length);
    public long CompressedLength { get; set; }
    public override string CompressedLengthDesc => NumberExtensions.GetFileLengthDesc(CompressedLength);
    public bool IsModFile => Mod is not null;
    public override string Path => $"{Folder?.Path}/{Name}";
    public override string Type => Name[(Name.LastIndexOf('.') + 1)..];
    public string PathWithoutRoot => Path[(FileManager.RootPathName.Length + 2)..];

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
                "png" or "jpg" or "jpeg" or "bmp" or "tiff"  or "tif" or "gif" or "ico" => Data,
                _ => null
            };
        }
    }

    public override bool DefaultVisibility => FileModel.Instance.IgnoreUserFilter.Filter(this);

    public override string Description => Mod?.PackageName ?? "未知";

    #region Function

    private string GetCacheFilePath()
    {
        if (_mod is null) return "";
        var userFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var path = System.IO.Path.Combine(userFolderPath, "AppData/Local/Temp",
            GlobalResources.CachePath, _mod.FileName, Path[1..]);
        return path;
    }

    public void OpenLocalFile()
    {
        var file = EnsureFileExist();

        if (file is not null)
        {
            FileManager.OpenFileOrFolder(file);
        }
    }

    /// <summary>
    /// ensure file or cache (for mod) exist,
    /// if not, create cache or return null.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public string? EnsureFileExist()
    {
        string path;
        if (_mod is null)
        {
            var mainPath = ConfigModel.ConfigInstance.MainFolderPath;
            var filePath = Path["/root/".Length..];
            path = System.IO.Path.Combine(mainPath, filePath);
        }
        else
        {
            path = GetCacheFilePath();
            if (!File.Exists(path))
            {
                using var archive = ZipFile.OpenRead(_mod.ModPath);
                var entry = archive.GetEntry(PathWithoutRoot);
                if (entry is null) return null;
                using var stream = entry.Open();
                var bytes = stream.ReadAllBytes();
                var folder = System.IO.Path.GetDirectoryName(path);
                if (folder is null) return null;
                FileManager.EnsureFolderExist(folder);
                using var fs = File.OpenWrite(path);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        return path;
    }

    #endregion
}