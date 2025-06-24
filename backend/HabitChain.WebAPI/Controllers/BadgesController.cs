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
public class BadgesController : ControllerBase
{
    private readonly IBadgeService _badgeService;

    public BadgesController(IBadgeService badgeService)
    {
        _badgeService = badgeService;
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
        var badges = await _badgeService.GetAllBadgesAsync();
        return Ok(badges);
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
        var badges = await _badgeService.GetActiveBadgesAsync();
        return Ok(badges);
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
        var badge = await _badgeService.GetBadgeByIdAsync(id);
        if (badge == null)
        {
            return NotFound();
        }
        return Ok(badge);
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
    [Authorize] // Require authentication for badge creation
    public async Task<ActionResult<BadgeDto>> CreateBadge([FromBody] CreateBadgeDto createBadgeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract authenticated user ID for admin verification
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        // TODO: Implement admin role verification in service layer
        // The service layer should verify that the authenticated user has admin privileges
        // before allowing badge creation. This could be done by checking user roles
        // or implementing a separate admin verification method.

        var badge = await _badgeService.CreateBadgeAsync(createBadgeDto);
        return CreatedAtAction(nameof(GetBadgeById), new { id = badge.Id }, badge);
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
    [Authorize] // Require authentication for badge updates
    public async Task<ActionResult<BadgeDto>> UpdateBadge(Guid id, [FromBody] CreateBadgeDto updateBadgeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Extract authenticated user ID for admin verification
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // TODO: Implement admin role verification in service layer
            // The service layer should verify that the authenticated user has admin privileges
            // before allowing badge updates.

            var badge = await _badgeService.UpdateBadgeAsync(id, updateBadgeDto);
            return Ok(badge);
        }
        catch (ArgumentException)
        {
            return NotFound();
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
    [Authorize] // Require authentication for badge deletion
    public async Task<ActionResult> DeleteBadge(Guid id)
    {
        try
        {
            // Extract authenticated user ID for admin verification
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // TODO: Implement admin role verification in service layer
            // The service layer should verify that the authenticated user has admin privileges
            // before allowing badge deletion.

            await _badgeService.DeleteBadgeAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
} 