using Examine;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Config
{
    public interface ISearchGovMockResultExtender
    {
        void ExtendResults(MockSearchResult result, ICollection<IPublishedContent> contents, IDictionary<string, ISearchResult> examineResultsById, string affiliate, string accessKey, string query, bool enableHighlighting, int limit, int offset, string sortBy);
    }
}
