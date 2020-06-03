using Examine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using uSync.Community.StaticSiteWithSearch.SearchGov.Config;
using uSync.Community.StaticSiteWithSearch.SearchGov.Models;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Controllers
{
    [PluginController("StaticSiteWithSearch")]
    public class ResultsController : UmbracoApiController
    {
        private readonly ISearchGovSearchConfig _searchGovSearchConfig;
        private readonly IExamineManager _examineManager;
        private readonly List<ISearchGovMockResultExtender> _searchGovMockResultExtenders;

        public ResultsController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper, UmbracoMapper umbracoMapper, ISearchGovSearchConfig searchGovSearchConfig, IExamineManager examineManager, IEnumerable<ISearchGovMockResultExtender> searchGovMockResultExtenders)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper, umbracoMapper)
        {
            _searchGovSearchConfig = searchGovSearchConfig;
            _examineManager = examineManager;
            _searchGovMockResultExtenders = searchGovMockResultExtenders?.ToList() ?? new List<ISearchGovMockResultExtender>();
        }

        [HttpGet]
        public MockSearchResult I14y(string affiliate, string access_key, string query, bool enable_highlighting = true, int limit = 20, int offset = 0, string sort_by = "relevance")
        {
            try
            {
                if (affiliate != _searchGovSearchConfig.Affiliate || access_key != _searchGovSearchConfig.AccessKey || !_examineManager.TryGetIndex("ExternalIndex", out var idx) || !(idx.GetSearcher() is ISearcher searcher)) return null;

                var results = searcher.Search(query);
                var map = new Dictionary<string, ISearchResult>((int)results.TotalItemCount);
                var contents = Umbraco.Content(results.Select(r => r.Id).ToArray()).ToList();

                results.ToList().ForEach(r => map[r.Id] = r);

                var result = new MockSearchResult
                {
                    Query = query,
                    Web = new WebResults
                    {
                        Total = contents.Count,
                        NextOffset = contents.Count > (offset + limit) ? new int?(offset + limit) : null,
                        Results = contents.Skip(offset).Take(limit).Select(c => new WebResult
                        {
                            Title = c.Name,
                            Url = c.Url(mode: UrlMode.Absolute),
                            Snippet = map.TryGetValue(c.Id.ToString(), out var s) ? GetSnippet(query, s) : null
                        })
                    }
                };

                _searchGovMockResultExtenders.ForEach(e => e.ExtendResults(result, contents, map, affiliate, access_key, query, enable_highlighting, limit, offset, sort_by));

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error<ResultsController>("Could not generate the Search.gov style results", ex);
                return new MockSearchResult
                {
                    Query = query,
                    Web = new WebResults
                    {
                        Results = new WebResult[0]
                    }
                };
            }
        }

        /// <summary>
        /// Gets the first two matches of the query, and a few words before and after
        /// </summary>
        private string GetSnippet(string query, ISearchResult result)
        {
            if (result == null) return null;

            var sb = new StringBuilder();
            var lower = query.ToLowerInvariant();
            var matches = 0;

            foreach (var field in result.AllValues)
            {
                foreach (var value in field.Value)
                {
                    if (value == null) continue;

                    var idx = value.ToLowerInvariant().IndexOf(lower);
                    if (idx < 0) continue;

                    var min = idx;
                    var max = idx + lower.Length;

                    // Include 7 words before
                    for (var i = 0; i < 7 && min > 0; i++)
                    {
                        min = Math.Max(value.LastIndexOf(' ', min - 1) + 1, 0);
                    }

                    // Include 6 words after
                    for (var i = 0; i < 6 && max < value.Length; i++)
                    {
                        var index = value.IndexOf(' ', max + 1);
                        max = idx > 0 ? index : value.Length;
                    }

                    if (min > 0 || matches > 0) sb.Append("...");
                    sb.Append(value.Substring(min, idx - min));
                    sb.Append('\uE000');
                    sb.Append(value.Substring(idx, query.Length));
                    sb.Append('\uE001');
                    sb.Append(value.Substring(idx + query.Length, max - (idx + query.Length)));

                    if (++matches >= 2) return sb.ToString();
                }
            }

            return null;
        }
    }
}
