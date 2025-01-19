using VaManager.Data.Tools;
using VaManager.Models.Basic;

namespace VaManager.Data.Files;

public class FileDefaultFilter : ViewModelBase,IFilterDescriptor<FileDescriptor>
{
    public  bool Filter(FileDescriptor item)
    {
        return item.Mod is null || item.Mod.IsSelected;
    }

    public  bool Enabled => true;
}