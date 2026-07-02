using MediaFinder.Enums;

namespace MediaFinder.DTOs.Comments
{
    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarPath { get; set; }
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string MediaTitle { get; set; } = string.Empty;
        public string? MediaPosterPath { get; set; }
        public string Content { get; set; } = string.Empty;
        public CommentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool HasCurrentUserReported { get; set; }
    }
}
