namespace MediaFinder.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty; // movie / tv
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
