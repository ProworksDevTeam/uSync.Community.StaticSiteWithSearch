using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using uSync.Community.StaticSiteWithSearch.Algolia.Models;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.Algolia.Config
{
    public class AlgoliaPublisherSearchConfigFactory : PublisherSearchConfigFactory<AlgoliaPublisherSearchConfig>
    {
        protected override void Populate(AlgoliaPublisherSearchConfig config, XElement serverElement, out XElement searchApplianceElement)
        {
            base.Populate(config, serverElement, out searchApplianceElement);

            config.ApplicationId = searchApplianceElement?.Element("applicationId")?.Value;
            config.IndexName = searchApplianceElement?.Element("indexName")?.Value;
            config.SearchApiKey = searchApplianceElement?.Element("searchApiKey")?.Value;
            config.UpdateApiKey = searchApplianceElement?.Element("updateApiKey")?.Value;

            config.ReplaceableValues = new Dictionary<string, string>
            {
                [nameof(IAlgoliaSearchConfig.ApplicationId)] = config.ApplicationId,
                [nameof(IAlgoliaSearchConfig.SearchApiKey)] = config.SearchApiKey,
                [nameof(IAlgoliaSearchConfig.UpdateApiKey)] = config.UpdateApiKey,
                [nameof(IAlgoliaSearchConfig.IndexName)] = config.IndexName
            };

            var displays = new Dictionary<string, string>(config.DisplayedValues.Count);
            config.DisplayedValues.ToList().ForEach(v => displays[v.Key] = v.Value);
            displays["Application ID"] = config.ApplicationId;
            displays["Has Search API Key"] = string.IsNullOrWhiteSpace(config.SearchApiKey) ? "No" : "Yes";
            displays["Has Update API Key"] = string.IsNullOrWhiteSpace(config.UpdateApiKey) ? "No" : "Yes";
            displays["Index Name"] = config.IndexName;
            config.DisplayedValues = displays;
        }
    }
}
