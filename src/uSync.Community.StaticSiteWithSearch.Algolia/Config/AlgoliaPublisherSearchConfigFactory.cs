using System.Xml.Linq;
using uSync.Community.StaticSiteWithSearch.Algolia.Models;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.Algolia.Config
{
    public class AlgoliaPublisherSearchConfigFactory : PublisherSearchConfigFactory<AlgoliaPublisherSearchConfig>
    {
        public AlgoliaPublisherSearchConfigFactory(uSync.Publisher.Static.SyncStaticDeployerCollection deployers) : base(deployers)
        {
        }

        protected override void Populate(AlgoliaPublisherSearchConfig config, XElement serverElement, out XElement searchApplianceElement)
        {
            base.Populate(config, serverElement, out searchApplianceElement);

            config.ApplicationId = searchApplianceElement?.Element("applicationId")?.Value;
            config.IndexName = searchApplianceElement?.Element("indexName")?.Value;
            config.SearchApiKey = searchApplianceElement?.Element("searchApiKey")?.Value;
            config.UpdateApiKey = searchApplianceElement?.Element("updateApiKey")?.Value;
            config.CanUpdate = !string.IsNullOrWhiteSpace(config.UpdateApiKey);

            config.Replaceables[nameof(IAlgoliaSearchConfig.ApplicationId)] = () => config.ApplicationId;
            config.Replaceables[nameof(IAlgoliaSearchConfig.SearchApiKey)] = () => config.SearchApiKey;
            config.Replaceables[nameof(IAlgoliaSearchConfig.UpdateApiKey)] = () => config.UpdateApiKey;
            config.Replaceables[nameof(IAlgoliaSearchConfig.IndexName)] = () => config.IndexName;

            config.Displayeds["Application ID"] = () => config.ApplicationId;
            config.Displayeds["Has Search API Key"] = () => string.IsNullOrWhiteSpace(config.SearchApiKey) ? "No" : "Yes";
            config.Displayeds["Has Update API Key"] = () => string.IsNullOrWhiteSpace(config.UpdateApiKey) ? "No" : "Yes";
            config.Displayeds["Index Name"] = () => config.IndexName;
        }
    }
}
