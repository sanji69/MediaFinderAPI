using MediaFinder.Enums;

namespace MediaFinder.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty; // movie / tv
        public string MediaTitle { get; set; } = string.Empty;
        public string? MediaPosterPath { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CommentStatus Status { get; set; } = CommentStatus.Visible;
        public DateTime? DeletedAt { get; set; }
        public DateTime? HiddenAt { get; set; }
        public Guid? ModeratedByUserId { get; set; }
        public ICollection<CommentReport> Reports { get; set; } = new List<CommentReport>();
    }
}
