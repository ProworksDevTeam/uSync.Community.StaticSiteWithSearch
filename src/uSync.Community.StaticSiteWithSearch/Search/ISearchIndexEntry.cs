using System.Collections.Generic;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public class ISearchIndexEntry
    {
        public string ObjectID { get; }
        public IEnumerable<string> Path { get; }
        public string Name { get; }
        public string Url { get; }
    }
}
