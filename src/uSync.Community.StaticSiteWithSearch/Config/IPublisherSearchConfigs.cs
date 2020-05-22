using System.Collections.Generic;

namespace uSync.Community.StaticSiteWithSearch.Config
{
    public interface IPublisherSearchConfigs
    {
        IReadOnlyDictionary<string, IPublisherSearchConfig> ConfigsByServerName { get; }
    }
}
