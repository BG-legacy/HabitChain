using HabitChain.Domain.Entities;
using System.Security.Claims;

namespace HabitChain.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateAccessToken(string token);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId);
    Task<bool> SaveRefreshTokenAsync(string userId, string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    DateTime GetTokenExpiration();
} 