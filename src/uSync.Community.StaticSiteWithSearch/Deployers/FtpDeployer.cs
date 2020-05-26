using FluentFTP;
using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Xml.Linq;
using Umbraco.Core;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Publisher.Static.Deployers;

namespace uSync.Community.StaticSiteWithSearch.Deployers
{
    public class FtpDeployer : StaticFTPDeployer, IStaticDeployer
    {
        public new string Name => "Extensible FTP Deployer";

        public new string Alias => "ftp-ext";

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
