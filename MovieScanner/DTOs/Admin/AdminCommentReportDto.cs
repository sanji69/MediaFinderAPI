using MediaFinder.Enums;

namespace MediaFinder.DTOs.Admin
{
    public class AdminCommentReportDto
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public string CommentContent { get; set; } = string.Empty;
        public string CommentAuthorUsername { get; set; } = string.Empty;
        public Guid CommentAuthorUserId { get; set; }
        public UserRole CommentAuthorRole { get; set; }
        public short CommentAuthorWarningCount { get; set; }
        public Guid ReporterUserId { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public int MediaId { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string MediaTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
    }
}
