namespace MediaFinder.DTOs.Offers
{
    public class PhysicalOfferDto
    {
        public string Title { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty; // eBay
        public string? ImageUrl { get; set; }
        public string Url { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal? ShippingPrice { get; set; }
        public string? Condition { get; set; }
        public string? Format { get; set; } // DVD, Blu-ray, 4K
        public string OfferType { get; set; } = "Unknown"; // Movie, Season, CompleteSeries, BoxSet
    }
}
