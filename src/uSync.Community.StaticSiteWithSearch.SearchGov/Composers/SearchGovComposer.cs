using Umbraco.Core;
using Umbraco.Core.Composing;
using uSync.Community.StaticSiteWithSearch.SearchGov.Config;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;
using uSync.Community.StaticSiteWithSearch.Composers;
using uSync.Community.StaticSiteWithSearch.Config;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Composers
{
    [ComposeAfter(typeof(SearchComposer))]
    public class SearchGovComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<ISearchConfig, SearchGovSearchConfig>();
            composition.RegisterUnique<IPublisherSearchConfigFactory, SearchGovPublisherSearchConfigFactory>();
        }
    }
}
