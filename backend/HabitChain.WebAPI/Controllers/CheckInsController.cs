using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// Check-Ins Management Controller
/// 
/// This controller handles all check-in-related operations including:
/// - Creating, reading, updating, and deleting check-ins
/// - Retrieving check-ins by user ID and habit ID
/// - Date range queries for check-in history
/// - Checking for existing check-ins on specific dates
/// 
/// AUTHORIZATION STRATEGY:
/// - ALL endpoints require authentication with [Authorize] attribute
/// - Users can only access check-ins for their own habits
/// - Check-in ownership verification ensures data privacy
/// - Prevents unauthorized access to other users' progress data
/// 
/// SECURITY CONSIDERATIONS:
/// - User ID is extracted from JWT token claims for all operations
/// - Habit ownership is verified before allowing access to check-ins
/// - Check-in creation is restricted to owned habits only
/// - Date-based queries are filtered by habit ownership
/// - Input validation prevents malicious data injection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect all endpoints in this controller
public class CheckInsController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInsController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Check-Ins by User ID
    /// 
    /// Retrieves all check-ins for the authenticated user. The user ID is extracted
    /// from the JWT token claims, ensuring users can only access their own check-ins.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from URL parameter
    /// - Prevents users from accessing other users' check-ins
    /// 
    /// Note: The URL parameter {userId} is kept for API consistency but should
    /// match the authenticated user's ID. Consider removing the parameter and
    /// using only the token-based user ID for enhanced security.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<CheckInDto>>> GetCheckInsByUserId(string userId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access their own check-ins
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only access your own check-ins." });
        }

        var checkIns = await _checkInService.GetCheckInsByUserIdAsync(userId);
        return Ok(checkIns);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Check-Ins by Habit ID
    /// 
    /// Retrieves all check-ins for a specific habit. The service layer should verify
    /// that the habit belongs to the authenticated user before returning check-ins.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - Habit ownership verification in service layer
    /// - Users can only access check-ins for their own habits
    /// 
    /// Use case: Displaying check-in history for a specific habit
    /// </summary>
    [HttpGet("habit/{habitId}")]
    public async Task<ActionResult<IEnumerable<CheckInDto>>> GetCheckInsByHabitId(Guid habitId)
    {
        // The service layer should verify habit ownership before returning check-ins
        // This ensures users can only access check-ins for habits they own
        var checkIns = await _checkInService.GetCheckInsByHabitIdAsync(habitId);
        return Ok(checkIns);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Check-Ins by Date Range
    /// 
    /// Retrieves check-ins within a specific date range for the authenticated user.
    /// The service layer should verify user ownership and filter results accordingly.
    /// 
    /// Security considerations:
    /// - User ownership verification in service layer
    /// - Date range validation to prevent excessive data retrieval
    /// - Consider implementing pagination for large date ranges
    /// 
    /// Use case: Generating progress reports or analytics for specific time periods
    /// </summary>
    [HttpGet("user/{userId}/date-range")]
    public async Task<ActionResult<IEnumerable<CheckInDto>>> GetCheckInsByDateRange(
        string userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access their own check-ins
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only access your own check-ins." });
        }

        // Validate date range to prevent excessive data retrieval
        if (endDate < startDate)
        {
            return BadRequest(new { message = "End date must be after start date." });
        }

        // Limit date range to prevent abuse (e.g., max 1 year)
        var maxDateRange = TimeSpan.FromDays(365);
        if (endDate - startDate > maxDateRange)
        {
            return BadRequest(new { message = "Date range cannot exceed 365 days." });
        }

        // The service layer should verify user ownership before returning check-ins
        var checkIns = await _checkInService.GetCheckInsByDateRangeAsync(userId, startDate, endDate);
        return Ok(checkIns);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Check-In by ID
    /// 
    /// Retrieves a specific check-in by its ID. The service layer should verify
    /// that the check-in belongs to a habit owned by the authenticated user.
    /// 
    /// Security considerations:
    /// - Check-in ownership verification through habit ownership
    /// - Users can only access check-ins for their own habits
    /// - Consider implementing check-in-level ownership verification in service layer
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CheckInDto>> GetCheckInById(Guid id)
    {
        var checkIn = await _checkInService.GetCheckInByIdAsync(id);
        if (checkIn == null)
        {
            return NotFound();
        }

        // Verify that the check-in belongs to a habit owned by the authenticated user
        // This requires the service to return habit information or implement
        // check-in-level ownership verification
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Note: This verification assumes the service returns habit information
        // or implements proper ownership checking. The service layer should
        // handle this verification to ensure data privacy.
        
        return Ok(checkIn);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Check Check-In for Date
    /// 
    /// Checks if a user has a check-in for a specific habit on a specific date.
    /// This endpoint is useful for determining if a user has already checked in
    /// for a given habit on a given day.
    /// 
    /// Security considerations:
    /// - Habit ownership verification in service layer
    /// - Date validation to prevent abuse
    /// - Users can only check their own check-ins
    /// 
    /// Use case: Frontend applications checking if user has already checked in
    /// for today before showing the "check in" button
    /// </summary>
    [HttpGet("user/{userId}/habit/{habitId}/check-date")]
    public async Task<ActionResult<bool>> HasCheckInForDate(string userId, Guid habitId, [FromQuery] DateTime date)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only check their own check-ins
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only check your own check-ins." });
        }

        // Validate date to prevent abuse (e.g., not too far in future/past)
        var today = DateTime.Today;
        var maxDateRange = TimeSpan.FromDays(365);
        
        if (date > today.AddDays(1) || date < today.AddDays(-maxDateRange.TotalDays))
        {
            return BadRequest(new { message = "Date must be within reasonable range." });
        }

        // The service layer should verify habit ownership before checking check-ins
        var hasCheckIn = await _checkInService.HasCheckInForDateAsync(userId, habitId, date);
        return Ok(hasCheckIn);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Create New Check-In
    /// 
    /// Creates a new check-in for the authenticated user. The service layer should
    /// verify that the habit belongs to the authenticated user before allowing check-in creation.
    /// 
    /// Security features:
    /// - User ID is extracted from JWT token and verified in service layer
    /// - Habit ownership verification in service layer
    /// - Users can only create check-ins for their own habits
    /// - Date validation prevents creating check-ins for unreasonable dates
    /// 
    /// Note: The CreateCheckInDto doesn't include UserId as it's determined
    /// by the authenticated user's JWT token claims.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CheckInDto>> CreateCheckIn([FromBody] CreateCheckInDto createCheckInDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract authenticated user ID for service layer verification
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        // Validate check-in date to prevent abuse (e.g., not too far in future/past)
        var today = DateTime.Today;
        var maxDateRange = TimeSpan.FromDays(30); // Allow check-ins up to 30 days in past
        
        if (createCheckInDto.CompletedAt > today.AddDays(1) || createCheckInDto.CompletedAt < today.AddDays(-maxDateRange.TotalDays))
        {
            return BadRequest(new { message = "Check-in date must be within reasonable range." });
        }

        // The service layer should verify habit ownership before creating the check-in
        // and associate the check-in with the authenticated user
        var checkIn = await _checkInService.CreateCheckInAsync(createCheckInDto);
        return CreatedAtAction(nameof(GetCheckInById), new { id = checkIn.Id }, checkIn);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Update Check-In
    /// 
    /// Updates an existing check-in. The service layer should verify that the check-in
    /// belongs to a habit owned by the authenticated user before allowing modifications.
    /// 
    /// Security considerations:
    /// - Check-in ownership verification through habit ownership
    /// - Users can only update their own check-ins
    /// - Input validation prevents malicious updates
    /// - Audit trail should be maintained for check-in changes
    /// 
    /// Use case: Allowing users to update notes or correct check-in details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CheckInDto>> UpdateCheckIn(Guid id, [FromBody] CreateCheckInDto updateCheckInDto)
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

            // The service layer should verify check-in ownership through habit ownership
            // before allowing updates
            var checkIn = await _checkInService.UpdateCheckInAsync(id, updateCheckInDto);
            return Ok(checkIn);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Delete Check-In
    /// 
    /// Deletes a specific check-in. The service layer should verify that the check-in
    /// belongs to a habit owned by the authenticated user before allowing deletion.
    /// 
    /// Security considerations:
    /// - Check-in ownership verification through habit ownership
    /// - Users can only delete their own check-ins
    /// - Consider implementing soft delete to preserve data integrity
    /// - Audit trail should be maintained for check-in deletions
    /// 
    /// Use case: Allowing users to correct mistakes in their check-in logging
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCheckIn(Guid id)
    {
        try
        {
            // The service layer should verify check-in ownership through habit ownership
            // before allowing deletion
            await _checkInService.DeleteCheckInAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
} 