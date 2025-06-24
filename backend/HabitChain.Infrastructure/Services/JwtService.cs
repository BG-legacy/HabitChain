using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HabitChain.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly HabitChainDbContext _context;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtService(IConfiguration configuration, HabitChainDbContext context)
    {
        _configuration = configuration;
        _context = context;
        _tokenValidationParameters = GetTokenValidationParameters();
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");
        var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim("isActive", user.IsActive.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            
            if (validatedToken is JwtSecurityToken jwtToken && 
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId)
    {
        var token = await _context.RefreshTokens
            .Where(rt => rt.Token == refreshToken && rt.UserId == userId)
            .FirstOrDefaultAsync();

        return token != null && 
               !token.IsRevoked && 
               token.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<bool> SaveRefreshTokenAsync(string userId, string refreshToken)
    {
        try
        {
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiration
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        try
        {
            var token = await _context.RefreshTokens
                .Where(rt => rt.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public DateTime GetTokenExpiration()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expireMinutes = int.Parse(jwtSettings["ExpireMinutes"] ?? "60");
        return DateTime.UtcNow.AddMinutes(expireMinutes);
    }

    private TokenValidationParameters GetTokenValidationParameters()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
} 