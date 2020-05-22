using System;

namespace uSync.Community.StaticSiteWithSearch.Config
{
    public interface IPublisherSearchConfig : ISearchConfig
    {
        string ServerAlias { get; }
        string SearchPageContentTypeAlias { get; }
        Uri Url { get; }
        Uri Folder { get; }
    }
}
