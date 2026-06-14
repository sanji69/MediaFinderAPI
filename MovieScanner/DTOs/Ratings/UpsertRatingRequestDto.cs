namespace MediaFinder.DTOs.Ratings
{
    public class UpsertRatingRequestDto
    {
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty; // movie / tv
        public decimal Score { get; set; } // 0 à 5
    }
}
