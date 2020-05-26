using System;
using System.Xml.Linq;
using uSync.Publisher.Static;

namespace uSync.Community.StaticSiteWithSearch.Config
{
    public interface IPublisherSearchConfig : ISearchConfig
    {
        string ServerAlias { get; }
        string SearchPageContentTypeAlias { get; }
        Uri Url { get; }
        Uri Folder { get; }
        IStaticDeployer Deployer { get; }
        ISyncStaticDeployer LimitedDeployer { get; }
        XElement DeployerConfig { get; }
    }
}
