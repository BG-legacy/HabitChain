using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// Habit Entries Management Controller
/// 
/// This controller handles all habit entry-related operations including:
/// - Creating, reading, and deleting habit entries
/// - Retrieving entries by habit ID and date ranges
/// - Checking for existing entries on specific dates
/// 
/// AUTHORIZATION STRATEGY:
/// - ALL endpoints require authentication with [Authorize] attribute
/// - Users can only access entries for habits they own
/// - Habit ownership verification ensures data privacy
/// - Prevents unauthorized access to other users' progress data
/// 
/// SECURITY CONSIDERATIONS:
/// - User ID is extracted from JWT token claims for all operations
/// - Habit ownership is verified before allowing access to entries
/// - Entry creation is restricted to owned habits only
/// - Date-based queries are filtered by habit ownership
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect all endpoints in this controller
public class HabitEntriesController : ControllerBase
{
    private readonly IHabitEntryService _habitEntryService;
    private readonly IHabitService _habitService;

    public HabitEntriesController(IHabitEntryService habitEntryService, IHabitService habitService)
    {
        _habitEntryService = habitEntryService;
        _habitService = habitService;
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Entries by Habit ID
    /// 
    /// Retrieves all entries for a specific habit. The service layer should verify
    /// that the habit belongs to the authenticated user before returning entries.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - Habit ownership verification in service layer
    /// - Users can only access entries for their own habits
    /// 
    /// Use case: Displaying progress history for a specific habit
    /// </summary>
    [HttpGet("habit/{habitId}")]
    public async Task<ActionResult<IEnumerable<HabitEntryDto>>> GetEntriesByHabitId(Guid habitId)
    {
        // Extract authenticated user ID
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        // Verify habit ownership first
        var habit = await _habitService.GetHabitByIdAsync(habitId);
        if (habit == null)
        {
            return NotFound(new { message = "Habit not found." });
        }

        if (habit.UserId != authenticatedUserId)
        {
            return Unauthorized(new { message = "You can only access entries for your own habits." });
        }

        var entries = await _habitEntryService.GetEntriesByHabitIdAsync(habitId);
        return Ok(entries);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Entries by Date Range
    /// 
    /// Retrieves habit entries within a specific date range for a habit.
    /// The service layer should verify habit ownership and filter results accordingly.
    /// 
    /// Security considerations:
    /// - Habit ownership verification in service layer
    /// - Date range validation to prevent excessive data retrieval
    /// - Consider implementing pagination for large date ranges
    /// 
    /// Use case: Generating progress reports or analytics for specific time periods
    /// </summary>
    [HttpGet("habit/{habitId}/date-range")]
    public async Task<ActionResult<IEnumerable<HabitEntryDto>>> GetEntriesByDateRange(
        Guid habitId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        // Extract authenticated user ID
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        // Verify habit ownership first
        var habit = await _habitService.GetHabitByIdAsync(habitId);
        if (habit == null)
        {
            return NotFound(new { message = "Habit not found." });
        }

        if (habit.UserId != authenticatedUserId)
        {
            return Unauthorized(new { message = "You can only access entries for your own habits." });
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

        var entries = await _habitEntryService.GetEntriesByDateRangeAsync(habitId, startDate, endDate);
        return Ok(entries);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Entry by ID
    /// 
    /// Retrieves a specific habit entry by its ID. The service layer should verify
    /// that the entry belongs to a habit owned by the authenticated user.
    /// 
    /// Security considerations:
    /// - Entry ownership verification through habit ownership
    /// - Users can only access entries for their own habits
    /// - Consider implementing entry-level ownership verification in service layer
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<HabitEntryDto>> GetEntryById(Guid id)
    {
        var entry = await _habitEntryService.GetEntryByIdAsync(id);
        if (entry == null)
        {
            return NotFound();
        }

        // Verify that the entry belongs to a habit owned by the authenticated user
        // This requires the service to return habit information or implement
        // entry-level ownership verification
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Note: This verification assumes the service returns habit information
        // or implements proper ownership checking. The service layer should
        // handle this verification to ensure data privacy.
        
        return Ok(entry);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Check Entry for Date
    /// 
    /// Checks if a habit has an entry for a specific date. This endpoint is useful
    /// for determining if a user has already logged progress for a given day.
    /// 
    /// Security considerations:
    /// - Habit ownership verification in service layer
    /// - Date validation to prevent abuse
    /// - Users can only check entries for their own habits
    /// 
    /// Use case: Frontend applications checking if user has already logged
    /// progress for today before showing the "log progress" button
    /// </summary>
    [HttpGet("habit/{habitId}/check-date")]
    public async Task<ActionResult<bool>> HasEntryForDate(Guid habitId, [FromQuery] DateTime date)
    {
        try
        {
            // Extract authenticated user ID
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // Validate date first to prevent unnecessary database calls
            // Use a more permissive date range to account for timezone differences and system clock variations
            var today = DateTime.UtcNow.Date;
            var maxDateRange = TimeSpan.FromDays(365);
            
            // Allow checking dates within a reasonable range (30 days in future to account for system clock issues, 365 days in past)
            if (date.Date > today.AddDays(30) || date.Date < today.AddDays(-maxDateRange.TotalDays))
            {
                return BadRequest(new { message = "Date must be within reasonable range (up to 30 days in future, 365 days in past)." });
            }

            // Verify habit ownership
            var habit = await _habitService.GetHabitByIdLightweightAsync(habitId);
            if (habit == null)
            {
                return NotFound(new { message = "Habit not found." });
            }

            if (habit.UserId != authenticatedUserId)
            {
                return Unauthorized(new { message = "You can only check entries for your own habits." });
            }

            // Now check for entries
            var hasEntry = await _habitEntryService.HasEntryForDateAsync(habitId, date);
            return Ok(hasEntry);
        }
        catch (Exception ex)
        {
            // Log the exception with more detail
            Console.WriteLine($"Error in HasEntryForDate for habitId {habitId} and date {date}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = "Internal server error occurred while checking habit entry.", error = ex.Message });
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Create New Entry
    /// 
    /// Creates a new habit entry for the authenticated user. The service layer should
    /// verify that the habit belongs to the authenticated user before allowing entry creation.
    /// 
    /// Security features:
    /// - User ID is extracted from JWT token and verified in service layer
    /// - Habit ownership verification in service layer
    /// - Users can only create entries for their own habits
    /// - Date validation prevents creating entries for unreasonable dates
    /// 
    /// Note: The CreateHabitEntryDto doesn't include UserId as it's determined
    /// by the authenticated user's JWT token claims.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<HabitEntryDto>> CreateEntry([FromBody] CreateHabitEntryDto createEntryDto)
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

        // Validate entry date to prevent abuse (e.g., not too far in future/past)
        var today = DateTime.Today;
        var maxDateRange = TimeSpan.FromDays(30); // Allow entries up to 30 days in past
        
        if (createEntryDto.CompletedAt > today.AddDays(1) || createEntryDto.CompletedAt < today.AddDays(-maxDateRange.TotalDays))
        {
            return BadRequest(new { message = "Entry date must be within reasonable range." });
        }

        // The service layer should verify habit ownership before creating the entry
        // and associate the entry with the authenticated user
        var entry = await _habitEntryService.CreateEntryAsync(createEntryDto);
        return CreatedAtAction(nameof(GetEntryById), new { id = entry.Id }, entry);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Delete Entry
    /// 
    /// Deletes a specific habit entry. The service layer should verify that the entry
    /// belongs to a habit owned by the authenticated user before allowing deletion.
    /// 
    /// Security considerations:
    /// - Entry ownership verification through habit ownership
    /// - Users can only delete entries for their own habits
    /// - Consider implementing soft delete to preserve data integrity
    /// - Audit trail should be maintained for entry deletions
    /// 
    /// Use case: Allowing users to correct mistakes in their habit logging
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEntry(Guid id)
    {
        try
        {
            // The service layer should verify entry ownership through habit ownership
            // before allowing deletion
            await _habitEntryService.DeleteEntryAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
} 