using Newtonsoft.Json;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Models
{
    public class SearchGovPublisherSearchConfig : PublisherSearchConfig, ISearchGovSearchConfig
    {
        [JsonIgnore]
        public string Affiliate { get; set; }

        [JsonIgnore]
        public string AccessKey { get; set; }
    }
}
