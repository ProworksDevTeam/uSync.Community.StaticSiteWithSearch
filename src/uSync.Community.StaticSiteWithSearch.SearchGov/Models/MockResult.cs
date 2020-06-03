using Newtonsoft.Json;
using System.Collections.Generic;

namespace uSync.Community.StaticSiteWithSearch.SearchGov.Models
{
    public class MockSearchResult
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("web")]
        public WebResults Web { get; set; }

        [JsonProperty("text_best_bets")]
        public IEnumerable<TextBestBet> TextBestBets { get; set; }

        [JsonProperty("graphic_best_bets")]
        public IEnumerable<GraphicBestBet> GraphicBestBets { get; set; }

        [JsonProperty("health_topics")]
        public IEnumerable<HealthTopic> HealthTopics { get; set; }

        [JsonProperty("job_openings")]
        public IEnumerable<JobOpening> JobOpenings { get; set; }

        [JsonProperty("recent_tweets")]
        public IEnumerable<RecentTweet> RecentTweets { get; set; }

        [JsonProperty("federal_register_documents")]
        public IEnumerable<FederalRegisterDocument> FederalRegisterDocuments { get; set; }

        [JsonProperty("related_search_terms")]
        public IEnumerable<string> RelatedSearchTerms { get; set; }

        [JsonProperty("recent_news")]
        public IEnumerable<RecentNews> RecentNews { get; set; }

        [JsonProperty("recent_video_news")]
        public IEnumerable<RecentVideoNews> RecentVideoNews { get; set; }
    }

    public class WebResults
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("next_offset")]
        public int? NextOffset { get; set; }

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

    public class TextBestBet
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class GraphicBestBet
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_url")]
        public string TitleUrl { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("image_alt_text")]
        public string ImageAltText { get; set; }

        [JsonProperty("links")]
        public IEnumerable<SimpleEntry> Links { get; set; }
    }

    public class HealthTopic
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("snippet")]
        public string Snippet { get; set; }

        [JsonProperty("related_topics")]
        public IEnumerable<SimpleEntry> RelatedTopics { get; set; }

        [JsonProperty("related_sites")]
        public IEnumerable<SimpleEntry> RelatedSites { get; set; }
    }

    public class JobOpening
    {
        [JsonProperty("position_id")]
        public string PositionId { get; set; }

        [JsonProperty("position_title")]
        public string PositionTitle { get; set; }

        [JsonProperty("position_uri")]
        public string PositionUri { get; set; }

        [JsonProperty("apply_uri")]
        public string ApplyUri { get; set; }

        [JsonProperty("position_location_display")]
        public string PositionLocationDisplay { get; set; }

        [JsonProperty("position_location")]
        public string PositionLocation { get; set; }

        [JsonProperty("organization_name")]
        public string OrganizationName { get; set; }

        [JsonProperty("department_name")]
        public string DepartmentName { get; set; }

        [JsonProperty("sub_agency")]
        public string SubAgency { get; set; }

        [JsonProperty("job_category")]
        public string JobCategory { get; set; }

        [JsonProperty("job_grade")]
        public string JobGrade { get; set; }

        [JsonProperty("position_schedule")]
        public string PositionSchedule { get; set; }

        [JsonProperty("position_offering_type")]
        public string PositionOfferingType { get; set; }

        [JsonProperty("qualification_summary")]
        public string QualificationSummary { get; set; }

        [JsonProperty("position_remuneration")]
        public string PositionRemuneration { get; set; }

        [JsonProperty("position_start_date")]
        public string PositionStartDate { get; set; }

        [JsonProperty("position_end_date")]
        public string PositionEndDate { get; set; }

        [JsonProperty("publication_start_date")]
        public string PublicationStartDate { get; set; }

        [JsonProperty("application_close_date")]
        public string ApplicationCloseDate { get; set; }

        [JsonProperty("position_formatted_description")]
        public string PositionFormattedDescription { get; set; }

        [JsonProperty("user_area")]
        public string UseArea { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("locations")]
        public string Locations { get; set; }

        [JsonProperty("minimum")]
        public string Minimum { get; set; }

        [JsonProperty("maximum")]
        public string Maximum { get; set; }

        [JsonProperty("rate_interval_code")]
        public string RateIntervalCode { get; set; }

        [JsonProperty("org_codes")]
        public IEnumerable<string> OrgCodes { get; set; }
    }

    public class RecentTweet
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }

    public class FederalRegisterDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("document_number")]
        public string DocumentNumber { get; set; }

        [JsonProperty("document_type")]
        public string DocumentType { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("agency_names")]
        public string AgencyNames { get; set; }

        [JsonProperty("page_length")]
        public string PageLength { get; set; }

        [JsonProperty("start_page")]
        public string StartPage { get; set; }

        [JsonProperty("end_page")]
        public string EndPage { get; set; }

        [JsonProperty("publication_date")]
        public string PublicationDate { get; set; }

        [JsonProperty("comments_close_date")]
        public string CommentsCloseDate { get; set; }
    }

    public class RecentNews
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("snippet")]
        public string Snippet { get; set; }

        [JsonProperty("publication_date")]
        public string PublicationDate { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }

    public class RecentVideoNews
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("snippet")]
        public string Snippet { get; set; }

        [JsonProperty("publication_date")]
        public string PublicationDate { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }

    public class SimpleEntry
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
