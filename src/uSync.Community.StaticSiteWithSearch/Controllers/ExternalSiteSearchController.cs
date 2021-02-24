using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Models;
using uSync.Community.StaticSiteWithSearch.Search;

namespace uSync.Community.StaticSiteWithSearch.Controllers
{
    [PluginController("StaticSiteWithSearch")]
    public class ExternalSiteSearchController : UmbracoAuthorizedApiController
    {
        private static readonly ConcurrentDictionary<Guid, RebuildStatus> _rebuilds = new ConcurrentDictionary<Guid, RebuildStatus>();

        private readonly ISearchConfig _searchConfig;
        private readonly IPublisherSearchConfigs _publisherSearchConfigs;
        private readonly ISearchApplianceService _searchApplianceService;
        private readonly IContentService _contentService;
        private readonly ISearchIndexEntryHelper _searchIndexEntryHelper;

        public ExternalSiteSearchController(ISearchConfig searchConfig, IPublisherSearchConfigs publisherSearchConfigs, ISearchApplianceService searchApplianceService, IContentService contentService, ISearchIndexEntryHelper searchIndexEntryHelper)
        {
            _searchConfig = searchConfig;
            _publisherSearchConfigs = publisherSearchConfigs;
            _searchApplianceService = searchApplianceService;
            _contentService = contentService;
            _searchIndexEntryHelper = searchIndexEntryHelper;
        }

        public bool GetApi() => true;

        [HttpGet]
        public IEnumerable<ISearchConfig> GetKnownSites()
        {
            var results = new[] { _searchConfig }.Concat(_publisherSearchConfigs.ConfigsByServerName.Values).ToList();
            return results;
        }

        [HttpGet]
        public SearchCountResult GetTotalRecords(string uniqueName)
        {
            var site = GetKnownSites().FirstOrDefault(s => s.UniqueName == uniqueName);
            if (site == null) return new SearchCountResult { Error = "Unknown Site" };

            return _searchApplianceService.GetTotalRecords(site);
        }

        [HttpGet]
        public Guid RebuildIndex(string uniqueName)
        {
            var site = GetKnownSites().FirstOrDefault(s => s.UniqueName == uniqueName);
            var guid = Guid.NewGuid();
            var status = new RebuildStatus { UniqueName = uniqueName };
            _rebuilds[guid] = status;

            if (site != null)
            {
                var baseUri = new Uri(Request.RequestUri, "/");
                HostingEnvironment.QueueBackgroundWorkItem(token =>
                {
                    try
                    {
                        if (site is IPublisherSearchConfig psc) RebuildRemoteSite(psc, token);
                        else RebuildLocalSite(baseUri, site, token);

                        status.Success = true;
                    }
                    catch (Exception ex)
                    {
                        var err = Guid.NewGuid();
                        Logger.Error<ExternalSiteSearchController>($"Could not rebuild index - Error ID: {err}", ex);
                        status.Error = $"An error occurred; search the logs for error ID {err} for more details.  " + ex.Message;
                    }
                    finally
                    {
                        status.Complete = true;
                    }
                });
            }
            else
            {
                status.Complete = true;
                status.Error = "Unknown Site";
            }

            return guid;
        }

        [HttpGet]
        public SearchCountResult GetRebuildStatus(Guid rebuildId)
        {
            if (_rebuilds.TryGetValue(rebuildId, out var status) && status.Complete)
            {
                // Clear out already completed records that aren't the current one
                _rebuilds.Keys.ToList().ForEach(k =>
                {
                    if (k == rebuildId || !_rebuilds.TryGetValue(k, out var s) || !s.Complete) return;
                    _rebuilds.TryRemove(k, out _);
                });

                if (status.Success) return GetTotalRecords(status.UniqueName);
                else return new SearchCountResult { Error = string.IsNullOrWhiteSpace(status.Error) ? "An unknown error has occurred" : status.Error };
            }
            else return new SearchCountResult();
        }

        [HttpGet]
        public SearchIndexResult SearchIndex(string uniqueName, string term, int page)
        {
            var site = GetKnownSites().FirstOrDefault(s => s.UniqueName == uniqueName);
            if (site == null) return new SearchIndexResult { PageNumber = 1, Results = new ISearchIndexEntry[0], TotalPages = 1 };

            return _searchApplianceService.Search(term, page, site);
        }


        private void RebuildLocalSite(Uri baseUri, ISearchConfig site, CancellationToken token)
        {
            var contents = _contentService.GetPagedDescendants(-1, 0, 1000, out var total);
            var entries = new List<ISearchIndexEntry>((int)total);
            var current = 0L;
            var page = 0;

            while (current < total)
            {
                foreach (var content in contents)
                {
                    if (token.IsCancellationRequested) return;
                    current++;

                    if (!content.Published) continue;
                    var entry = _searchIndexEntryHelper.GetIndexEntry(baseUri, content);
                    if (entry != null) entries.Add(entry);
                }

                if (token.IsCancellationRequested) return;
                if (current < total) contents = _contentService.GetPagedDescendants(-1, ++page, 1000, out total);
            }

            if (token.IsCancellationRequested) return;
            _searchApplianceService.UpdateSearchAppliance(entries, new[] { new UpdateItemReference { ContentUdi = new GuidUdi(global::Umbraco.Core.Constants.UdiEntityType.Document, Guid.Empty), IncludeDescendents = true } }, site, true);
        }

        private void RebuildRemoteSite(IPublisherSearchConfig site, CancellationToken token)
        {
            if (site?.Deployer == null || token.IsCancellationRequested) return;

            var result = site.Deployer.GetFile(site.DeployerConfig, Constants.IndexDataFilePath);
            if (token.IsCancellationRequested) return;

            var existingContent = result.Success ? Encoding.UTF8.GetString(result.Result) : null;
            var entries = !string.IsNullOrWhiteSpace(existingContent) && existingContent[0] == '[' ? JArray.Parse(existingContent)?.OfType<JObject>()?.ToList().ConvertAll(_searchIndexEntryHelper.Convert).Where(e => e != null).ToList() : new List<ISearchIndexEntry>();

            if (token.IsCancellationRequested) return;
            if (entries.Count > 0) _searchApplianceService.UpdateSearchAppliance(entries, new[] { new UpdateItemReference { ContentUdi = new GuidUdi(global::Umbraco.Core.Constants.UdiEntityType.Document, Guid.Empty), IncludeDescendents = true } }, site, true);
        }

        private class RebuildStatus
        {
            public string UniqueName;
            public volatile bool Complete;
            public volatile bool Success;
            public volatile string Error;
        }
    }
}
