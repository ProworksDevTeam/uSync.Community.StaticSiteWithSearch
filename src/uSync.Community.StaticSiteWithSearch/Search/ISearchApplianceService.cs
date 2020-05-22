using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using uSync.Community.StaticSiteWithSearch.Config;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public interface ISearchApplianceService
    {
        ISearchIndexEntry GetIndexEntry(Uri baseUri, IContent content);
        void UpdateSearchAppliance(ICollection<ISearchIndexEntry> entries, ICollection<UpdateItemReference> updatedItems = null, ISearchConfig config = null, bool waitForCompletion = false);
        SearchCountResult GetTotalRecords(ISearchConfig config = null);
        SearchIndexResult Search(string term, int page = 1, ISearchConfig config = null);
    }
}
