using MediaFinder.DTOs.Auth;
using MediaFinder.Entities;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
        {
            var userId = GetCurrentUserId();

            var profile = await _profileService.GetCurrentUserAsync(userId);

            return Ok(profile);
        }

        [HttpPost("avatar")]
        public async Task<ActionResult<UserProfileDto>> UploadAvatar(
            IFormFile file)
        {
            var userId = GetCurrentUserId();

            var profile = await _profileService.UploadAvatarAsync(
                userId,
                file);

            return Ok(profile);
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            return Guid.Parse(userId);
        }
    }
}
