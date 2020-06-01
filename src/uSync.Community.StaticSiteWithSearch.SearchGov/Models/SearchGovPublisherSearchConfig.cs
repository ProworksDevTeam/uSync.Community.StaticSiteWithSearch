using Newtonsoft.Json;
using System.Configuration;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Models
{
    public class SearchGovPublisherSearchConfig : PublisherSearchConfig, ISearchGovSearchConfig
    {
        [JsonIgnore]
        public string BaseUrl { get; set; } = ConfigurationManager.AppSettings["Search:BaseUrl"] ?? "https://api.gsa.gov/technology/searchgov/v2/";

        [JsonIgnore]
        public string Affiliate { get; set; }

        [JsonIgnore]
        public string AccessKey { get; set; }
    }
}
