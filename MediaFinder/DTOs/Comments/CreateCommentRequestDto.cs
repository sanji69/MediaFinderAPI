namespace MediaFinder.DTOs.Comments
{
    public class CreateCommentRequestDto
    {
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string MediaTitle { get; set; } = string.Empty;
        public string? MediaPosterPath { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
