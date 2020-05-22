using Newtonsoft.Json;

namespace uSync.Community.StaticSiteWithSearch.Models
{
    public class SearchCountResult
    {
        [JsonProperty("totalRecords")]
        public long TotalRecords { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
