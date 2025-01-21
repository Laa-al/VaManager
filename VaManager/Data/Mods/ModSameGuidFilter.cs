using VaManager.Data.Tools;
using VaManager.Models;
using VaManager.Models.Basic;

namespace VaManager.Data.Mods;

public class ModSameGuidFilter : ViewModelBase, IFilter<ModDescriptor>
{
    private bool _enabled;

    public bool Filter(ModDescriptor item)
    {
        return (item.ModGroup?.Count ?? 0) > 1;
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            SetProperty(ref _enabled, value);
            ModModel.Instance.RefreshModList();
        }
    }
}