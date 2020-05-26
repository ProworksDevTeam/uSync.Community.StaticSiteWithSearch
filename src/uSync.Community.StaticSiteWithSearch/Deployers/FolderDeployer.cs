using System.IO;
using System.Xml.Linq;
using Umbraco.Core;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Expansions.Core.Physical;
using uSync.Publisher.Static.Deployers;

namespace uSync.Community.StaticSiteWithSearch.Deployers
{
    public class FolderDeployer : StaticFolderDeployer, IStaticDeployer
    {
        public new string Name => "Extensible Static Folder Deployer";

        public new string Alias => "folder-ext";

        public FolderDeployer(TemplateFileService templateFileService) : base(templateFileService)
        {
        }

        public Attempt<byte[]> GetFile(XElement config, string relativePath)
        {
            var folder = config.Element("folder").Value;
            var path = string.IsNullOrWhiteSpace(folder) ? relativePath : Path.Combine(folder, relativePath);

            return File.Exists(path) ? Attempt.Succeed(File.ReadAllBytes(path)) : Attempt.Fail<byte[]>();
        }
    }
}
