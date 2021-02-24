using Algolia.Search.Clients;
using Algolia.Search.Models.Common;
using Algolia.Search.Models.Search;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Logging;
using uSync.Community.StaticSiteWithSearch.Algolia.Models;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Models;
using uSync.Community.StaticSiteWithSearch.Search;

namespace uSync.Community.StaticSiteWithSearch.Algolia.Services
{
    public class AlgoliaSearchApplianceService : ISearchApplianceService
    {
        private readonly IAlgoliaSearchHelper _algoliaSearchHelper;
        private readonly IAlgoliaSearchConfig _algoliaSearchConfig;
        private readonly ISearchIndexEntryHelper _searchIndexEntryHelper;
        private readonly ILogger _logger;

        public AlgoliaSearchApplianceService(IAlgoliaSearchHelper algoliaSearchHelper, IAlgoliaSearchConfig algoliaSearchConfig, ISearchIndexEntryHelper searchIndexEntryHelper, ILogger logger)
        {
            _algoliaSearchHelper = algoliaSearchHelper;
            _algoliaSearchConfig = algoliaSearchConfig;
            _searchIndexEntryHelper = searchIndexEntryHelper;
            _logger = logger;
        }

        public void UpdateSearchAppliance(ICollection<ISearchIndexEntry> entries, ICollection<UpdateItemReference> updatedItems = null, ISearchConfig config = null, bool waitForCompletion = false)
        {
            var currentEntries = entries;
            var currentItems = updatedItems ?? entries.Select(e => new UpdateItemReference { ContentUdi = Udi.Parse(e.ObjectID) }).ToList();
            var currentConfig = config as IAlgoliaSearchConfig ?? _algoliaSearchConfig;

            if (waitForCompletion)
            {
                try
                {
                    var src = new CancellationTokenSource();
                    UpdateSearchAppliance(src.Token, currentEntries, currentItems, currentConfig).Wait();
                }
                catch (Exception ex)
                {
                    _logger.Error<AlgoliaSearchApplianceService>("Could not update search appliance", ex);
                }
            }
            else HostingEnvironment.QueueBackgroundWorkItem(async t =>
            {
                try
                {
                    await UpdateSearchAppliance(t, currentEntries, currentItems, currentConfig);
                }
                catch (Exception ex)
                {
                    _logger.Error<AlgoliaSearchApplianceService>("Could not update search appliance", ex);
                }
            });
        }

        public SearchCountResult GetTotalRecords(ISearchConfig config = null)
        {
            if (!(config is IAlgoliaSearchConfig cfg)) cfg = _algoliaSearchConfig;

            try
            {
                var client = new SearchClient(cfg.ApplicationId, cfg.UpdateApiKey);
                var index = client.InitIndex(cfg.IndexName);
                var results = index.BrowseFrom<JObject>(new BrowseIndexQuery() { HitsPerPage = 1 });
                return new SearchCountResult { Success = true, TotalRecords = results.NbHits };
            }
            catch (Exception ex)
            {
                return new SearchCountResult { Error = ex.Message };
            }
        }

        public SearchIndexResult Search(string term, int page = 1, ISearchConfig config = null)
        {
            if (!(config is IAlgoliaSearchConfig cfg)) cfg = _algoliaSearchConfig;

            try
            {
                var client = new SearchClient(cfg.ApplicationId, cfg.SearchApiKey);
                var index = client.InitIndex(cfg.IndexName);
                var results = index.Search<JObject>(new Query(term)
                {
                    AttributesToHighlight = new string[0],
                    AttributesToRetrieve = _algoliaSearchHelper.GetAllAttributeAliases(),
                    AttributesToSnippet = new string[0],
                    Page = page - 1
                });
                return new SearchIndexResult { PageNumber = results.Page + 1, Results = results.Hits?.ConvertAll(_searchIndexEntryHelper.Convert).Where(e => e != null), TotalPages = results.NbPages, TotalResults = results.NbHits };
            }
            catch (Exception ex)
            {
                _logger.Error<AlgoliaSearchApplianceService>($"Could not retrieve search results from {cfg.ApplicationId}:{cfg.IndexName} for '{term}'", ex);
                return new SearchIndexResult { PageNumber = 1, Results = new ISearchIndexEntry[0], TotalPages = 1 };
            }
        }

        private async Task UpdateSearchAppliance(CancellationToken token, ICollection<ISearchIndexEntry> entries, ICollection<UpdateItemReference> updatedItems, IAlgoliaSearchConfig config)
        {
            if (token.IsCancellationRequested) return;

            var client = new SearchClient(config.ApplicationId, config.UpdateApiKey);
            var index = client.InitIndex(config.IndexName);
            if (token.IsCancellationRequested) return;

            // Make sure the settings match what we expect
            var settings = _algoliaSearchHelper.GetIndexSettings();
            if (settings != null) await index.WaitTaskAsync((await index.SetSettingsAsync(settings)).TaskID);
            if (token.IsCancellationRequested) return;

            // Delete any existing entries (or more importantly previously removed children) for the items being inserted
            var filterItems = updatedItems.Select(i => i.IncludeDescendents ? $"path:{i.ContentUdi}" : $"objectID:{i.ContentUdi}");
            var filters = string.Join(" OR ", filterItems);
            if (!string.IsNullOrWhiteSpace(filters))
            {
                await index.WaitTaskAsync((await index.DeleteByAsync(new Query()
                {
                    Filters = filters
                })).TaskID);
                if (token.IsCancellationRequested) return;
            }

            if (entries.Count > 0)
            {
                var responses = (await index.SaveObjectsAsync(entries)).Responses;
                foreach (var response in responses) await index.WaitTaskAsync(response.TaskID);
            }
        }
    }
}
