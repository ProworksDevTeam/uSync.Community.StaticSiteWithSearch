using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Publisher.Models;
using uSync.Publisher.Static.Deployers;

namespace uSync.Community.StaticSiteWithSearch.Deployers
{
    public class FtpDeployer : StaticFTPDeployer, IStaticDeployer
    {
        public new string Name => "Extensible FTP Deployer";

        public new string Alias => "ftp";

        public Attempt<byte[]> GetFile(XElement config, string relativePath)
        {
            var result = Attempt.Fail<byte[]>();

            try
            {
                var settings = LoadSettings(config);
                var ftpClient = new FtpClient(settings.Server)
                {
                    Credentials = new NetworkCredential(settings.Username, settings.Password),
                    EncryptionMode = FtpEncryptionMode.Explicit,
                    SslProtocols = (SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12)
                };

                ftpClient.ValidateCertificate += (s, e) => e.Accept = true;

                ftpClient.Connect();
                if (ftpClient.Download(out var b, Path.Combine(settings.Folder, relativePath).Replace('\\', '/'))) result = Attempt.Succeed(b);
                ftpClient.Disconnect();
            }
            catch (Exception ex)
            {
                result = Attempt.Fail<byte[]>(null, ex);
            }

            return result;
        }

        public async Task<SyncServerStatus> CheckStatus(XElement config)
        {
            try
            {
                var settings = LoadSettings(config);
                var ftpClient = new FtpClient(settings.Server)
                {
                    Credentials = new NetworkCredential(settings.Username, settings.Password),
                    EncryptionMode = FtpEncryptionMode.Explicit,
                    SslProtocols = (SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12)
                };

                ftpClient.ValidateCertificate += (s, e) => e.Accept = true;

                ftpClient.Connect();
                var result = await ftpClient.DirectoryExistsAsync(settings.Folder);
                ftpClient.Disconnect();
                return result ? SyncServerStatus.Success : SyncServerStatus.Unavailable;
            }
            catch
            {
                return SyncServerStatus.ServerError;
            }
        }

        public Attempt<int> RemovePathsIfExist(XElement config, IEnumerable<string> relativePaths)
        {
            var success = 0;
            Exception ex = null;

            try
            {
                var settings = LoadSettings(config);
                var ftpClient = new FtpClient(settings.Server)
                {
                    Credentials = new NetworkCredential(settings.Username, settings.Password),
                    EncryptionMode = FtpEncryptionMode.Explicit,
                    SslProtocols = (SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12)
                };

                ftpClient.ValidateCertificate += (s, e) => e.Accept = true;

                ftpClient.Connect();
                var rPaths = relativePaths.ToList();
                if (!ftpClient.DirectoryExists(settings.Folder)) return Attempt.Succeed(rPaths.Count);

                // Don't ever directly delete the root folder, instead delete all its contents
                if (rPaths.RemoveAll(p => p == "/") > 0)
                {
                    rPaths.AddRange(ftpClient.GetListing(settings.Folder).Select(i => i.FullName));
                }

                foreach (var relativePath in relativePaths)
                {
                    if (string.IsNullOrWhiteSpace(relativePath)) continue;

                    try
                    {
                        var path = relativePath.Replace('/', '\\');
                        if (path[0] == '\\') path = path.Substring(1);
                        path = settings.Folder + (settings.Folder.EndsWith("/") ? "" : "/") + path;

                        if (ftpClient.DirectoryExists(path)) ftpClient.DeleteDirectory(path, FtpListOption.Recursive);
                        else if (ftpClient.FileExists(path)) ftpClient.DeleteFile(path);
                        success++;
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                }
                ftpClient.Disconnect();
            }
            catch (Exception e)
            {
                ex = e;
            }

            return ex == null ? Attempt.Succeed(success) : Attempt.Fail(success, ex);
        }

        private Credentials LoadSettings(XElement config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            return new Credentials
            {
                Server = config.Element("server").Value ?? "",
                Username = config.Element("username").Value ?? "",
                Password = config.Element("password").Value ?? "",
                Folder = config.Element("folder").Value ?? ""
            };
        }

        private class Credentials
        {
            public string Server { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Folder { get; set; }
        }
    }
}
