using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using uSync.Community.StaticSiteWithSearch.Config;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Models
{
    public interface ISearchGovSearchConfig : ISearchConfig
    {
        string Affiliate { get; }
        string AccessKey { get; }
    }

    public class SearchGovSearchConfig : ISearchGovSearchConfig
    {
        [JsonIgnore]
        public string Affiliate { get; } = ConfigurationManager.AppSettings["Search:Affiliate"];

        [JsonIgnore]
        public string AccessKey { get; } = ConfigurationManager.AppSettings["Search:AccessKey"];

        [JsonProperty("uniqueName")]
        public string UniqueName => "Local Site";

        [JsonProperty("canUpdate")]
        public bool CanUpdate => false;

        [JsonIgnore]
        public IReadOnlyDictionary<string, string> ReplaceableValues => new Dictionary<string, string>
        {
            [nameof(Affiliate)] = Affiliate,
            [nameof(AccessKey)] = AccessKey
        };

        [JsonIgnore]
        public IReadOnlyDictionary<string, string> DisplayedValues => new Dictionary<string, string>
        {
            ["Affiliate Name"] = Affiliate,
            ["Has Access Key"] = string.IsNullOrWhiteSpace(AccessKey) ? "No" : "Yes"
        };
    }
}
