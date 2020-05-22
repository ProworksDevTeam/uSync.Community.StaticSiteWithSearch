using Newtonsoft.Json;
using System.Collections.Generic;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public class SearchIndexResult
    {
        [JsonProperty("results")]
        public IEnumerable<ISearchIndexEntry> Results { get; set; }

        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }
}
