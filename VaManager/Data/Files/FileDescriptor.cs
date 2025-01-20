using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media.Imaging;
using Serilog;
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
    private BitmapImage? _preview;


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

    public override BitmapImage? Preview
    {
        get
        {
            if (Length > ConfigModel.ConfigInstance.MaxImageLength * 1024)
            {
                return null;
            }

            if (_preview is null)
            {
                AutoSetPreview();
            }

            return _preview;
        }
    }

    public override bool DefaultVisibility => FileModel.Instance.IgnoreUserFilter.Filter(this);

    public override string Description => Mod?.PackageName ?? "未知";

    #region Function

    private static readonly ConcurrentDictionary<string, FileDescriptor> Cache = [];

    private void AutoSetPreview()
    {
        Application.Current.Dispatcher.BeginInvoke(() => { _preview = GlobalResources.Instance.DefaultImage; });
        switch (Type)
        {
            case "png":
            case "jpg":
            {
                UsingStream(s =>
                {
                    var bytes = s.ReadAllBytes();
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = new MemoryStream(bytes);
                        image.EndInit();
                        SetProperty(ref _preview, image, nameof(Preview));
                    });
                });

                break;
            }
        }
    }

    public void OpenLocalFile()
    {
        if (Mod is null)
        {
            var mainPath = ConfigModel.ConfigInstance.MainFolderPath;
            var filePath = Path["/root/".Length..];
            var path = System.IO.Path.Combine(mainPath, filePath);
            FileManager.OpenFileOrFolder(path);
            return;
        }

        Task.Run(OpenFileInMod);
    }

    private void OpenFileInMod()
    {
        var environment = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var path = System.IO.Path.Combine(environment, "AppData/Local/Temp",
            GlobalResources.CachePath, Mod!.FileName, Path[1..]);
        if (File.Exists(path))
        {
            FileManager.OpenFileOrFolder(path);
            return;
        }

        if (Cache.ContainsKey(path)) return;

        var folder = System.IO.Path.GetDirectoryName(path);
        if (folder is null) return;
        if (!FileManager.EnsureFolderExist(folder)) return;

        Cache.TryAdd(path, this);

        try
        {
            UsingStream(s =>
            {
                File.WriteAllBytes(path, s.ReadAllBytes());
                FileManager.OpenFileOrFolder(path);
            });
        }
        catch (Exception e)
        {
            Log.Error(e, e.Message);
        }

        Cache.Remove(path, out _);
    }

    public void UsingStream(Action<Stream> action)
    {
        var pathWithoutRoot = Path["/root/".Length..];
        if (_mod is null)
        {
            var path = System.IO.Path.Combine(
                ConfigModel.ConfigInstance.MainFolderPath, pathWithoutRoot);

            if (!File.Exists(path))
            {
                Log.Warning($"File {pathWithoutRoot} does not exist");
                return;
            }

            using var stream = File.OpenRead(path);
            action(stream);
        }
        else
        {
            if (!File.Exists(_mod.ModPath))
            {
                Log.Warning($"File {_mod.ModPath} does not exist!");
                return;
            }

            using var archive = ZipFile.OpenRead(_mod.ModPath);
            var entry = archive.GetEntry(pathWithoutRoot);
            if (entry is not null)
            {
                using var stream = entry.Open();
                action(stream);
            }
            else
            {
                Log.Warning($"File {pathWithoutRoot} does not exist in mod!");
            }
        }
    }

    #endregion
}