using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;
using Umbraco.Web.JavaScript;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Community.StaticSiteWithSearch.Controllers;
using uSync.Community.StaticSiteWithSearch.Models;
using uSync.Community.StaticSiteWithSearch.Publisher;
using uSync.Community.StaticSiteWithSearch.Search;
using uSync.Publisher.Static;
using uSync.Publisher.Static.Deployers;

namespace uSync.Community.StaticSiteWithSearch.Composers
{
    [ComposeAfter(typeof(uSync.Publisher.Static.StaticPublisherComposer))]
    public class SearchComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<IPublisherSearchConfigFactory, DefaultPublisherSearchConfigFactory>();
            composition.Register<IStaticSitePublisherExtension, SearchApplianceExtension>();
            composition.Register<IStaticSitePublisherExtension, ImageProcessorExtension>();
            composition.Register<IPublisherSearchConfigs, PublisherSearchConfigs>();
            composition.Components().Append<SearchComponent>();
            composition.WithCollectionBuilder<SyncStaticDeployerCollectionBuilder>().Exclude<SyncStaticSFtpDeployer>();
            composition.WithCollectionBuilder<SyncStaticDeployerCollectionBuilder>().Exclude<StaticFTPDeployer>();
            composition.WithCollectionBuilder<SyncStaticDeployerCollectionBuilder>().Exclude<StaticFolderDeployer>();
        }
    }

    public class SearchComponent : IComponent
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISearchApplianceService _searchApplianceService;
        private readonly ILogger _logger;
        private readonly ISearchConfig _searchConfig;
        private readonly ISearchIndexEntryHelper _searchIndexEntryHelper;

        public SearchComponent(IUmbracoContextFactory umbracoContextFactory, ISearchApplianceService searchApplianceService, ILogger logger, ISearchConfig searchConfig, ISearchIndexEntryHelper searchIndexEntryHelper)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _searchApplianceService = searchApplianceService;
            _logger = logger;
            _searchConfig = searchConfig;
            _searchIndexEntryHelper = searchIndexEntryHelper;
        }

        public void Initialize()
        {
            if (_searchConfig.CanUpdate)
            {
                ContentService.Published += ContentService_Published;
                ContentService.Unpublished += ContentService_Unpublished;
                ContentService.Deleted += ContentService_Deleted;
            }
            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
        }

        private void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> e)
        {
            if (HttpContext.Current == null)
                throw new InvalidOperationException("This method requires that an HttpContext be active");

            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            if (!e.TryGetValue("uSync", out var u) || !(u is Dictionary<string, object> uSync)) e["uSync"] = uSync = new Dictionary<string, object>();

            uSync.Add("Community", new Dictionary<string, object>
            {
                { "StaticSiteWithSearch", new Dictionary<string, object>
                    {
                        { "ExternalSiteSearch", new Dictionary<string, object>
                            {
                                { "serviceRoot", urlHelper.GetUmbracoApiServiceBaseUrl<ExternalSiteSearchController>(controller => controller.GetApi()) }
                            }
                        }
                    }
                }
            });
        }

        private void ContentService_Deleted(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.DeleteEventArgs<IContent> e) => RemoveIndexEntries(e.DeletedEntities);
        private void ContentService_Unpublished(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.PublishEventArgs<IContent> e) => RemoveIndexEntries(e.PublishedEntities);

        private void RemoveIndexEntries(IEnumerable<IContent> contents)
        {
            try
            {
                var updatedItems = contents.Select(c => new UpdateItemReference { ContentId = c.Id }).ToList();
                _searchApplianceService.UpdateSearchAppliance(new List<ISearchIndexEntry>(), updatedItems);
            }
            catch (Exception ex)
            {
                _logger.Error<SearchComponent>("Could not remove index entries", ex);
            }
        }

        private void ContentService_Published(Umbraco.Core.Services.IContentService sender, Umbraco.Core.Events.ContentPublishedEventArgs e)
        {
            try
            {
                using (var context = _umbracoContextFactory.EnsureUmbracoContext())
                {
                    var defaultUrl = new Uri("/", UriKind.Relative);
                    var entries = e.PublishedEntities.Select(pe =>
                    {
                        var pc = context.UmbracoContext.Content.GetById(pe.Id);
                        var baseUrl = defaultUrl;

                        if (pc != null)
                        {
                            var absUrl = pc.Url(mode: UrlMode.Absolute).ToString();
                            var relUrl = pc.Url(mode: UrlMode.Relative).ToString();
                            baseUrl = new Uri(absUrl.Substring(0, absUrl.Length - relUrl.Length));
                        }

                        return _searchIndexEntryHelper.GetIndexEntry(baseUrl, pe);
                    }).Where(ie => ie != null).ToList();

                    _searchApplianceService.UpdateSearchAppliance(entries);
                }
            }
            catch (Exception ex)
            {
                _logger.Error<SearchComponent>("Could not update index entries", ex);
            }
        }

        public void Terminate()
        {
        }
    }
}
