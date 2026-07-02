using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediaFinder.DTOs.Admin;
using MediaFinder.DTOs.Common;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Authorization;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Moderator")]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminUsersController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
        {
            try
            {
                var userId = GetCurrentUserId();

                var users = await _adminService.GetUsersAsync(userId);

                return Ok(users);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorDto { Code = ex.Message });
            }
        }

        [HttpPut("{targetUserId:guid}/ban")]
        public async Task<IActionResult> Ban(Guid targetUserId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _adminService.BanUserAsync(userId, targetUserId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorDto { Code = ex.Message });
            }
        }

        [HttpPut("{targetUserId:guid}/unban")]
        public async Task<IActionResult> Unban(Guid targetUserId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _adminService.UnbanUserAsync(userId, targetUserId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorDto { Code = ex.Message });
            }
        }

        [HttpPut("{targetUserId:guid}/reset-warnings")]
        public async Task<IActionResult> ResetWarnings(Guid targetUserId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _adminService.ResetWarningsAsync(userId, targetUserId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorDto { Code = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{targetUserId:guid}/role")]
        public async Task<IActionResult> UpdateRole(
            Guid targetUserId,
            UpdateUserRoleRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _adminService.UpdateUserRoleAsync(
                    userId,
                    targetUserId,
                    request.Role);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorDto { Code = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(userIdValue, out var userId))
                throw new UnauthorizedAccessException("INVALID_USER_TOKEN");

            return userId;
        }
    }
}
