namespace MediaFinder.DTOs.Favorites
{
    public class AddFavoriteRequestDto
    {
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty; // movie / tv
        public string Title { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
    }
}
