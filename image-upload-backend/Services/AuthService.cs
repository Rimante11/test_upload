using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ImageUploadApi.Data;
using ImageUploadApi.Models;
using ImageUploadApi.Models.DTOs;

namespace ImageUploadApi.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<User?> GetUserByIdAsync(int userId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthService : IAuthService
    {
        private readonly ImageUploadDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ImageUploadDbContext context,
            ITenantService tenantService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tenantService = tenantService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            try
            {
                // Get tenant first
                var tenant = await _tenantService.GetTenantBySubdomainAsync(request.TenantSubdomain);
                if (tenant == null)
                {
                    _logger.LogWarning("Login attempt with invalid tenant subdomain: {Subdomain}", request.TenantSubdomain);
                    return null;
                }

                // Find user in tenant
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && 
                                            u.TenantId == tenant.Id && 
                                            u.IsActive);

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for email {Email} in tenant {TenantId}", request.Email, tenant.Id);
                    return null;
                }

                // Generate JWT token
                var token = GenerateJwtToken(user, tenant);

                _logger.LogInformation("Successful login for user {UserId} in tenant {TenantId}", user.Id, tenant.Id);

                return new LoginResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    UserId = user.Id,
                    TenantName = tenant.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email} in tenant {TenantSubdomain}", 
                    request.Email, request.TenantSubdomain);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
                throw;
            }
        }

        public string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[32];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            var hashBytes = new byte[64];
            Array.Copy(salt, 0, hashBytes, 0, 32);
            Array.Copy(hash, 0, hashBytes, 32, 32);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                var hashBytes = Convert.FromBase64String(hash);
                var salt = new byte[32];
                Array.Copy(hashBytes, 0, salt, 0, 32);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                var testHash = pbkdf2.GetBytes(32);

                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 32] != testHash[i])
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(User user, Tenant tenant)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "default-development-key-change-in-production-minimum-32-characters";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "ImageUploadApi";
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("TenantId", tenant.Id.ToString()),
                new Claim("TenantSubdomain", tenant.Subdomain)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtIssuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}