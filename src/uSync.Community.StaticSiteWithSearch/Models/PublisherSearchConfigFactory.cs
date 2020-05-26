using System.Xml.Linq;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Publisher;
using uSync.Publisher.Static;

namespace uSync.Community.StaticSiteWithSearch.Models
{
    public abstract class PublisherSearchConfigFactory<T> : IPublisherSearchConfigFactory where T : PublisherSearchConfig, new()
    {
        protected readonly SyncStaticDeployerCollection _deployers;

        protected PublisherSearchConfigFactory(SyncStaticDeployerCollection deployers)
        {
            _deployers = deployers;
        }

        public virtual IPublisherSearchConfig Create(XElement element)
        {
            var publisherAlias = element?.Element("publisher")?.Value;
            if (string.IsNullOrWhiteSpace(publisherAlias) || !Handles(publisherAlias)) return null;

            var config = CreateNew();
            Populate(config, element, out _);
            return config;
        }

        protected virtual bool Handles(string publisherAlias) => ExtensibleStaticPublisher.PublisherAlias == publisherAlias;
        protected virtual T CreateNew() => new T();
        protected virtual void Populate(T config, XElement serverElement, out XElement searchApplianceElement)
        {
            config.Populate(_deployers, serverElement, out searchApplianceElement);
        }
    }

    public class DefaultPublisherSearchConfigFactory : PublisherSearchConfigFactory<PublisherSearchConfig>
    {
        public DefaultPublisherSearchConfigFactory(SyncStaticDeployerCollection deployers) : base(deployers)
        {
        }
    }
}
