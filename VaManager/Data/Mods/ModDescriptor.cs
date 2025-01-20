using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;
using VaManager.Data.Files;
using VaManager.Extensions;
using VaManager.Models;
using VaManager.Models.Basic;

namespace VaManager.Data.Mods;

public class ModDescriptor(string modPath, JsonObject metadata, string[] entries)
    : ViewModelBase
{
    private readonly List<FileDescriptor> _files = [];
    private string[]? _contentList;
    private string? _packageName;
    private string? _creatorName;
    private string? _programVersion;
    private string? _fileName;
    private bool _isSelected = true;
    private List<ModDescriptor>? _modGroup;
    public string ModPath { get; set; } = modPath;
    public string FileName => _fileName ??= Path.GetFileName(ModPath);
    public long Length { get; init; }
    public string LengthDesc => NumberExtensions.GetFileLengthDesc(Length);
    public JsonObject MetaData { get; set; } = metadata;
    public string[] Entries { get; set; } = entries;
    public string[] ContentList => _contentList ??= MetaData["contentList"].ToStringArray();
    public string PackageName => _packageName ??= GetString("packageName");
    public string ProgramVersion => _programVersion ??= GetString("programVersion");
    public string CreatorName => _creatorName ??= GetString("creatorName");
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

    public string Guid => $"{PackageName};{CreatorName}";

    private string GetString(string key)
    {
        return MetaData[key]?.ToString() ?? string.Empty;
    }

    internal static void MoveFile(ModDescriptor? form, ModDescriptor? to, FileDescriptor file)
    {
        form?._files.Remove(file);
        to?._files.Add(file);
    }
}