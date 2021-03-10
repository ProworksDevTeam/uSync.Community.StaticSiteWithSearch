using ClientDependency.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Publisher.Static;

namespace uSync.Community.StaticSiteWithSearch.Models
{
    public class PublisherSearchConfig : IPublisherSearchConfig
    {
        internal void Populate(SyncStaticDeployerCollection deployers, XElement serverElement, out XElement searchAppliance)
        {
            var url = serverElement?.Attribute("url")?.Value;
            if (url != null && !url.EndsWith("/")) url += '/';
            Url = Uri.TryCreate(url, UriKind.Absolute, out var serverUri) ? serverUri : null;

            ServerAlias = serverElement?.Attribute("alias")?.Value;
            DeployerConfig = serverElement?.Element("deployer");

            var alias = DeployerConfig?.Attribute("alias")?.Value;
            LimitedDeployer = deployers.GetDeployer(alias);
            Deployer = LimitedDeployer as IStaticDeployer;

            var server = DeployerConfig?.Element("server")?.Value;
            var username = DeployerConfig?.Element("username")?.Value;

            if (alias.EndsWith("-ext")) alias = alias.Substring(0, alias.Length - 4);
            var prefix = (alias == "folder" ? "file" : alias) + "://" + (!string.IsNullOrWhiteSpace(username) ? username + "@" : "") + (!string.IsNullOrWhiteSpace(server) ? server + "/" : "");
            var folder = DeployerConfig?.Element("folder")?.Value?.Replace('\\', '/');
            if (folder != null && folder.StartsWith("//"))
            {
                prefix = (alias == "folder" ? "file" : alias) + "://" + (!string.IsNullOrWhiteSpace(username) ? username + "@" : "");
                folder = folder.Substring(2);
            }
            if (folder != null && !folder.EndsWith("/")) folder += '/';
            Folder = folder != null ? new Uri(prefix + folder) : null;

            searchAppliance = serverElement?.Element("searchAppliance");

            Displayeds["Url"] = () => Url.ToString();
            Displayeds["Server"] = () => ServerAlias;
            Displayeds["Folder"] = () => Folder?.ToString();
            Displayeds["Can Update"] = () => CanUpdate ? "Yes" : "No";
        }

        [JsonIgnore]
        public string ServerAlias { get; set; }

        [JsonIgnore]
        public string SearchPageContentTypeAlias { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("folder")]
        public Uri Folder { get; set; }

        [JsonProperty("uniqueName")]
        public virtual string UniqueName => Url?.ToString();

        [JsonProperty("canUpdate")]
        public virtual bool CanUpdate { get; set; }

        [JsonProperty("replaceableValues")]
        public IReadOnlyDictionary<string, string> ReplaceableValues => Replaceables.ToDictionary(p => p.Key, p => p.Value.Invoke());

        [JsonProperty("displayedValues")]
        public IReadOnlyDictionary<string, string> DisplayedValues => Displayeds.ToDictionary(p => p.Key, p => p.Value.Invoke());

        [JsonIgnore]
        public Dictionary<string, Func<string>> Replaceables { get; } = new Dictionary<string, Func<string>>();

        [JsonIgnore]
        public Dictionary<string, Func<string>> Displayeds { get; } = new Dictionary<string, Func<string>>();

        public IStaticDeployer Deployer { get; set; }
        public ISyncStaticDeployer LimitedDeployer { get; set; }
        public XElement DeployerConfig { get; set; }
    }
}
