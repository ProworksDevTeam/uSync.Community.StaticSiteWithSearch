using Umbraco.Core;
using Umbraco.Core.Composing;
using uSync.Community.StaticSiteWithSearch.Algolia.Config;
using uSync.Community.StaticSiteWithSearch.Algolia.Models;
using uSync.Community.StaticSiteWithSearch.Composers;
using uSync.Community.StaticSiteWithSearch.Config;

namespace uSync.Community.StaticSiteWithSearch.Algolia.Composers
{
    [ComposeAfter(typeof(SearchComposer))]
    public class AlgoliaComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<ISearchConfig, AlgoliaSearchConfig>();
            composition.RegisterUnique<IPublisherSearchConfigFactory, AlgoliaPublisherSearchConfigFactory>();
        }
    }
}
