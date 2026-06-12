namespace MediaFinder.Entities
{
    public class Favorite
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty; // movie / tv
        public string Title { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
