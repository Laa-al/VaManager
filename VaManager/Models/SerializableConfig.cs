using System.Collections.ObjectModel;
using VaManager.Models.Basic;

namespace VaManager.Models;

public class SerializableConfig:ViewModelBase
{
    private string _mainFolderPath = string.Empty;
    private long _maxImageLength = 1024;

    public string MainFolderPath
    {
        get => _mainFolderPath;
        set => SetProperty(ref _mainFolderPath, value);
    }

    public ObservableCollection<string> ExtraModPaths { get; set; } = [];

    public long MaxImageLength
    {
        get => _maxImageLength;
        set => SetProperty(ref _maxImageLength, value);
    }
}