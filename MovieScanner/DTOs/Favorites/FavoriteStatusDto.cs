namespace MediaFinder.DTOs.Favorites
{
    public class FavoriteStatusDto
    {
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}
