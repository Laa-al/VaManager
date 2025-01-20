using VaManager.Data.Tools;
using VaManager.Models;
using VaManager.Models.Basic;

namespace VaManager.Data.Files;

public enum FileIgnoreUserFilterMode
{
    不忽略任何项 = 0,
    仅筛选用户项 = 1,
    仅筛选Mod项 = 2
}

public class FileIgnoreUserFilter : ViewModelBase, IFilterDescriptor<FileDescriptor>
{
    public static FileIgnoreUserFilterMode[] Modes { get; } =
    [
        FileIgnoreUserFilterMode.不忽略任何项, FileIgnoreUserFilterMode.仅筛选用户项, FileIgnoreUserFilterMode.仅筛选Mod项
    ];

    private FileIgnoreUserFilterMode _mode;

    public bool Filter(FileDescriptor item)
    {
        return Mode switch
        {
            FileIgnoreUserFilterMode.不忽略任何项 => item.Mod is null || item.Mod.IsSelected,
            FileIgnoreUserFilterMode.仅筛选Mod项 => item.Mod is not null && item.Mod.IsSelected,
            FileIgnoreUserFilterMode.仅筛选用户项 => item.Mod is null,
            _ => true
        };
    }

    public FileIgnoreUserFilterMode Mode
    {
        get => _mode;
        set
        {
            if (SetProperty(ref _mode, value))
            {
                FileModel.Instance.RefreshFileList();
            }
        }
    }

    public bool Enabled => true;
}