using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Exception e = null;
            var success = 0;
            var baseFolder = config.Element("folder").Value;
            var rPaths = relativePaths.ToList();
            if (!Directory.Exists(baseFolder)) return Attempt.Succeed(rPaths.Count);

            // Don't ever directly delete the root folder, instead delete all its contents
            if (rPaths.RemoveAll(p => p == "/") > 0)
            {
                rPaths.AddRange(Directory.EnumerateFileSystemEntries(baseFolder).Select(f => f.StartsWith(baseFolder) ? f.Substring(baseFolder.Length + (baseFolder.EndsWith("\\") ? 0 : 1)) : f).Where(f => f.Length > 0));
            }

            foreach (var relativePath in rPaths)
            {
                if (string.IsNullOrWhiteSpace(relativePath)) continue;

                try
                {
                    var path = relativePath.Replace('/', '\\');
                    if (path[0] == '\\') path = path.Substring(1);
                    path = Path.Combine(baseFolder, path);
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    else if (File.Exists(path)) File.Delete(path);
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
