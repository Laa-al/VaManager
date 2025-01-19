using System.Collections.ObjectModel;
using VaManager.Data;
using VaManager.Data.Files;
using VaManager.Data.Mods;
using VaManager.Data.Tools;
using VaManager.Extensions;
using VaManager.Models.Basic;
using VaManager.Services;

namespace VaManager.Models;

public sealed class FileModel : ViewModelBase<FileModel>
{
    private const int MaxHistory = 10;
    private string _explorerPath = string.Empty;
    private int _historyIndex;
    private FolderDescriptor? _folderOpen = FileManager.Instance.GetFolderDescriptor("")!;
    private ItemDescriptor? _itemSelect;
    private FileDescriptor? _fileSelect;
    private bool _flatView;


    public FileModel()
    {
        FileFilters =
        [
            new FileDefaultFilter(),
            NameFilter,
            TypeFilter,
            IgnoreUserFilter,
            DescriptionFilter
        ];
    }

    public FolderDescriptor? FolderOpen
    {
        get => _folderOpen;
        set
        {
            if (value == _folderOpen || value == null)
            {
                ExplorerPath = _folderOpen?.Path ?? ExplorerPath;
                return;
            }

            _folderOpen = value;
            ExplorerPath = _folderOpen.Path;
            RefreshFileList();
        }
    }

    public FileDescriptor? FileSelect
    {
        get => _fileSelect;
        set => SetProperty(ref _fileSelect, value);
    }

    public ItemDescriptor? ItemSelect
    {
        get => _itemSelect;
        set
        {
            if (!SetProperty(ref _itemSelect, value)) return;
            switch (value)
            {
                case FileDescriptor file:
                    FileSelect = file;
                    break;
                case FolderDescriptor folder:
                    break;
            }
        }
    }

    public string ExplorerPath
    {
        get => _explorerPath;
        set => SetProperty(ref _explorerPath, value);
    }

    public int HistoryIndex
    {
        get => _historyIndex;
        set => SetProperty(ref _historyIndex, value);
    }


    public ObservableCollection<ItemDescriptor> ItemList { get; } = [];
    public List<FolderDescriptor> HistoryFolder { get; } = [];

    public IFilterDescriptor<FileDescriptor>[] FileFilters { get; }

    #region Filters

    public FileNameFilter NameFilter { get; } = new();
    public FileTypeFilter TypeFilter { get; } = new();
    public FileDescriptionFilter DescriptionFilter { get; } = new();

    public FileIgnoreUserFilter IgnoreUserFilter { get; } = new();

    #endregion

    public bool FlatView
    {
        get => _flatView;
        set
        {
            SetProperty(ref _flatView, value);
            RefreshFileList();
        }
    }

    public void RefreshFileList()
    {
        ItemList.Clear();
        if (FlatView)
        {
            foreach (var file in FileManager.Instance.FileDescriptors.Filter(FileFilters))
            {
                ItemList.Add(file);
            }
        }
        else if (FolderOpen is not null)
        {
            foreach (var folder in FolderOpen.Children)
            {
                ItemList.Add(folder);
            }

            foreach (var file in FolderOpen.Files.Filter(FileFilters))
            {
                ItemList.Add(file);
            }
        }

        OnPropertyChanged(nameof(ItemList));
    }

    private void AddHistory(FolderDescriptor folder)
    {
        if (HistoryFolder.Count >= MaxHistory)
        {
            HistoryFolder.RemoveAt(HistoryFolder.Count - 1);
        }

        HistoryFolder.Insert(0, folder);
    }

    public void NavigateBefore()
    {
        HistoryIndex = Math.Min(HistoryFolder.Count - 1, HistoryIndex + 1);
        NavigateToHistory();
    }

    public void NavigateNext()
    {
        HistoryIndex = Math.Max(0, HistoryIndex - 1);
        NavigateToHistory();
    }

    public void NavigateToHistory()
    {
        if (HistoryFolder.Count > HistoryIndex && 0 < HistoryIndex)
        {
            NavigateTo(HistoryFolder[HistoryIndex], false);
        }
    }

    public void NavigateTo(FolderDescriptor? folder, bool addHistory = true)
    {
        if (folder != null && addHistory)
        {
            AddHistory(folder);
            HistoryIndex = 0;
        }

        FolderOpen = folder;
    }

    public void NavigateToPath(string folderPath, bool addHistory = true)
    {
        var folder = FileManager.Instance.GetFolderDescriptor(folderPath);
        NavigateTo(folder, addHistory);
    }
}