using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using uSync.Community.StaticSiteWithSearch.Config;

namespace uSync.Community.StaticSiteWithSearch.Models
{
    public class PublisherSearchConfig : IPublisherSearchConfig
    {
        internal void Populate(XElement serverElement, out XElement searchAppliance)
        {
            var url = serverElement?.Attribute("url")?.Value;
            if (url != null && !url.EndsWith("/")) url += '/';
            Url = Uri.TryCreate(url, UriKind.Absolute, out var serverUri) ? serverUri : null;

            ServerAlias = serverElement?.Attribute("alias")?.Value;
            var deployer = serverElement?.Element("deployer");
            var alias = deployer?.Attribute("alias")?.Value;
            var server = deployer?.Element("server")?.Value;
            var username = deployer?.Element("username")?.Value;
            var prefix = (alias == "folder" ? "file" : alias) + "://" + (!string.IsNullOrWhiteSpace(username) ? username + "@" : "") + (!string.IsNullOrWhiteSpace(server) ? server + "/" : "");
            var folder = deployer?.Element("folder")?.Value?.Replace('\\', '/');
            if (folder.StartsWith("//"))
            {
                prefix = (alias == "folder" ? "file" : alias) + "://" + (!string.IsNullOrWhiteSpace(username) ? username + "@" : "");
                folder = folder.Substring(2);
            }
            if (!folder.EndsWith("/")) folder += '/';
            Folder = new Uri(prefix + folder);

            searchAppliance = serverElement?.Element("searchAppliance");
            var replaceables = new Dictionary<string, string>();
            var displayed = new Dictionary<string, string>
            {
                ["Url"] = Url.ToString(),
                ["Server"] = ServerAlias,
                ["Folder"] = Folder?.ToString(),
                ["Can Update"] = CanUpdate ? "Yes" : "No"
            };

            if (searchAppliance != null)
            {
                foreach (var element in searchAppliance.Elements())
                {
                    replaceables[element.Name.LocalName] = element.Value;
                    displayed[element.Name.LocalName] = element.Value;
                }
            }

            ReplaceableValues = replaceables;
            DisplayedValues = displayed;
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
        public bool CanUpdate { get; set; }
        [JsonProperty("replaceableValues")]
        public IReadOnlyDictionary<string, string> ReplaceableValues { get; set; }
        [JsonProperty("displayedValues")]
        public IReadOnlyDictionary<string, string> DisplayedValues { get; set; }
    }
}
