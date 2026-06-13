using MediaFinder.DTOs.Auth;

namespace MediaFinder.Interface
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetCurrentUserAsync(Guid userId);

        Task<UserProfileDto> UploadAvatarAsync(Guid userId, IFormFile file);
    }
}
