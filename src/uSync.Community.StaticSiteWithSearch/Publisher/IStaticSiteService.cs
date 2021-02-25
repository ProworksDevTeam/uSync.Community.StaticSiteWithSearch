using System;
using Umbraco.Core;
using Umbraco.Web;

namespace uSync.Community.StaticSiteWithSearch.Publisher
{
    public interface IStaticSiteService
    {
        int GetItemId(Udi udi);
        string GenerateItemHtml(UmbracoHelper umbracoHelper, int itemId);
        string GetItemPath(int itemId);
        void SaveMedia(Guid id, Udi udi);
        void SaveFolders(Guid id, string[] folders);
        void UseUmbracoHelper(Action<UmbracoHelper> action);
    }
}
