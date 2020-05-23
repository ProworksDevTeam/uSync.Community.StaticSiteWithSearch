using Newtonsoft.Json;
using System.Collections.Generic;
using uSync.Community.StaticSiteWithSearch.Search;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Models
{
    public class SearchIndexEntry : ISearchIndexEntry
    {
        public string ObjectID => null;
        public IEnumerable<string> Path => null;

        [JsonProperty("title")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; }

        [JsonProperty("snippet")]
        public string Snippet { get; }

        [JsonProperty("publication_date")]
        public string PublicationDate { get; }
    }
}
