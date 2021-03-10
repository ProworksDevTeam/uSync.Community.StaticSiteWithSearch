using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using uSync.Community.StaticSiteWithSearch.Config;

namespace uSync.Community.StaticSiteWithSearch.Models
{
    public class PublisherSearchConfigs : IPublisherSearchConfigs
    {
        protected readonly string _configFile;

        public PublisherSearchConfigs(IPublisherSearchConfigFactory factory, ILogger logger)
        {
            _configFile = Path.Combine(SystemDirectories.Config + "/uSync.Publish.config");

            var configs = new Dictionary<string, IPublisherSearchConfig>(StringComparer.InvariantCultureIgnoreCase);

            try
            {
                var servers = XElement.Load(IOHelper.MapPath(_configFile)).Element("servers");

                if (servers != null)
                {
                    foreach (XElement serverElement in servers.Elements("server"))
                    {
                        var config = factory.Create(serverElement);
                        if (config == null) continue;

                        configs[config.ServerAlias] = config;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error<PublisherSearchConfigs>(ex, "Could not load server configs");
            }

            ConfigsByServerName = configs;
        }

        public IReadOnlyDictionary<string, IPublisherSearchConfig> ConfigsByServerName { get; }
    }
}
