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
    [Route("api/admin/reports")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminReportsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminCommentReportDto>>> GetPendingReports()
        {
            try
            {
                var userId = GetCurrentUserId();

                var reports = await _adminService.GetPendingReportsAsync(userId);

                return Ok(reports);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorDto { Code = ex.Message });
            }
        }

        [HttpPut("{reportId:guid}/reject")]
        public async Task<IActionResult> Reject(Guid reportId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _adminService.RejectReportAsync(userId, reportId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorDto { Code = ex.Message });
            }
        }

        [HttpPut("{reportId:guid}/accept")]
        public async Task<IActionResult> Accept(Guid reportId)
        {
            try
            {
                var userId = GetCurrentUserId();

                await _adminService.AcceptReportAsync(userId, reportId);

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
