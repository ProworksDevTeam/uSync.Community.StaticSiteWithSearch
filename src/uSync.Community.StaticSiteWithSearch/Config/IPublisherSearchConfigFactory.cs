using System.Xml.Linq;

namespace uSync.Community.StaticSiteWithSearch.Config
{
    public interface IPublisherSearchConfigFactory
    {
        IPublisherSearchConfig Create(XElement element);
    }
}
