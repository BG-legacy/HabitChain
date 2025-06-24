using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// Authentication and Authorization Controller
/// 
/// This controller handles all authentication-related operations including:
/// - User registration and login (public endpoints)
/// - Token management (refresh, revoke)
/// - Password changes
/// - Current user information retrieval
/// 
/// AUTHORIZATION STRATEGY:
/// - Public endpoints: /register, /login, /refresh (no authentication required)
/// - Protected endpoints: /revoke, /change-password, /me (require valid JWT token)
/// 
/// The [Authorize] attribute ensures that protected endpoints can only be accessed
/// by authenticated users with valid JWT tokens. The token must be included in
/// the Authorization header as "Bearer {token}".
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// PUBLIC ENDPOINT - User Registration
    /// 
    /// Allows new users to create an account. This endpoint is intentionally
    /// not protected with [Authorize] because users need to register before
    /// they can authenticate and receive a JWT token.
    /// 
    /// Security considerations:
    /// - Input validation is handled by the service layer
    /// - Password hashing is performed in the service layer
    /// - Email uniqueness is enforced at the database level
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during registration." });
        }
    }

    /// <summary>
    /// PUBLIC ENDPOINT - User Login
    /// 
    /// Allows existing users to authenticate and receive JWT tokens.
    /// This endpoint is intentionally not protected because users need to
    /// login to receive their first JWT token.
    /// 
    /// Returns:
    /// - Access token (short-lived, typically 15-60 minutes)
    /// - Refresh token (long-lived, typically 7-30 days)
    /// - User information
    /// 
    /// Security considerations:
    /// - Credentials are validated against the database
    /// - JWT tokens are generated with user claims
    /// - Refresh tokens are stored securely for later validation
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during login." });
        }
    }

    /// <summary>
    /// PUBLIC ENDPOINT - Token Refresh
    /// 
    /// Allows users to obtain a new access token using their refresh token.
    /// This endpoint is public because access tokens expire and users need
    /// to refresh them without re-authenticating.
    /// 
    /// Security considerations:
    /// - Refresh token is validated against stored tokens
    /// - New access token is generated with updated expiration
    /// - Old refresh token is invalidated (token rotation)
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during token refresh." });
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Token Revocation
    /// 
    /// Allows authenticated users to revoke their refresh token, effectively
    /// logging them out. This endpoint requires a valid JWT token because:
    /// - Only authenticated users should be able to revoke their own tokens
    /// - Prevents unauthorized token revocation
    /// - Ensures proper session management
    /// 
    /// The [Authorize] attribute ensures that:
    /// - The request includes a valid JWT token in the Authorization header
    /// - The token contains valid user claims
    /// - The user's identity is available via User.FindFirst(ClaimTypes.NameIdentifier)
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RevokeTokenAsync(request.RefreshToken);
            if (result)
            {
                return Ok(new { message = "Token revoked successfully." });
            }
            return BadRequest(new { message = "Failed to revoke token." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during token revocation." });
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Password Change
    /// 
    /// Allows authenticated users to change their password. This endpoint
    /// requires authentication because:
    /// - Only the user should be able to change their own password
    /// - Current password verification is required
    /// - Prevents unauthorized password changes
    /// 
    /// Security features:
    /// - User ID is extracted from JWT token claims
    /// - Current password is verified before allowing change
    /// - New password is hashed before storage
    /// - Password change invalidates existing refresh tokens
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            // Extract user ID from JWT token claims
            // This ensures the user can only change their own password
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found." });
            }

            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            if (result)
            {
                return Ok(new { message = "Password changed successfully." });
            }
            return BadRequest(new { message = "Failed to change password." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred during password change." });
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Current User Information
    /// 
    /// Returns information about the currently authenticated user.
    /// This endpoint requires authentication because:
    /// - Only authenticated users should access their own information
    /// - User ID is extracted from JWT token claims
    /// - Prevents unauthorized access to user data
    /// 
    /// The [Authorize] attribute ensures that:
    /// - The request includes a valid JWT token
    /// - User claims are available in the User property
    /// - The user's identity is verified before returning data
    /// 
    /// This endpoint is commonly used by frontend applications to:
    /// - Display user profile information
    /// - Verify authentication status
    /// - Get user preferences and settings
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            // Extract user ID from JWT token claims
            // This ensures we return data for the authenticated user only
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found." });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user information." });
        }
    }
} 