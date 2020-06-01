using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Expansions.Core.Physical;
using uSync.Publisher.Models;
using uSync.Publisher.Static.Deployers;

namespace uSync.Community.StaticSiteWithSearch.Deployers
{
    public class FolderDeployer : StaticFolderDeployer, IStaticDeployer
    {
        public new string Name => "Extensible Static Folder Deployer";

        public new string Alias => "folder";

        public FolderDeployer(TemplateFileService templateFileService) : base(templateFileService)
        {
        }

        public Attempt<byte[]> GetFile(XElement config, string relativePath)
        {
            var folder = config.Element("folder").Value;
            var path = string.IsNullOrWhiteSpace(folder) ? relativePath : Path.Combine(folder, relativePath);

            return File.Exists(path) ? Attempt.Succeed(File.ReadAllBytes(path)) : Attempt.Fail<byte[]>();
        }

        public Task<SyncServerStatus> CheckStatus(XElement config)
        {
            try
            {
                return Task.FromResult(Directory.Exists(config.Element("folder").Value) ? SyncServerStatus.Success : SyncServerStatus.Unavailable);
            }
            catch
            {
                return Task.FromResult(SyncServerStatus.ServerError);
            }
        }

        public Attempt<int> RemovePathsIfExist(XElement config, IEnumerable<string> relativePaths)
        {
            var success = 0;
            var baseFolder = config.Element("folder").Value;
            Exception e = null;

            foreach (var relativePath in relativePaths)
            {
                if (string.IsNullOrWhiteSpace(relativePath)) continue;

                try
                {
                    var path = relativePath.Replace('/', '\\');
                    if (path[0] == '\\') path = path.Substring(1);
                    path = Path.Combine(baseFolder, path);
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    success++;
                }
                catch (Exception ex)
                {
                    e = ex;
                }
            }

            return e == null ? Attempt.Succeed(success) : Attempt.Fail(success, e);
        }
    }
}
