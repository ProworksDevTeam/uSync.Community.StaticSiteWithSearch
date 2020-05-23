using System.Collections.Generic;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Models;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public interface ISearchApplianceService
    {
        void UpdateSearchAppliance(ICollection<ISearchIndexEntry> entries, ICollection<UpdateItemReference> updatedItems = null, ISearchConfig config = null, bool waitForCompletion = false);
        SearchCountResult GetTotalRecords(ISearchConfig config = null);
        SearchIndexResult Search(string term, int page = 1, ISearchConfig config = null);
    }
}
