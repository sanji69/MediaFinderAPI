using MediaFinder.Data;
using MediaFinder.DTOs.Auth;
using MediaFinder.Interface;
using Microsoft.EntityFrameworkCore;

namespace MediaFinder.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly MediaFinderDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] AllowedExtensions =
        [
            ".jpg",
        ".jpeg",
        ".png",
        ".webp"
        ];

        public ProfileService(
            MediaFinderDbContext dbContext,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            return ToProfileDto(user);
        }

        public async Task<UserProfileDto> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            if (file.Length == 0)
                throw new InvalidOperationException("File is empty.");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File is too large.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file type.");

            var monthFolder = DateTime.UtcNow.ToString("yyyy-MM");

            var relativeFolder = Path.Combine(
                "uploads",
                "profile-pictures",
                monthFolder);

            var webRootPath = _environment.WebRootPath  ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

            Directory.CreateDirectory(webRootPath);

            var absoluteFolder = Path.Combine(
                webRootPath,
                relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var absolutePath = Path.Combine(absoluteFolder, fileName);

            await using (var stream = File.Create(absolutePath))
            {
                await file.CopyToAsync(stream);
            }

            user.AvatarPath = "/" + Path.Combine(relativeFolder, fileName)
                .Replace("\\", "/");

            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return ToProfileDto(user);
        }

        private static UserProfileDto ToProfileDto(Entities.User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarPath = user.AvatarPath,
                IsEmailConfirmed = user.IsEmailConfirmed
            };
        }
    }
}
