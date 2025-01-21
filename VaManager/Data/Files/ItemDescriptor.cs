using VaManager.Models.Basic;

namespace VaManager.Data.Files;

public abstract class ItemDescriptor(string name) : ViewModelBase
{
    public abstract string Path { get; }
    public virtual string Name { get; set; } = name;
    public abstract string Type { get; }
    public abstract byte[]? Preview { get; }
    public abstract bool DefaultVisibility { get; }
    public abstract string Description { get; }
    public virtual string LengthDesc => string.Empty;
    public virtual string CompressedLengthDesc => string.Empty;
}