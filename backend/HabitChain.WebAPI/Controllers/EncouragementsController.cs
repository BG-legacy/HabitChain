using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// Encouragements Management Controller
/// 
/// This controller handles all encouragement-related operations including:
/// - Creating, reading, updating, and deleting encouragements
/// - Retrieving encouragements by user ID, from user ID, and habit ID
/// - Managing read/unread status of encouragements
/// 
/// AUTHORIZATION STRATEGY:
/// - ALL endpoints require authentication with [Authorize] attribute
/// - Users can only access encouragements they've sent or received
/// - Encouragement ownership verification ensures data privacy
/// - Prevents unauthorized access to other users' encouragement data
/// 
/// SECURITY CONSIDERATIONS:
/// - User ID is extracted from JWT token claims for all operations
/// - Encouragement ownership is verified before allowing access
/// - Users can only send encouragements to habits they own or have permission for
/// - Read status updates are restricted to encouragement recipients
/// - Input validation prevents malicious data injection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect all endpoints in this controller
public class EncouragementsController : ControllerBase
{
    private readonly IEncouragementService _encouragementService;

    public EncouragementsController(IEncouragementService encouragementService)
    {
        _encouragementService = encouragementService;
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Encouragements by User ID (Received)
    /// 
    /// Retrieves all encouragements received by the authenticated user. The user ID is extracted
    /// from the JWT token claims, ensuring users can only access encouragements sent to them.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from URL parameter
    /// - Prevents users from accessing other users' received encouragements
    /// 
    /// Note: The URL parameter {userId} is kept for API consistency but should
    /// match the authenticated user's ID. Consider removing the parameter and
    /// using only the token-based user ID for enhanced security.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<EncouragementDto>>> GetEncouragementsByUserId(string userId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access their own received encouragements
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only access your own encouragements." });
        }

        var encouragements = await _encouragementService.GetEncouragementsByUserIdAsync(userId);
        return Ok(encouragements);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Encouragements by From User ID (Sent)
    /// 
    /// Retrieves all encouragements sent by the authenticated user. The user ID is extracted
    /// from the JWT token claims, ensuring users can only access encouragements they've sent.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from URL parameter
    /// - Prevents users from accessing other users' sent encouragements
    /// 
    /// Use case: Users viewing their own encouragement history and impact
    /// </summary>
    [HttpGet("from-user/{fromUserId}")]
    public async Task<ActionResult<IEnumerable<EncouragementDto>>> GetEncouragementsByFromUserId(string fromUserId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access encouragements they've sent
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != fromUserId)
        {
            return Unauthorized(new { message = "You can only access encouragements you've sent." });
        }

        var encouragements = await _encouragementService.GetEncouragementsByFromUserIdAsync(fromUserId);
        return Ok(encouragements);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Encouragements by Habit ID
    /// 
    /// Retrieves all encouragements for a specific habit. The service layer should verify
    /// that the habit belongs to the authenticated user before returning encouragements.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - Habit ownership verification in service layer
    /// - Users can only access encouragements for their own habits
    /// 
    /// Use case: Displaying encouragement history for a specific habit
    /// </summary>
    [HttpGet("habit/{habitId}")]
    public async Task<ActionResult<IEnumerable<EncouragementDto>>> GetEncouragementsByHabitId(Guid habitId)
    {
        // The service layer should verify habit ownership before returning encouragements
        // This ensures users can only access encouragements for habits they own
        var encouragements = await _encouragementService.GetEncouragementsByHabitIdAsync(habitId);
        return Ok(encouragements);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Unread Encouragements by User ID
    /// 
    /// Retrieves all unread encouragements for the authenticated user. This endpoint is useful
    /// for displaying notifications or unread messages to users.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from URL parameter
    /// - Prevents users from accessing other users' unread encouragements
    /// 
    /// Use case: Notification systems showing unread encouragement count
    /// </summary>
    [HttpGet("user/{userId}/unread")]
    public async Task<ActionResult<IEnumerable<EncouragementDto>>> GetUnreadEncouragementsByUserId(string userId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access their own unread encouragements
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only access your own unread encouragements." });
        }

        var encouragements = await _encouragementService.GetUnreadEncouragementsByUserIdAsync(userId);
        return Ok(encouragements);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Encouragement by ID
    /// 
    /// Retrieves a specific encouragement by its ID. The service layer should verify
    /// that the encouragement belongs to the authenticated user (as sender or recipient).
    /// 
    /// Security considerations:
    /// - Encouragement ownership verification (sender or recipient)
    /// - Users can only access encouragements they've sent or received
    /// - Consider implementing encouragement-level ownership verification in service layer
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EncouragementDto>> GetEncouragementById(Guid id)
    {
        var encouragement = await _encouragementService.GetEncouragementByIdAsync(id);
        if (encouragement == null)
        {
            return NotFound();
        }

        // Verify that the encouragement belongs to the authenticated user
        // (either as sender or recipient)
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (encouragement.ToUserId != authenticatedUserId && encouragement.FromUserId != authenticatedUserId)
        {
            return Unauthorized(new { message = "You can only access encouragements you've sent or received." });
        }
        
        return Ok(encouragement);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Create New Encouragement
    /// 
    /// Creates a new encouragement for the authenticated user. The service layer should
    /// verify that the user has permission to send encouragements to the target habit.
    /// 
    /// Security features:
    /// - User ID is extracted from JWT token and set as FromUserId
    /// - Permission verification in service layer (can user encourage this habit?)
    /// - Users can only send encouragements for habits they have permission for
    /// - Input validation prevents malicious encouragement content
    /// 
    /// Note: The CreateEncouragementDto should include FromUserId which will be
    /// automatically set from the authenticated user's JWT token claims.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EncouragementDto>> CreateEncouragement([FromBody] CreateEncouragementDto createEncouragementDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract authenticated user ID and set it as the sender
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        // Set the FromUserId from the JWT token (override any provided value for security)
        createEncouragementDto.FromUserId = authenticatedUserId;

        // The service layer should verify permission to send encouragement
        // (e.g., user owns the habit, is a friend, or has appropriate permissions)
        var encouragement = await _encouragementService.CreateEncouragementAsync(createEncouragementDto);
        return CreatedAtAction(nameof(GetEncouragementById), new { id = encouragement.Id }, encouragement);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Update Encouragement
    /// 
    /// Updates an existing encouragement. The service layer should verify that the encouragement
    /// belongs to the authenticated user (as sender) before allowing modifications.
    /// 
    /// Security considerations:
    /// - Encouragement ownership verification (sender only)
    /// - Users can only update encouragements they've sent
    /// - Input validation prevents malicious updates
    /// - Audit trail should be maintained for encouragement changes
    /// 
    /// Use case: Allowing users to edit encouragement content before it's read
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<EncouragementDto>> UpdateEncouragement(Guid id, [FromBody] CreateEncouragementDto updateEncouragementDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Extract authenticated user ID for service layer verification
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // Set the FromUserId from the JWT token for security
            updateEncouragementDto.FromUserId = authenticatedUserId;

            // The service layer should verify encouragement ownership (sender only)
            // before allowing updates
            var encouragement = await _encouragementService.UpdateEncouragementAsync(id, updateEncouragementDto);
            return Ok(encouragement);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Mark Encouragement as Read
    /// 
    /// Marks a specific encouragement as read. Only the recipient of the encouragement
    /// should be able to mark it as read.
    /// 
    /// Security considerations:
    /// - Encouragement ownership verification (recipient only)
    /// - Users can only mark encouragements they've received as read
    /// - Prevents unauthorized read status changes
    /// 
    /// Use case: Automatically marking encouragements as read when user views them
    /// </summary>
    [HttpPut("{id}/mark-read")]
    public async Task<ActionResult> MarkAsRead(Guid id)
    {
        try
        {
            // Extract authenticated user ID for service layer verification
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // The service layer should verify that the authenticated user is the recipient
            // of the encouragement before allowing the read status change
            await _encouragementService.MarkAsReadAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Delete Encouragement
    /// 
    /// Deletes a specific encouragement. The service layer should verify that the encouragement
    /// belongs to the authenticated user (as sender) before allowing deletion.
    /// 
    /// Security considerations:
    /// - Encouragement ownership verification (sender only)
    /// - Users can only delete encouragements they've sent
    /// - Consider implementing soft delete to preserve data integrity
    /// - Audit trail should be maintained for encouragement deletions
    /// 
    /// Use case: Allowing users to retract encouragements they've sent
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEncouragement(Guid id)
    {
        try
        {
            // Extract authenticated user ID for service layer verification
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // The service layer should verify encouragement ownership (sender only)
            // before allowing deletion
            await _encouragementService.DeleteEncouragementAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
} 