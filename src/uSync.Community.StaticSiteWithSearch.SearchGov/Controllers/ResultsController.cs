using Examine;
using Newtonsoft.Json;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Controllers
{
    [PluginController("StaticSiteWithSearch")]
    public class ResultsController : UmbracoApiController
    {
        private readonly ISearchGovSearchConfig _searchGovSearchConfig;
        private readonly IExamineManager _examineManager;

        public ResultsController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper, UmbracoMapper umbracoMapper, ISearchGovSearchConfig searchGovSearchConfig, IExamineManager examineManager)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper, umbracoMapper)
        {
            _searchGovSearchConfig = searchGovSearchConfig;
            _examineManager = examineManager;
        }

        [HttpGet]
        public IEnumerable<MockSearchResult> I14y(string affiliate, string access_key, string query, bool enable_highlighting = true, int limit = 20, int offset = 0, string sort_by = "relevance")
        {
            if (affiliate != _searchGovSearchConfig.Affiliate || access_key != _searchGovSearchConfig.AccessKey || !_examineManager.TryGetIndex("ExternalIndex", out var idx) || !(idx.GetSearcher() is ISearcher searcher)) return null;

            var results = searcher.Search(query);
            var map = new Dictionary<string, ISearchResult>((int)results.TotalItemCount);
            var contents = Umbraco.Content(results.Select(r => r.Id).ToArray()).ToList();

            results.ToList().ForEach(r => map[r.Id] = r);

            return new[]
            {
                new MockSearchResult
                {
                    Query = query,
                    Web = new WebResults
                    {
                        Total = contents.Count,
                        NextOffset = contents.Count > (offset + limit) ? offset + limit : -1,
                        Results = contents.Skip(offset).Take(limit).Select(c => new WebResult
                        {
                            Title = c.Name,
                            Url = c.Url(mode: UrlMode.Absolute),
                            Snippet = map.TryGetValue(c.Id.ToString(), out var s) ? GetSnippet(query, s) : null
                        })
                    }
                }
            };
        }

        private string GetSnippet(string query, ISearchResult result)
        {
            if (result == null) return null;

            var lower = query.ToLowerInvariant();
            foreach (var field in result.AllValues)
            {
                foreach (var value in field.Value)
                {
                    if (value == null) continue;

                    var idx = value.ToLowerInvariant().IndexOf(lower);
                    if (idx < 0) continue;

                    var min = Math.Max(idx - 20, 0);
                    var max = Math.Min(idx + lower.Length + 20, value.Length);

                    return value.Substring(min, idx - min) + '\uE000' + value.Substring(idx, query.Length) + '\uE001' + value.Substring(idx + query.Length, max - (idx + query.Length));
                }
            }

            return null;
        }

        public class MockSearchResult
        {
            [JsonProperty("query")]
            public string Query { get; set; }

            [JsonProperty("web")]
            public WebResults Web { get; set; }
        }

        public class WebResults
        {
            [JsonProperty("total")]
            public int Total { get; set; }

            [JsonProperty("next_offset")]
            public int NextOffset { get; set; }

            [JsonProperty("spelling_correction")]
            public string SpellingCorrection { get; set; }

            [JsonProperty("results")]
            public IEnumerable<WebResult> Results { get; set; }
        }

        public class WebResult
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("snippet")]
            public string Snippet { get; set; }

            [JsonProperty("publication_date")]
            public string PublicationDate { get; set; }
        }
    }
}
