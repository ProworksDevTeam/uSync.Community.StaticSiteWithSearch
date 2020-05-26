using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Models;
using uSync.Community.StaticSiteWithSearch.Publisher;
using uSync.Expansions.Core.Models;
using uSync.Publisher.Publishers;
using uSync8.BackOffice.Services;
using uSync8.Core.Dependency;

namespace uSync.Community.StaticSiteWithSearch.Search
{
    public class SearchApplianceExtension : BaseStaticSitePublisherExtension
    {
        private readonly ISearchConfig _searchConfig;
        private readonly ISearchApplianceService _searchApplianceService;
        private readonly IPublisherSearchConfigs _publisherSearchConfigs;
        private readonly ISearchIndexEntryHelper _searchIndexEntryHelper;

        public SearchApplianceExtension(ISearchConfig searchConfig, ISearchApplianceService searchApplianceService, IPublisherSearchConfigs publisherSearchConfigs, ISearchIndexEntryHelper searchIndexEntryHelper)
        {
            _searchConfig = searchConfig;
            _searchApplianceService = searchApplianceService;
            _publisherSearchConfigs = publisherSearchConfigs;
            _searchIndexEntryHelper = searchIndexEntryHelper;
        }

        #region Interface Methods
        public override object BeginPublish(Guid id, string syncRoot, SyncPublisherAction action, ActionArguments args)
        {
            var searchConfig = _publisherSearchConfigs.ConfigsByServerName.TryGetValue(args.Target, out var sc) ? sc : null;

            var config = new ExtensionContext
            {
                Config = searchConfig,
                DeployedItems = args.Options.Items.ToList(),
                Id = id,
                SyncRoot = syncRoot
            };

            return config;
        }

        public override string TransformHtml(object state, IContent content, string itemPath, string generatedHtml)
        {
            if (!(state is ExtensionContext ctx)) return generatedHtml;

            if (ctx.Config.CanUpdate)
            {
                var entry = _searchIndexEntryHelper.GetIndexEntry(ctx.Config.Url, content);
                if (entry != null) ctx.Entries.Add(entry);
            }

            if (_searchConfig.ReplaceableValues == null || _searchConfig.ReplaceableValues.Count == 0
                || ctx.Config.ReplaceableValues == null || ctx.Config.ReplaceableValues.Count == 0
                || content?.ContentType?.Alias != ctx.Config.SearchPageContentTypeAlias) return generatedHtml;

            var html = generatedHtml;

            foreach (var val in _searchConfig.ReplaceableValues)
            {
                if (string.IsNullOrWhiteSpace(val.Value) || !ctx.Config.ReplaceableValues.TryGetValue(val.Key, out var updated)) continue;
                html = html.Replace(val.Value, updated);
            }

            return html;
        }

        public override void AddCustomFilesAndFolders(object state, ICollection<string> customFolders, IDictionary<string, Stream> customFiles)
        {
            if (!(state is ExtensionContext ctx) || !ctx.Config.CanUpdate) return;

            var result = ctx.Config.Deployer.GetFile(ctx.Config.DeployerConfig, Constants.IndexDataFilePath);
            var existingContent = result.Success ? Encoding.UTF8.GetString(result.Result) : null;
            var entries = !string.IsNullOrWhiteSpace(existingContent) && existingContent[0] == '[' ? JArray.Parse(existingContent)?.OfType<JObject>()?.ToList().ConvertAll(_searchIndexEntryHelper.Convert).Where(e => e != null).ToList() : new List<ISearchIndexEntry>();

            var objectsToRemove = ctx.DeployedItems.Where(i => !i.flags.HasFlag(DependencyFlags.IncludeChildren)).Select(i => i.Id.ToString()).ToArray();
            var pathsToRemove = ctx.DeployedItems.Where(i => i.flags.HasFlag(DependencyFlags.IncludeChildren)).Select(i => i.Id.ToString()).ToArray();

            entries.RemoveAll(i => objectsToRemove.Contains(i.ObjectID) || i.Path.Any(p => pathsToRemove.Contains(p)));
            entries.AddRange(ctx.Entries);

            var updated = JsonConvert.SerializeObject(entries);
            customFiles[Constants.IndexDataFilePath] = new MemoryStream(Encoding.UTF8.GetBytes(updated));
        }

        public override void EndPublish(object state)
        {
            if (!(state is ExtensionContext ctx) || !ctx.Config.CanUpdate) return;

            UpdateSearchAppliance(ctx);
        }
        #endregion

        #region Helper Methods
        private void UpdateSearchAppliance(ExtensionContext ctx)
        {
            _searchApplianceService.UpdateSearchAppliance(ctx.Entries, ctx.UpdatedItems.ToList(), ctx.Config);
        }
        #endregion

        #region Helper Classes
        private class ExtensionContext
        {
            public IPublisherSearchConfig Config { get; set; }
            public string SyncRoot { get; set; }
            public Guid Id { get; set; }
            public List<ISearchIndexEntry> Entries { get; } = new List<ISearchIndexEntry>();
            public List<SyncItem> DeployedItems { get; set; }
            public IEnumerable<UpdateItemReference> UpdatedItems => DeployedItems.Select(i => new UpdateItemReference { ContentId = i.Id, IncludeDescendents = i.flags.HasFlag(DependencyFlags.IncludeChildren) });
        }
        #endregion
    }
}
