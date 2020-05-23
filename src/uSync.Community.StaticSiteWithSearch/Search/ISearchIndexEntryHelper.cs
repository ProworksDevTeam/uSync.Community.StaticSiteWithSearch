using Newtonsoft.Json.Linq;
using System;
using Umbraco.Core.Models;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public interface ISearchIndexEntryHelper
    {
        ISearchIndexEntry Convert(JObject entry);
        ISearchIndexEntry GetIndexEntry(Uri baseUri, IContent content);
    }
}
