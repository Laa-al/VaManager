using VaManager.Models.Basic;

namespace VaManager.Data.Tools;

public interface IFilterDescriptor<in TItem>
{
    bool Filter(TItem item);
    bool Enabled { get; }
}