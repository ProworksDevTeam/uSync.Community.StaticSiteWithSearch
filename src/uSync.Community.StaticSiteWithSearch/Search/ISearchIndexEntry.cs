using System.Collections.Generic;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public interface ISearchIndexEntry
    {
        string ObjectID { get; }
        IEnumerable<string> Path { get; }
        string Name { get; }
        string Url { get; }
    }
}
