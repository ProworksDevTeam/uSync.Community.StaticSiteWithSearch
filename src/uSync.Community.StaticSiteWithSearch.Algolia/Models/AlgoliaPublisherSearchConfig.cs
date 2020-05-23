using Newtonsoft.Json;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.Algolia.Models
{
    public class AlgoliaPublisherSearchConfig : PublisherSearchConfig, IAlgoliaSearchConfig
    {
        [JsonIgnore]
        public string ApplicationId { get; set; }

        [JsonIgnore]
        public string SearchApiKey { get; set; }

        [JsonIgnore]
        public string UpdateApiKey { get; set; }

        [JsonIgnore]
        public string IndexName { get; set; }
    }
}
