using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core;
using uSync.Publisher.Models;
using uSync.Publisher.Static;

namespace uSync.Community.StaticSiteWithSearch.Config
{
    public interface IStaticDeployer : ISyncStaticDeployer
    {
        Attempt<byte[]> GetFile(XElement config, string relativePath);
        Task<SyncServerStatus> CheckStatus(XElement config);
    }
}
