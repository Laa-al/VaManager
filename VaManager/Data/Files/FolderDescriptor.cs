namespace VaManager.Data.Files;

public class FolderDescriptor(string name) : ItemDescriptor(name)
{
    #region Properties

    private string? _path;
    public override string Path => _path ??= $"{Parent?.Path}{Name}/";

    #endregion

    #region Calculated properties

    public override string Name
    {
        get => base.Name;
        set
        {
            ResetPath();
            base.Name = value;
        }
    }

    public override string Type => "文件夹";
    public override byte[]? Preview => null;

    public override string Description => string.Empty;

    public override bool DefaultVisibility =>
        _files.Any(u => u.DefaultVisibility) ||
        _children.Any(u => u.DefaultVisibility);

    #endregion

    #region Connect properties

    private FolderDescriptor? _parent;
    private readonly List<FolderDescriptor> _children = [];
    private readonly List<FileDescriptor> _files = [];


    public FolderDescriptor? Parent
    {
        get => _parent;
        private set
        {
            _parent?._children.Remove(this);
            value?._children.Add(this);
            _parent = value;
            ResetPath();
        }
    }

    public IReadOnlyList<FolderDescriptor> Children => _children;
    public IReadOnlyList<FileDescriptor> Files => _files;

    #endregion

    #region Functions

    private void ResetPath()
    {
        _path = null;
        foreach (var folder in _children)
        {
            folder.ResetPath();
        }
    }

    public void Clear()
    {
        _children.Clear();
        _files.Clear();
    }

    private static FolderDescriptor? GetByFolderQueue(
        Queue<string> folderNames, FolderDescriptor startFolder, bool createIfNotExists = false)
    {
        var currentFolder = startFolder;

        while (folderNames.Count > 0)
        {
            var folderName = folderNames.Dequeue();
            var child = currentFolder.Children.FirstOrDefault(x => x.Name == folderName);

            if (child is null)
            {
                if (!createIfNotExists) return null;

                child = new FolderDescriptor(folderName)
                {
                    Parent = currentFolder,
                };
            }

            currentFolder = child;
        }

        return currentFolder;
    }


    private Queue<string> SplitFolderPathToQueue(string folderPath)
    {
        var index = -1;
        Queue<string> folderNames = [];
        for (var i = 0; i < folderPath.Length; i++)
        {
            if (folderPath[i] is not ('/' or '\\')) continue;
            Enqueue(index + 1, i);
            index = i;
        }

        Enqueue(index + 1, folderPath.Length);

        return folderNames;

        void Enqueue(int start, int end)
        {
            if (end - start <= 0) return;
            folderNames.Enqueue(folderPath[start..end]);
        }
    }

    public FolderDescriptor? GetChildByFolderPath(string folderPath, bool createIfNotExists = false)
    {
        var folderQueue = SplitFolderPathToQueue(folderPath);
        return GetByFolderQueue(folderQueue, this, createIfNotExists);
    }

    internal static void MoveFile(FolderDescriptor? form, FolderDescriptor? to, FileDescriptor file)
    {
        form?._files.Remove(file);
        to?._files.Add(file);
    }

    #endregion
}