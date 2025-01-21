using VaManager.Models.Basic;

namespace VaManager.Data.Tools;

public interface IFilter<in TItem>
{
    bool Filter(TItem item);
    bool Enabled { get; }
}