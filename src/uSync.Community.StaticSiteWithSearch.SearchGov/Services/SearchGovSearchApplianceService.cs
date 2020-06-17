using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Umbraco.Core.Logging;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Models;
using uSync.Community.StaticSiteWithSearch.Search;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Services
{
    public class SearchGovSearchApplianceService : ISearchApplianceService, IDisposable
    {
        private readonly ISearchGovSearchConfig _searchGovSearchConfig;
        private readonly ISearchIndexEntryHelper _searchIndexEntryHelper;
        private readonly ILogger _logger;
        private HttpClient _client;

        public SearchGovSearchApplianceService(ISearchGovSearchConfig searchGovSearchConfig, ISearchIndexEntryHelper searchIndexEntryHelper, ILogger logger)
        {
            _searchGovSearchConfig = searchGovSearchConfig;
            _searchIndexEntryHelper = searchIndexEntryHelper;
            _logger = logger;
            _client = new HttpClient { BaseAddress = new Uri(searchGovSearchConfig.BaseUrl) };
        }

        public SearchCountResult GetTotalRecords(ISearchConfig config = null)
        {
            try
            {
                var result = Search("*", 1, false, 1, config);
                return new SearchCountResult { TotalRecords = result.TotalResults, Success = true };
            }
            catch (Exception ex)
            {
                var err = Guid.NewGuid();
                _logger.Error<SearchGovSearchApplianceService>(ex, $"Could not perform a search for all records, error ID# {err}");
                return new SearchCountResult { Error = $"Could not determine the number of records.  Check the logs for the error ID# {err}" };
            }
        }

        public SearchIndexResult Search(string term, int page = 1, ISearchConfig config = null) => Search(term, page, true, 20, config);

        public void UpdateSearchAppliance(ICollection<ISearchIndexEntry> entries, ICollection<UpdateItemReference> updatedItems = null, ISearchConfig config = null, bool waitForCompletion = false)
        {
            // You cannot update the Search.gov index
            throw new NotImplementedException();
        }

        private SearchIndexResult Search(string term, int page, bool enableHighlighting, int pageSize, ISearchConfig config)
        {
            if (!(config is ISearchGovSearchConfig cfg)) cfg = _searchGovSearchConfig;

            var url = $"{_searchGovSearchConfig.BaseUrl}?affiliate={Uri.EscapeDataString(cfg.Affiliate)}"
                + $"&access_key={Uri.EscapeDataString(cfg.AccessKey)}"
                + $"&query={Uri.EscapeDataString(term)}"
                + $"&enable_highlighting={enableHighlighting}"
                + $"&offset={(page - 1) * pageSize}"
                + $"&limit={pageSize}";

            var resp = _client.GetAsync(url).Result;
            if (!resp.IsSuccessStatusCode) return new SearchIndexResult { PageNumber = 1, TotalPages = 1, Results = new ISearchIndexEntry[0] };

            var content = resp.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrWhiteSpace(content) || content[0] != '[') return new SearchIndexResult { PageNumber = 1, TotalPages = 1, Results = new ISearchIndexEntry[0] };

            // Get the first result, as we only have one search feature
            var web = JArray.Parse(content).OfType<JObject>().FirstOrDefault()?["web"] as JObject;
            if (web == null) return new SearchIndexResult { PageNumber = 1, TotalPages = 1, Results = new ISearchIndexEntry[0] };

            var total = int.TryParse(web["total"]?.ToString(), out var t) ? t : 0;
            var results = (web["results"] as JArray)?.OfType<JObject>().ToList().ConvertAll(_searchIndexEntryHelper.Convert).Where(e => e != null).ToList();

            return new SearchIndexResult { PageNumber = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize), TotalResults = total, Results = results };
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}
