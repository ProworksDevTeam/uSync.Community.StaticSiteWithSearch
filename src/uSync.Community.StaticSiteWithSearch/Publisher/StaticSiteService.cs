using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Scoping;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;
using uSync.Expansions.Core.Physical;

namespace uSync.Community.StaticSiteWithSearch.Publisher
{
    public class StaticSiteService : IStaticSiteService
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly TemplateFileService _templateFileService;
        private readonly uSyncMediaFileService _mediaFileService;
        private readonly string _syncRoot;

        public StaticSiteService(
            IScopeProvider scopeProvider,
            IUmbracoContextFactory umbracoContextFactory,
            TemplateFileService templateFileService,
            uSyncMediaFileService mediaFileService,
            IGlobalSettings globalSettings
            )
        {
            _scopeProvider = scopeProvider;
            _umbracoContextFactory = umbracoContextFactory;
            _templateFileService = templateFileService;
            _mediaFileService = mediaFileService;

            _syncRoot = Path.GetFullPath(Path.Combine(globalSettings.LocalTempPath, "uSync", "pack"));
        }

        public virtual string GenerateItemHtml(UmbracoHelper umbracoHelper, int itemId) => umbracoHelper.RenderTemplate(itemId, new int?()).ToString();

        public virtual int GetItemId(Udi udi) => UseUmbracoContext(x => x.Content.GetById(udi)?.Id ?? -1);

        public virtual string GetItemPath(int itemId) => UseUmbracoContext(x => x.Content.GetById(itemId)?.Url ?? $"_errors/{itemId}");

        public virtual void SaveFolders(Guid packId, string[] folders)
        {
            if (folders == null || folders.Length == 0) return;

            var rootPath = Path.Combine(_syncRoot, packId.ToString());

            foreach (string folder in folders)
            {
                var target = Path.Combine(rootPath, folder.Substring(2));
                _templateFileService.CopyFolder(folder, target);
            }
        }

        public virtual void SaveMedia(Guid packId, Udi mediaId)
        {
            var rootPath = Path.Combine(_syncRoot, packId.ToString());

            _mediaFileService.CopyMediaFile(mediaId, rootPath);
        }

        public virtual void UseUmbracoHelper(Action<UmbracoHelper> action)
            => UseUmbracoContext(x =>
            {
                action(new UmbracoHelper(
                    x.IsFrontEndUmbracoRequest ? x.PublishedRequest?.PublishedContent : null,
                    Current.Factory.GetInstance<ITagQuery>(),
                    Current.Factory.GetInstance<ICultureDictionaryFactory>(),
                    Current.Factory.GetInstance<IUmbracoComponentRenderer>(),
                    Current.Factory.GetInstance<IPublishedContentQuery>(),
                    Current.Factory.GetInstance<MembershipHelper>()
                    ));
                return true;
            }
            );

        protected virtual T UseUmbracoContext<T>(Func<UmbracoContext, T> action)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                try
                {
                    using (var ucr = _umbracoContextFactory.EnsureUmbracoContext())
                    {
                        return action(ucr.UmbracoContext);
                    }
                }
                finally
                {
                    scope.Complete();
                }
            }
        }
    }
}
