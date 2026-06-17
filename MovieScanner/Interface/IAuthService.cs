using MediaFinder.DTOs.Auth;

namespace MediaFinder.Interface
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequestDto request, string? language = null);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task ConfirmEmailAsync(string token);
        Task DeleteCurrentUserAsync(Guid userId);
    }
}
