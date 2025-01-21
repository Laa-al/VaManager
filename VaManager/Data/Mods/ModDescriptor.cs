using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;
using VaManager.Data.Files;
using VaManager.Extensions;
using VaManager.Models;
using VaManager.Models.Basic;
using VaManager.Services;

namespace VaManager.Data.Mods;

public class ModDescriptor : ViewModelBase
{
    private ModDescriptor(string modPath, FileInfo fileInfo, JsonObject metadata)
    {
        ModPath = modPath;
        Length = fileInfo.Length;
        MetaData = metadata;
    }

    #region Properties

    private string? _fileName;
    private readonly List<FileDescriptor> _files = [];
    private List<ModDescriptor>? _modGroup;

    public string ModPath { get; }
    public long Length { get; }
    public string FileName => _fileName ??= Path.GetFileName(ModPath);
    public IReadOnlyList<FileDescriptor> Files => _files;

    public List<ModDescriptor>? ModGroup
    {
        get => _modGroup;
        set
        {
            _modGroup?.Remove(this);
            _modGroup = value;
            _modGroup?.Add(this);
        }
    }

    #endregion

    #region Meta

    private string[]? _contentList;
    private string? _packageName;
    private string? _programVersion;
    private string? _creatorName;

    public JsonObject MetaData { get; }
    public string[] ContentList => _contentList ??= MetaData["contentList"].ToStringArray();
    public string PackageName => _packageName ??= GetStringFromMeta("packageName");
    public string ProgramVersion => _programVersion ??= GetStringFromMeta("programVersion");
    public string CreatorName => _creatorName ??= GetStringFromMeta("creatorName");
    public string Guid => $"{PackageName};{CreatorName}";

    #endregion

    #region UI

    private bool _isSelected = true;

    public string LengthDesc => NumberExtensions.GetFileLengthDesc(Length);

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                ModModel.Instance.OnPropertyChanged(nameof(ModModel.IsAllItemsSelected));
            }
        }
    }

    public bool IsSelectedWithNotification
    {
        get => IsSelected;
        set
        {
            IsSelected = value;
            FileModel.Instance.RefreshFileList();
        }
    }

    #endregion

    #region Functions

    private string GetStringFromMeta(string key)
    {
        return MetaData[key]?.ToString() ?? string.Empty;
    }

    internal static void MoveFile(ModDescriptor? form, ModDescriptor? to, FileDescriptor file)
    {
        form?._files.Remove(file);
        to?._files.Add(file);
    }

    public static ModDescriptor? CreateFromLocalPath(
        string filePath, FolderDescriptor rootFolder)
    {
        if (!File.Exists(filePath))
        {
            Log.Warning($"File {filePath} does not exist.");
            return null;
        }

        using var archive = ZipFile.OpenRead(filePath);
        var metaEntry = archive.GetEntry("meta.json");
        if (metaEntry is null)
        {
            Log.Warning($"Not find meta.json in mod: {filePath}.");
            return null;
        }

        using var stream = metaEntry.Open();
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        if (JsonNode.Parse(json, null, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
            }) is not JsonObject metaData)
        {
            Log.Warning($"meta.json deserialized failed: {filePath}.");
            return null;
        }

        List<FileDescriptor> files = [];

        foreach (var entry in archive.Entries)
        {
            var fileDescriptor = FileDescriptor
                .CreateFromZipArchiveEntry(entry);
            var directory = Path.GetDirectoryName(entry.FullName);
            if (fileDescriptor is null ||
                fileDescriptor.Name == "meta.json" ||
                directory is null) continue;
            var folderDescriptor = rootFolder
                .GetChildByFolderPath(directory, true);
            fileDescriptor.Folder = folderDescriptor;
            files.Add(fileDescriptor);
        }

        var fileInfo = new FileInfo(filePath);
        var modDescriptor = new ModDescriptor(filePath, fileInfo, metaData);

        foreach (var file in files)
        {
            file.Mod = modDescriptor;
        }

        return modDescriptor;
    }

    #endregion
}