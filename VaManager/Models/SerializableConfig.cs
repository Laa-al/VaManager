using System.Collections.ObjectModel;
using VaManager.Models.Basic;

namespace VaManager.Models;

public class SerializableConfig:ViewModelBase
{
    private string _mainFolderPath = string.Empty;
    private long _maxMemoryLength = 1024;

    public string MainFolderPath
    {
        get => _mainFolderPath;
        set => SetProperty(ref _mainFolderPath, value);
    }

    public ObservableCollection<string> ExtraModPaths { get; set; } = [];

    public long MaxMemoryLength
    {
        get => _maxMemoryLength;
        set => SetProperty(ref _maxMemoryLength, value);
    }
}