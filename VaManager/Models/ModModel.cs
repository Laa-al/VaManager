using System.Collections.ObjectModel;
using VaManager.Data.Files;
using VaManager.Data.Mods;
using VaManager.Data.Tools;
using VaManager.Extensions;
using VaManager.Models.Basic;
using VaManager.Services;

namespace VaManager.Models;

public class ModModel : ViewModelBase<ModModel>
{
    private ModDescriptor? _modSelect;

    public ModModel()
    {
        ModFilters =
        [
            NameFilter,
            CreatorFilter,
            PathFilter,
            FileNameFilter,
            SameGuidFilter,
        ];
    }

    public ModDescriptor? ModSelect
    {
        get => _modSelect;
        set => SetProperty(ref _modSelect, value);
    }

    public ObservableCollection<ModDescriptor> ModList { get; } = [];
    public IFilter<ModDescriptor>[] ModFilters { get; }

    #region Filters

    public ModNameFilter NameFilter { get; } = new();
    public ModCreatorFilter CreatorFilter { get; } = new();
    public ModSameGuidFilter SameGuidFilter { get; } = new();
    public ModPathFilter PathFilter { get; } = new();

    public ModFileNameFilter FileNameFilter { get; } = new();

    #endregion

    public bool? IsAllItemsSelected
    {
        get
        {
            bool allSelected = true;
            bool allDeselected = true;
            foreach (var mod in ModList)
            {
                if (mod.IsSelected) allDeselected = false;
                else allSelected = false;
            }
            if (!allDeselected && !allSelected) return null;
            return allSelected;
        }
        set
        {
            foreach (var mod in ModList)
            {
                mod.IsSelected = value ?? false;
            }
            FileModel.Instance.RefreshFileList();
            ModModel.Instance.RefreshModList();
        }
    }

    public void RefreshModList()
    {
        ModList.Clear();

        foreach (var mod in FileManager.Instance.ModDescriptors.Filter(ModFilters))
        {
            ModList.Add(mod);
        }

        OnPropertyChanged(nameof(ModList));
    }
}