using VaManager.Data.Tools;

namespace VaManager.Extensions;

public static class FilterExtensions
{
    public static IEnumerable<TItem> Filter<TItem, TFilter>(this IEnumerable<TItem> query, IEnumerable<TFilter> filters)
        where TItem : class
        where TFilter : IFilterDescriptor<TItem>
    {
        var enabledFilters = filters.Where(f => f.Enabled).ToArray();
        return query.Where(u => enabledFilters.All(v => v.Filter(u)));
    }
}