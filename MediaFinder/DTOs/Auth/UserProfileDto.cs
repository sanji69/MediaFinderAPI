using MediaFinder.Enums;

namespace MediaFinder.DTOs.Auth
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarPath { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public short WarningCount { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public UserRole Role { get; set; }
    }
}
