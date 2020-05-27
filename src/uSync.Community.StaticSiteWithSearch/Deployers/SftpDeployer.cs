using Renci.SshNet;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Publisher.Models;
using uSync.Publisher.Static;

namespace uSync.Community.StaticSiteWithSearch.Deployers
{
    public class SftpDeployer : SyncStaticSFtpDeployer, IStaticDeployer
    {
        public new string Name => "Extensible SFTP Deployer";

        public new string Alias => "sftp";

        public Attempt<byte[]> GetFile(XElement config, string relativePath)
        {
            var result = Attempt.Fail<byte[]>();

            try
            {
                var settings = LoadSettings(config);
                var connectionInfo = new ConnectionInfo(settings.Server, settings.Username, new PasswordAuthenticationMethod(settings.Username, settings.Password), new PrivateKeyAuthenticationMethod("rsa.key"));

                using (SftpClient sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();

                    using (var ms = new MemoryStream())
                    {
                        sftpClient.DownloadFile(Path.Combine(settings.Folder, relativePath).Replace('\\', '/'), ms);
                        result = Attempt.Succeed(ms.ToArray());
                    }

                    sftpClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                result = Attempt.Fail<byte[]>(null, ex);
            }

            return result;
        }

        public Task<SyncServerStatus> CheckStatus(XElement config)
        {
            try
            {
                var settings = LoadSettings(config);
                var connectionInfo = new ConnectionInfo(settings.Server, settings.Username, new PasswordAuthenticationMethod(settings.Username, settings.Password), new PrivateKeyAuthenticationMethod("rsa.key"));

                using (SftpClient sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();
                    var result = sftpClient.Exists(settings.Folder);
                    sftpClient.Disconnect();
                    return Task.FromResult(result ? SyncServerStatus.Success : SyncServerStatus.Unavailable);
                }
            }
            catch
            {
                return Task.FromResult(SyncServerStatus.ServerError);
            }
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
