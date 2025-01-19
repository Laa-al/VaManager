﻿using VaManager.Data.Tools;
using VaManager.Models;
using VaManager.Models.Basic;

namespace VaManager.Data.Mods;

public class ModCreatorFilter :ViewModelBase, IFilterDescriptor<ModDescriptor>
{
    private string _filterValue = string.Empty;

    public string FilterValue
    {
        get => _filterValue; set
        {
            if (SetProperty(ref _filterValue, value))
            {
                ModModel.Instance.RefreshModList();
            }
        }
    }

    public  bool Filter(ModDescriptor item)
    {
        return    item.CreatorName.Contains(FilterValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public  bool Enabled => !string.IsNullOrEmpty(FilterValue);
}