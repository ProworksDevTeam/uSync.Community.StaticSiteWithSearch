using Newtonsoft.Json.Linq;
using System;
using Umbraco.Core.Models;
using uSync.Community.StaticSiteWithSearch.Search;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Services
{
    public class SearchIndexEntryHelper : ISearchIndexEntryHelper
    {
        public ISearchIndexEntry Convert(JObject entry) => entry?.ToObject<SearchIndexEntry>();

        // Cannot update the Search.gov index
        public ISearchIndexEntry GetIndexEntry(Uri baseUri, IContent content) => null;
    }
}
