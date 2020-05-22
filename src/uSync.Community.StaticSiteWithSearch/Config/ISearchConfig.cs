using System.Collections.Generic;

namespace uSync.Community.StaticSiteWithSearch.Config
{
    public interface ISearchConfig
    {
        string UniqueName { get; }
        IReadOnlyDictionary<string, string> ReplaceableValues { get; }
        bool CanUpdate { get; }
    }
}
