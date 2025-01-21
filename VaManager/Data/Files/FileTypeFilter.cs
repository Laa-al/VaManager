﻿using VaManager.Data.Files;
using VaManager.Data.Tools;
using VaManager.Models;
using VaManager.Models.Basic;

namespace VaManager.Data.Mods;

public class FileTypeFilter : ViewModelBase, IFilter<FileDescriptor>
{
    private string _filterValue = string.Empty;

    public string FilterValue
    {
        get => _filterValue;
        set
        {
            if (SetProperty(ref _filterValue, value))
            {
                FileModel.Instance.RefreshFileList();
            }
        }
    }

    public bool Filter(FileDescriptor item)
    {
        return item.Type.Contains(FilterValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public bool Enabled => !string.IsNullOrEmpty(FilterValue);
}