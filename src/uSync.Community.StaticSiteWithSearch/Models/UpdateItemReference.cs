using Umbraco.Core;

namespace uSync.Community.StaticSiteWithSearch.Models
{
    public class UpdateItemReference
    {
        public Udi ContentUdi { get; set; }
        public bool IncludeDescendents { get; set; }
    }
}
