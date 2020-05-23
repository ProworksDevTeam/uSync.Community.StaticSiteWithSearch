using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using uSync.Community.StaticSiteWithSearch.Config;

namespace uSync.Community.StaticSiteWithSearch.Algolia.Models
{
    public interface IAlgoliaSearchConfig : ISearchConfig
    {
        string ApplicationId { get; }
        string SearchApiKey { get; }
        string UpdateApiKey { get; }
        string IndexName { get; }
    }

    public class AlgoliaSearchConfig : IAlgoliaSearchConfig
    {
        [JsonIgnore]
        public string ApplicationId { get; } = ConfigurationManager.AppSettings["Search:ApplicationId"];

        [JsonIgnore]
        public string SearchApiKey { get; } = ConfigurationManager.AppSettings["Search:SearchApiKey"];

        [JsonIgnore]
        public string UpdateApiKey { get; } = ConfigurationManager.AppSettings["Search:UpdateApiKey"];

        [JsonIgnore]
        public string IndexName { get; } = ConfigurationManager.AppSettings["Search:IndexName"];

        [JsonProperty("uniqueName")]
        public string UniqueName => "Local Site";

        [JsonProperty("canUpdate")]
        public bool CanUpdate => !string.IsNullOrWhiteSpace(UpdateApiKey);

        [JsonIgnore]
        public IReadOnlyDictionary<string, string> ReplaceableValues => new Dictionary<string, string>
        {
            [nameof(ApplicationId)] = ApplicationId,
            [nameof(SearchApiKey)] = SearchApiKey,
            [nameof(UpdateApiKey)] = UpdateApiKey,
            [nameof(IndexName)] = IndexName
        };

        [JsonProperty("displayedValues")]
        public IReadOnlyDictionary<string, string> DisplayedValues => new Dictionary<string, string>
        {
            ["Application ID"] = ApplicationId,
            ["Has Search API Key"] = string.IsNullOrWhiteSpace(SearchApiKey) ? "No" : "Yes",
            ["Has Update API Key"] = string.IsNullOrWhiteSpace(UpdateApiKey) ? "No" : "Yes",
            ["Index Name"] = IndexName
        };
    }
}
