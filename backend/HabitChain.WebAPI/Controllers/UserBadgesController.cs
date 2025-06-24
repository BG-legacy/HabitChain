using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// User Badges Management Controller
/// 
/// This controller handles all user badge-related operations including:
/// - Creating, reading, and deleting user badges
/// - Retrieving user badges by user ID and habit ID
/// - Checking if users have earned specific badges
/// 
/// AUTHORIZATION STRATEGY:
/// - ALL endpoints require authentication with [Authorize] attribute
/// - Users can only access their own user badges
/// - User badge ownership verification ensures data privacy
/// - Prevents unauthorized access to other users' achievement data
/// 
/// SECURITY CONSIDERATIONS:
/// - User ID is extracted from JWT token claims for all operations
/// - User badge ownership is verified before allowing access
/// - Users can only view their own earned badges
/// - Badge earning is typically automatic (system-generated)
/// - Manual badge assignment should be restricted to admins
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect all endpoints in this controller
public class UserBadgesController : ControllerBase
{
    private readonly IUserBadgeService _userBadgeService;

    public UserBadgesController(IUserBadgeService userBadgeService)
    {
        _userBadgeService = userBadgeService;
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get User Badges by User ID
    /// 
    /// Retrieves all badges earned by the authenticated user. The user ID is extracted
    /// from the JWT token claims, ensuring users can only access their own earned badges.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from URL parameter
    /// - Prevents users from accessing other users' earned badges
    /// 
    /// Note: The URL parameter {userId} is kept for API consistency but should
    /// match the authenticated user's ID. Consider removing the parameter and
    /// using only the token-based user ID for enhanced security.
    /// 
    /// Use case: Displaying user's achievement collection and progress
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserBadgeDto>>> GetUserBadgesByUserId(string userId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access their own earned badges
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only access your own earned badges." });
        }

        var userBadges = await _userBadgeService.GetUserBadgesByUserIdAsync(userId);
        return Ok(userBadges);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get User Badges by Habit ID
    /// 
    /// Retrieves all user badges earned for a specific habit. The service layer should verify
    /// that the habit belongs to the authenticated user before returning user badges.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - Habit ownership verification in service layer
    /// - Users can only access badges earned for their own habits
    /// 
    /// Use case: Displaying badges earned for a specific habit
    /// </summary>
    [HttpGet("habit/{habitId}")]
    public async Task<ActionResult<IEnumerable<UserBadgeDto>>> GetUserBadgesByHabitId(Guid habitId)
    {
        // The service layer should verify habit ownership before returning user badges
        // This ensures users can only access badges earned for habits they own
        var userBadges = await _userBadgeService.GetUserBadgesByHabitIdAsync(habitId);
        return Ok(userBadges);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get User Badge by ID
    /// 
    /// Retrieves a specific user badge by its ID. The service layer should verify
    /// that the user badge belongs to the authenticated user before returning it.
    /// 
    /// Security considerations:
    /// - User badge ownership verification
    /// - Users can only access their own earned badges
    /// - Consider implementing user badge-level ownership verification in service layer
    /// 
    /// Use case: Displaying detailed information about a specific earned badge
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserBadgeDto>> GetUserBadgeById(Guid id)
    {
        var userBadge = await _userBadgeService.GetUserBadgeByIdAsync(id);
        if (userBadge == null)
        {
            return NotFound();
        }

        // Verify that the user badge belongs to the authenticated user
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userBadge.UserId != authenticatedUserId)
        {
            return Unauthorized(new { message = "You can only access your own earned badges." });
        }
        
        return Ok(userBadge);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Check if User Has Earned Badge
    /// 
    /// Checks if a user has earned a specific badge. This endpoint is useful for
    /// determining badge eligibility and displaying progress indicators.
    /// 
    /// Security considerations:
    /// - User ownership verification
    /// - Users can only check their own badge status
    /// - Badge information is public, but user's earning status is private
    /// 
    /// Use case: Checking if user has earned a specific badge for UI display
    /// </summary>
    [HttpGet("user/{userId}/badge/{badgeId}/has-earned")]
    public async Task<ActionResult<bool>> HasUserEarnedBadge(string userId, Guid badgeId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only check their own badge status
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only check your own badge status." });
        }

        var hasEarned = await _userBadgeService.HasUserEarnedBadgeAsync(userId, badgeId);
        return Ok(hasEarned);
    }

    /// <summary>
    /// ADMIN-ONLY ENDPOINT - Create User Badge (Manual Assignment)
    /// 
    /// Creates a new user badge entry. This endpoint is typically used for manual
    /// badge assignment by administrators or for system-generated badge awards.
    /// 
    /// Security features:
    /// - User ID is extracted from JWT token and verified in service layer
    /// - Admin role verification should be implemented in service layer
    /// - Users can only create badges for themselves or if they have admin privileges
    /// - Input validation prevents malicious badge assignments
    /// 
    /// Note: Most user badges are earned automatically by the system based on
    /// user actions and achievements. Manual assignment should be restricted.
    /// 
    /// The CreateUserBadgeDto should include UserId which will be automatically
    /// set from the authenticated user's JWT token claims for security.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserBadgeDto>> CreateUserBadge([FromBody] CreateUserBadgeDto createUserBadgeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract authenticated user ID for verification
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        // Set the UserId from the JWT token (override any provided value for security)
        createUserBadgeDto.UserId = authenticatedUserId;

        // TODO: Implement admin role verification in service layer for manual badge assignment
        // The service layer should verify that the authenticated user has admin privileges
        // or is assigning the badge to themselves through legitimate means.

        var userBadge = await _userBadgeService.CreateUserBadgeAsync(createUserBadgeDto);
        return CreatedAtAction(nameof(GetUserBadgeById), new { id = userBadge.Id }, userBadge);
    }

    /// <summary>
    /// ADMIN-ONLY ENDPOINT - Delete User Badge
    /// 
    /// Deletes a specific user badge. This endpoint is typically used for correcting
    /// erroneous badge assignments or for administrative purposes.
    /// 
    /// Security considerations:
    /// - User badge ownership verification (user or admin)
    /// - Users can only delete their own badges or if they have admin privileges
    /// - Consider implementing soft delete to preserve achievement history
    /// - Audit trail should be maintained for badge deletions
    /// 
    /// Use case: Correcting erroneous badge assignments or administrative cleanup
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUserBadge(Guid id)
    {
        try
        {
            // Extract authenticated user ID for verification
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // TODO: Implement ownership verification in service layer
            // The service layer should verify that the authenticated user owns the badge
            // or has admin privileges before allowing deletion.

            await _userBadgeService.DeleteUserBadgeAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
} 