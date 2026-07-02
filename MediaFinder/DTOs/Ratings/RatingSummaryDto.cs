namespace MediaFinder.DTOs.Ratings
{
    public class RatingSummaryDto
    {
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
        public int VoteCount { get; set; }
        public decimal? CurrentUserScore { get; set; }
    }
}
