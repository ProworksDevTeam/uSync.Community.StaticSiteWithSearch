using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Models;
using uSync.Community.StaticSiteWithSearch.Config;
using uSync.Publisher.Publishers;
using uSync8.Core.Dependency;


namespace uSync.Community.StaticSiteWithSearch.Publisher
{
    public class BaseStaticSitePublisherExtension : IStaticSitePublisherExtension
    {
        public virtual void AddCustomDependencies(object state, ICollection<uSyncDependency> dependencies)
        {
        }

        public virtual void AddCustomFilesAndFolders(object state, ICollection<string> customFolders, IDictionary<string, Stream> customFiles)
        {
        }

        public virtual void EndPublish(object state)
        {
        }

        public virtual object BeginPublish(Guid id, string syncRoot, SyncPublisherAction action, ActionArguments args, IPublisherSearchConfig config) => null;

        public virtual string TransformHtml(object state, IContent content, string itemPath, string generatedHtml) => generatedHtml;

        public virtual void BeforeFinalPublish(object state)
        {
        }
    }
}
