using Newtonsoft.Json;
using System.Collections.Generic;
using uSync.Community.StaticSiteWithSearch.Search;

namespace uSync.Community.StaticSiteWithSearch.Models
{
    public class SearchIndexResult
    {
        [JsonProperty("results")]
        public IEnumerable<ISearchIndexEntry> Results { get; set; }

        [JsonProperty("pageNumber")]
        public int PageNumber { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
    }
}
