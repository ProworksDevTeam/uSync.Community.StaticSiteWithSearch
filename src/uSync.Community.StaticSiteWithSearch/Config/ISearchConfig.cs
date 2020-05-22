using System.Collections.Generic;

namespace uSync.Community.StaticSiteWithSearch.Config
{
    public interface ISearchConfig
    {
        string UniqueName { get; }
        bool CanUpdate { get; }
        IReadOnlyDictionary<string, string> ReplaceableValues { get; }
        IReadOnlyDictionary<string, string> DisplayedValues { get; }
    }
}
