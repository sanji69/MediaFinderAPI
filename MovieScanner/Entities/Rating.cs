namespace MediaFinder.Entities
{
    public class Rating
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty; // movie / tv
        public int Score { get; set; } // 1 à 5
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
