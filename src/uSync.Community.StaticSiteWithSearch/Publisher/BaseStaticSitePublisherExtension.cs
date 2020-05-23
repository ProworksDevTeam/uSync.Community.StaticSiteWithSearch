﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using uSync.Publisher.Publishers;
using uSync8.Core.Dependency;


namespace uSync.Community.StaticSiteWithSearch.Publisher
{
    public class BaseStaticSitePublisherExtension : IStaticSitePublisherExtension
    {
        public virtual void AddCustomDependencies(object state, ICollection<uSyncDependency> dependencies)
        {
        }

        public virtual void AddCustomFolders(object state, ICollection<string> customFolders)
        {
        }

        public virtual void EndPublish(object state)
        {
        }

        public virtual object BeginPublish(Guid id, string syncRoot, SyncPublisherAction action, ActionArguments args) => null;

        public virtual string TransformHtml(object state, IContent content, string itemPath, string generatedHtml) => generatedHtml;

        public virtual void BeforeFinalPublish(object state)
        {
        }
    }
}