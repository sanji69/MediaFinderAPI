using MediaFinder.DTOs.Auth;
using MediaFinder.Interface;
using Microsoft.AspNetCore.Mvc;

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
            await _authService.RegisterAsync(request, language);

            return Ok(new
            {
                message = "Account created. Please confirm your email."
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            await _authService.ConfirmEmailAsync(token);

            return Ok(new
            {
                message = "Email confirmed."
            });
        }
    }
}
