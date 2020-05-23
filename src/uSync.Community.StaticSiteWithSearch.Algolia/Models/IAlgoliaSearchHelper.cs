using Algolia.Search.Models.Settings;

namespace uSync.Community.StaticSiteWithSearch.Algolia.Models
{
    public interface IAlgoliaSearchHelper
    {
        string[] GetAllAttributeAliases();
        IndexSettings GetIndexSettings();
    }
}
