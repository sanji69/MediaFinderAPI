namespace MediaFinder.DTOs.Offers
{
    public class PhysicalOfferSearchQuery
    {
        public string Title { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty; // movie / tv
        public int? SeasonNumber { get; set; }
        public string? Language { get; set; }
        public string? CountryCode { get; set; }
    }
}
