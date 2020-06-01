using System.Xml.Linq;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Config
{
    public class SearchGovPublisherSearchConfigFactory : PublisherSearchConfigFactory<SearchGovPublisherSearchConfig>
    {
        public SearchGovPublisherSearchConfigFactory(uSync.Publisher.Static.SyncStaticDeployerCollection deployers) : base(deployers)
        {
        }

        protected override void Populate(SearchGovPublisherSearchConfig config, XElement serverElement, out XElement searchApplianceElement)
        {
            base.Populate(config, serverElement, out searchApplianceElement);

            config.BaseUrl = searchApplianceElement?.Element("baseUrl")?.Value ?? config.BaseUrl;
            config.Affiliate = searchApplianceElement?.Element("affiliate")?.Value;
            config.AccessKey = searchApplianceElement?.Element("accessKey")?.Value;

            config.Replaceables[nameof(ISearchGovSearchConfig.Affiliate)] = () => config.Affiliate;
            config.Replaceables[nameof(ISearchGovSearchConfig.AccessKey)] = () => config.AccessKey;

            config.Displayeds["Affiliate Name"] = () => config.Affiliate;
            config.Displayeds["Has Access Key"] = () => string.IsNullOrWhiteSpace(config.AccessKey) ? "No" : "Yes";
        }
    }
}
