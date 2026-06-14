namespace MediaFinder.DTOs.Favorites
{
    public class FavoriteDto
    {
        public Guid Id { get; set; }
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
