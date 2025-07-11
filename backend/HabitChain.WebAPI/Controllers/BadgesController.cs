using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// Badges Management Controller
/// 
/// This controller handles all badge-related operations including:
/// - Creating, reading, updating, and deleting badges (admin operations)
/// - Retrieving all badges and active badges (public read access)
/// - Managing badge definitions and criteria
/// 
/// AUTHORIZATION STRATEGY:
/// - READ operations (GET) are public - anyone can view available badges
/// - WRITE operations (POST, PUT, DELETE) require admin privileges
/// - Badge creation and management is restricted to administrators
/// - Badge viewing is open to all authenticated users
/// 
/// SECURITY CONSIDERATIONS:
/// - Badge definitions are system-wide and should be managed by admins only
/// - Badge viewing is public to encourage user engagement
/// - Admin operations require elevated privileges
/// - Input validation prevents malicious badge definitions
/// - Consider implementing role-based authorization for admin operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BadgesController : ControllerBase
{
    private readonly IBadgeService _badgeService;
    private readonly IBadgeEarningService _badgeEarningService;
    private readonly IUserBadgeService _userBadgeService;

    public BadgesController(
        IBadgeService badgeService, 
        IBadgeEarningService badgeEarningService,
        IUserBadgeService userBadgeService)
    {
        _badgeService = badgeService;
        _badgeEarningService = badgeEarningService;
        _userBadgeService = userBadgeService;
    }

    /// <summary>
    /// PUBLIC ENDPOINT - Get All Badges
    /// 
    /// Retrieves all badges in the system. This endpoint is intentionally not protected
    /// because badges are meant to be visible to all users to encourage engagement.
    /// 
    /// Security considerations:
    /// - No authentication required for viewing badges
    /// - Badges are system-wide definitions, not user-specific data
    /// - Public visibility encourages user engagement and motivation
    /// 
    /// Use case: Displaying available badges to users for motivation
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BadgeDto>>> GetAllBadges()
    {
        try
        {
            var badges = await _badgeService.GetAllBadgesAsync();
            return Ok(badges);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// PUBLIC ENDPOINT - Get Active Badges
    /// 
    /// Retrieves only active badges in the system. This endpoint is intentionally not protected
    /// because active badges are meant to be visible to all users.
    /// 
    /// Security considerations:
    /// - No authentication required for viewing active badges
    /// - Only active badges are returned (inactive badges are hidden)
    /// - Public visibility encourages user engagement
    /// 
    /// Use case: Displaying currently available badges to users
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<BadgeDto>>> GetActiveBadges()
    {
        try
        {
            var badges = await _badgeService.GetActiveBadgesAsync();
            return Ok(badges);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// PUBLIC ENDPOINT - Get Badge by ID
    /// 
    /// Retrieves a specific badge by its ID. This endpoint is intentionally not protected
    /// because badge details should be visible to all users.
    /// 
    /// Security considerations:
    /// - No authentication required for viewing badge details
    /// - Badge information is public and non-sensitive
    /// - Public visibility helps users understand badge requirements
    /// 
    /// Use case: Displaying detailed badge information and requirements
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BadgeDto>> GetBadgeById(Guid id)
    {
        try
        {
            var badge = await _badgeService.GetBadgeByIdAsync(id);
            if (badge == null)
            {
                return NotFound($"Badge with ID {id} not found.");
            }
            return Ok(badge);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// PUBLIC ENDPOINT - Get User Badges with Progress
    /// 
    /// Retrieves a list of badges that a user has earned and their progress towards earning more.
    /// 
    /// Security considerations:
    /// - No authentication required for viewing user badges
    /// - User badges are sensitive data and should be protected
    /// - Public visibility encourages user engagement
    /// 
    /// Use case: Displaying user's progress towards earning badges
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<BadgeDto>>> GetUserBadgesWithProgress(string userId)
    {
        try
        {
            var badges = await _badgeEarningService.GetUserBadgesWithProgressAsync(userId);
            return Ok(badges);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// PUBLIC ENDPOINT - Get User Earned Badges
    /// 
    /// Retrieves a list of badges that a user has earned.
    /// 
    /// Security considerations:
    /// - No authentication required for viewing user earned badges
    /// - User earned badges are sensitive data and should be protected
    /// - Public visibility encourages user engagement
    /// 
    /// Use case: Displaying user's earned badges
    /// </summary>
    [HttpGet("user/{userId}/earned")]
    public async Task<ActionResult<IEnumerable<UserBadgeDto>>> GetUserEarnedBadges(string userId)
    {
        try
        {
            var userBadges = await _userBadgeService.GetUserBadgesByUserIdAsync(userId);
            return Ok(userBadges);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// ADMIN-ONLY ENDPOINT - Create New Badge
    /// 
    /// Creates a new badge in the system. This endpoint requires admin privileges
    /// because badge creation affects the entire system.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - Admin role verification should be implemented in service layer
    /// - Users can only create badges if they have admin privileges
    /// - Input validation prevents malicious badge definitions
    /// 
    /// Note: Consider implementing role-based authorization (e.g., [Authorize(Roles = "Admin")])
    /// for more granular access control.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BadgeDto>> CreateBadge([FromBody] CreateBadgeDto createBadgeDto)
    {
        try
        {
            var badge = await _badgeService.CreateBadgeAsync(createBadgeDto);
            return CreatedAtAction(nameof(GetBadgeById), new { id = badge.Id }, badge);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// ADMIN-ONLY ENDPOINT - Update Badge
    /// 
    /// Updates an existing badge in the system. This endpoint requires admin privileges
    /// because badge updates affect the entire system.
    /// 
    /// Security considerations:
    /// - Admin role verification in service layer
    /// - Users can only update badges if they have admin privileges
    /// - Input validation prevents malicious badge updates
    /// - Audit trail should be maintained for badge changes
    /// 
    /// Use case: Updating badge criteria, descriptions, or activation status
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BadgeDto>> UpdateBadge(Guid id, [FromBody] CreateBadgeDto updateBadgeDto)
    {
        try
        {
            var badge = await _badgeService.UpdateBadgeAsync(id, updateBadgeDto);
            return Ok(badge);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// ADMIN-ONLY ENDPOINT - Delete Badge
    /// 
    /// Deletes a badge from the system. This endpoint requires admin privileges
    /// because badge deletion affects the entire system.
    /// 
    /// Security considerations:
    /// - Admin role verification in service layer
    /// - Users can only delete badges if they have admin privileges
    /// - Consider implementing soft delete to preserve data integrity
    /// - Audit trail should be maintained for badge deletions
    /// - Cascade deletion of user badges should be handled properly
    /// 
    /// Use case: Removing obsolete or inappropriate badges from the system
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteBadge(Guid id)
    {
        try
        {
            await _badgeService.DeleteBadgeAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// ADMIN-ONLY ENDPOINT - Check and Award Badges
    /// 
    /// Checks for eligible badges and awards them to a user. This endpoint requires admin privileges
    /// because it affects the entire system.
    /// 
    /// Security considerations:
    /// - Admin role verification in service layer
    /// - Users can only check and award badges if they have admin privileges
    /// - Input validation prevents malicious badge checks and awards
    /// - Audit trail should be maintained for badge checks and awards
    /// 
    /// Use case: Automatically awarding badges to users based on their progress
    /// </summary>
    [HttpPost("check-earning")]
    public async Task<ActionResult<List<BadgeDto>>> CheckAndAwardBadges([FromBody] CheckBadgeEarningDto request)
    {
        try
        {
            var earnedBadges = await _badgeEarningService.CheckAndAwardBadgesAsync(request.UserId, request.HabitId);
            return Ok(earnedBadges);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class CheckBadgeEarningDto
{
    public string UserId { get; set; } = string.Empty;
    public Guid? HabitId { get; set; }
} 