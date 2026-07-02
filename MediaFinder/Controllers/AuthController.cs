using MediaFinder.DTOs.Auth;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace MediaFinder.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request, [FromQuery] string? language = null)
        {
            try
            {
                await _authService.RegisterAsync(request, language);

                return Ok(new
                {
                    message = "Account created. Please confirm your email."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    code = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    code = ex.Message
                });
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            try
            {
                await _authService.ConfirmEmailAsync(token);

                return Ok(new
                {
                    message = "Email confirmed."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    code = ex.Message
                });
            }
        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe()
        {
            var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
                return Unauthorized();

            await _authService.DeleteCurrentUserAsync(userId);

            return NoContent();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request, [FromQuery] string? language = null)
        {
            await _authService.ForgotPasswordAsync(request, language);

            return Ok(new
            {
                message = "If the email exists, a reset link has been sent."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);

                return Ok(new
                {
                    message = "Password has been reset."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    code = ex.Message
                });
            }
        }
    }
}
