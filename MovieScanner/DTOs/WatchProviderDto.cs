namespace MediaFinder.DTOs
{
    public class WatchProviderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public string Type { get; set; } = string.Empty; // flatrate, rent, buy
    }
}
