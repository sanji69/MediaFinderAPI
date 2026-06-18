using MediaFinder.Data;
using MediaFinder.DTOs.Auth;
using MediaFinder.Entities;
using MediaFinder.Enums;
using MediaFinder.Interface;
using MediaFinder.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MediaFinder.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly MediaFinderDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtOptions _jwtOptions;
        private readonly FrontendOptions _frontendOptions;
        private readonly IEmailService _emailService;

        public AuthService(
            MediaFinderDbContext dbContext,
            IOptions<JwtOptions> jwtOptions,
            IOptions<FrontendOptions> frontendOptions,
            IEmailService emailService)
        {
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<User>();
            _jwtOptions = jwtOptions.Value;
            _frontendOptions = frontendOptions.Value;
            _emailService = emailService;
        }

        public async Task RegisterAsync(RegisterRequestDto request, string? language = null)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var username = request.Username.Trim();

            var emailExists = await _dbContext.Users
                .AnyAsync(x => x.Email == email);

            if (emailExists)
                throw new InvalidOperationException("EMAIL_ALREADY_EXISTS");

            var usernameExists = await _dbContext.Users
                .AnyAsync(x => x.Username == username);

            if (usernameExists)
                throw new InvalidOperationException("USERNAME_ALREADY_EXISTS");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                IsEmailConfirmed = false,
                EmailConfirmationToken = GenerateToken(),
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var confirmationUrl =
                $"{_frontendOptions.BaseUrl}/confirm-email?token={user.EmailConfirmationToken}";

            await _emailService.SendEmailConfirmationAsync(
                user.Email,
                confirmationUrl,
                language);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == email && x.AccountStatus != AccountStatus.Deleted);

            if (user == null)
                throw new UnauthorizedAccessException("INVALID_CREDENTIALS");

            if (user.AccountStatus == AccountStatus.Deleted)
                throw new UnauthorizedAccessException("ACCOUNT_DELETED");

            if (user.AccountStatus == AccountStatus.Banned)
                throw new UnauthorizedAccessException("ACCOUNT_BANNED");

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("INVALID_CREDENTIALS");

            if (!user.IsEmailConfirmed)
                throw new UnauthorizedAccessException("EMAIL_NOT_CONFIRMED");

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = ToProfileDto(user)
            };
        }

        public async Task ConfirmEmailAsync(string token)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x =>
                    x.EmailConfirmationToken == token
                    && x.AccountStatus != AccountStatus.Deleted);

            if (user == null)
                throw new InvalidOperationException("INVALID_CONFIRMATION_TOKEN");

            if (user.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("INVALID_CONFIRMATION_TOKEN");

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCurrentUserAsync(Guid userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            if (user.AccountStatus == AccountStatus.Deleted)
                return;

            _dbContext.Favorites.RemoveRange(user.Favorites);

            var anonymizedValue = $"deleted-user-{user.Id:N}";

            user.Username = anonymizedValue;
            user.Email = $"{anonymizedValue}@deleted.local";
            user.PasswordHash = string.Empty;
            user.AvatarPath = null;

            user.IsEmailConfirmed = false;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiresAt = null;

            user.AccountStatus = AccountStatus.Deleted;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, string? language = null)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    x.AccountStatus == AccountStatus.Active);

            if (user == null)
                return;

            user.PasswordResetToken = GenerateToken();
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var resetUrl =
                $"{_frontendOptions.BaseUrl}/reset-password?token={user.PasswordResetToken}";

            await _emailService.SendPasswordResetAsync(
                user.Email,
                resetUrl,
                language);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x =>
                    x.PasswordResetToken == request.Token &&
                    x.AccountStatus == AccountStatus.Active);

            if (user == null)
                throw new InvalidOperationException("INVALID_RESET_TOKEN");

            if (user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("RESET_TOKEN_EXPIRED");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username", user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Secret));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }

        private static UserProfileDto ToProfileDto(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarPath = user.AvatarPath,
                IsEmailConfirmed = user.IsEmailConfirmed,
                WarningCount = user.WarningCount,
                AccountStatus = user.AccountStatus,
                Role = user.Role
            };
        }
    }
}
