using MediaFinder.Enums;

namespace MediaFinder.Entities
{
    public class CommentReport
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; } = null!;
        public Guid ReporterUserId { get; set; }
        public User ReporterUser { get; set; } = null!;
        public string Reason { get; set; } = string.Empty;
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
    }
}
