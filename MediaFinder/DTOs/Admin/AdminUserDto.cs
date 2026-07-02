using MediaFinder.Enums;

namespace MediaFinder.DTOs.Admin
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
        public UserRole Role { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public short WarningCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? BannedAt { get; set; }
    }
}
