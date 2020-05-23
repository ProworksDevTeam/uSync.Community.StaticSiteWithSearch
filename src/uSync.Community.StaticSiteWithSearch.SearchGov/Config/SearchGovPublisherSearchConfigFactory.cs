using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Config
{
    public class SearchGovPublisherSearchConfigFactory : PublisherSearchConfigFactory<SearchGovPublisherSearchConfig>
    {
        protected override void Populate(SearchGovPublisherSearchConfig config, XElement serverElement, out XElement searchApplianceElement)
        {
            base.Populate(config, serverElement, out searchApplianceElement);

            config.Affiliate = searchApplianceElement?.Element("affiliate")?.Value;
            config.AccessKey = searchApplianceElement?.Element("accessKey")?.Value;

            config.ReplaceableValues = new Dictionary<string, string>
            {
                [nameof(ISearchGovSearchConfig.Affiliate)] = config.Affiliate,
                [nameof(ISearchGovSearchConfig.AccessKey)] = config.AccessKey
            };

            var displays = new Dictionary<string, string>(config.DisplayedValues.Count);
            config.DisplayedValues.ToList().ForEach(v => displays[v.Key] = v.Value);
            displays["Affiliate Name"] = config.Affiliate;
            displays["Has Access Key"] = string.IsNullOrWhiteSpace(config.AccessKey) ? "No" : "Yes";
            config.DisplayedValues = displays;
        }
    }
}
