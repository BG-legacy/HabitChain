using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<User> userManager,
        IJwtService jwtService,
        IMapper mapper)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Update last login
        await UpdateLastLoginAsync(user.Id);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // Save refresh token
        await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            User = userDto
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        existingUser = await _userManager.FindByNameAsync(registerDto.Username);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this username already exists.");
        }

        // Create new user
        var user = new User
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // Save refresh token
        await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

        // Update last login
        await UpdateLastLoginAsync(user.Id);

        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            User = userDto
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Find the refresh token in the database
        var tokenEntity = await _userManager.Users
            .Where(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow))
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync();

        if (tokenEntity == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // Validate the refresh token
        var isValidToken = await _jwtService.ValidateRefreshTokenAsync(refreshToken, tokenEntity.Id);
        if (!isValidToken)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        // Revoke the old refresh token
        await _jwtService.RevokeRefreshTokenAsync(refreshToken);

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(tokenEntity);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        
        // Save new refresh token
        await _jwtService.SaveRefreshTokenAsync(tokenEntity.Id, newRefreshToken);

        var userDto = _mapper.Map<UserDto>(tokenEntity);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(),
            User = userDto
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        return await _jwtService.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        return result.Succeeded;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<bool> UpdateLastLoginAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
} 