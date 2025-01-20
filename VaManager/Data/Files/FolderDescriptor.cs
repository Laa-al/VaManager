using System.Windows.Media.Imaging;
using VaManager.Data.Files;
using VaManager.Resources;

namespace VaManager.Data;

public class FolderDescriptor(string name) : ItemDescriptor(name)
{
    private readonly List<FolderDescriptor> _children = [];
    private readonly List<FileDescriptor> _files = [];
    private FolderDescriptor? _parent;
    private string? _path;

    private void ResetPath()
    {
        _path = null;
        foreach (var folder in _children)
        {
            folder.ResetPath();
        }
    }

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
    public override BitmapImage Preview => GlobalResources.Instance.FolderImage;

    public override bool DefaultVisibility =>
        _files.Any(u => u.DefaultVisibility) ||
        _children.Any(u => u.DefaultVisibility);

    public override string Description => string.Empty;

    public FolderDescriptor? Parent
    {
        get => _parent;
        set
        {
            _parent?._children.Remove(this);
            value?._children.Add(this);
            _parent = value;
            ResetPath();
        }
    }

    public override string Path => _path ??= $"{Parent?.Path}/{Name}";

    public IReadOnlyList<FolderDescriptor> Children => _children;
    public IReadOnlyList<FileDescriptor> Files => _files;


    public void Clear()
    {
        _children.Clear();
        _files.Clear();
    }

    public FolderDescriptor? GetByPath(Queue<string> folderNames, bool createIfNotExists = false)
    {
        if (folderNames.Count == 0) return this;
        var folderName = folderNames.Dequeue();
        var child = _children
            .FirstOrDefault(x => x.Name == folderName);
        if (createIfNotExists && child is null)
        {
            child = new FolderDescriptor(folderName)
            {
                Parent = this
            };
        }

        return child?.GetByPath(folderNames, createIfNotExists);
    }

    internal static void MoveFile(FolderDescriptor? form, FolderDescriptor? to, FileDescriptor file)
    {
        form?._files.Remove(file);
        to?._files.Add(file);
    }
}