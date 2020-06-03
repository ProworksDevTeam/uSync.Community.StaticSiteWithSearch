using Newtonsoft.Json;
using System.Collections.Generic;
using uSync.Community.StaticSiteWithSearch.Search;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Models
{
    public class SearchIndexEntry : WebResult, ISearchIndexEntry
    {
        public string ObjectID => null;
        public IEnumerable<string> Path => null;

        [JsonProperty("name")]
        public string Name => Title;
    }
}
