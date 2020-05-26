using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Models;
using uSync.Publisher.Publishers;
using uSync8.Core.Dependency;

namespace uSync.Community.StaticSiteWithSearch.Publisher
{
    public interface IStaticSitePublisherExtension
    {
        object BeginPublish(Guid id, string syncRoot, SyncPublisherAction action, ActionArguments args);
        void AddCustomDependencies(object state, ICollection<uSyncDependency> dependencies);
        string TransformHtml(object state, IContent content, string itemPath, string generatedHtml);
        void AddCustomFilesAndFolders(object state, ICollection<string> customFolders, IDictionary<string, Stream> customFiles);
        void BeforeFinalPublish(object state);
        void EndPublish(object state);
    }
}
