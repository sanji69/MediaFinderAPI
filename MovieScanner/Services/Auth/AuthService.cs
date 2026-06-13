using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediaFinder.Data;
using MediaFinder.DTOs.Auth;
using MediaFinder.Entities;
using MediaFinder.Interface;
using MediaFinder.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
                throw new InvalidOperationException("Email already exists.");

            var usernameExists = await _dbContext.Users
                .AnyAsync(x => x.Username == username);

            if (usernameExists)
                throw new InvalidOperationException("Username already exists.");

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
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsEmailConfirmed)
                throw new UnauthorizedAccessException("Email is not confirmed.");

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
                    && !x.IsDeleted);

            if (user == null)
                throw new InvalidOperationException("Invalid confirmation token.");

            if (user.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Confirmation token expired.");

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username", user.Username)
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
                IsEmailConfirmed = user.IsEmailConfirmed
            };
        }
    }
}
