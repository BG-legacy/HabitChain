using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

/// <summary>
/// Habits Management Controller
/// 
/// This controller handles all habit-related operations including:
/// - Creating, reading, updating, and deleting habits
/// - Retrieving habits by user ID
/// - Managing active/inactive habits
/// 
/// AUTHORIZATION STRATEGY:
/// - ALL endpoints require authentication with [Authorize] attribute
/// - User can only access their own habits (user ID extracted from JWT token)
/// - Prevents unauthorized access to other users' habit data
/// - Ensures data privacy and security
/// 
/// SECURITY CONSIDERATIONS:
/// - User ID is extracted from JWT token claims for all operations
/// - Habit ownership is verified before allowing modifications
/// - Soft delete patterns may be implemented in the service layer
/// - Input validation prevents malicious data injection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Protect all endpoints in this controller
public class HabitsController : ControllerBase
{
    private readonly IHabitService _habitService;

    public HabitsController(IHabitService habitService)
    {
        _habitService = habitService;
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Habits by User ID
    /// 
    /// Retrieves all habits for the authenticated user. The user ID is extracted
    /// from the JWT token claims, ensuring users can only access their own habits.
    /// 
    /// Security features:
    /// - [Authorize] ensures only authenticated users can access
    /// - User ID is extracted from JWT token, not from URL parameter
    /// - Prevents users from accessing other users' habits
    /// 
    /// Note: The URL parameter {userId} is kept for API consistency but should
    /// match the authenticated user's ID. Consider removing the parameter and
    /// using only the token-based user ID for enhanced security.
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<HabitDto>>> GetHabitsByUserId(string userId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access their own habits
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only access your own habits." });
        }

        var habits = await _habitService.GetHabitsByUserIdAsync(userId);
        return Ok(habits);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Active Habits by User ID
    /// 
    /// Retrieves only active habits for the authenticated user. This endpoint
    /// follows the same security pattern as GetHabitsByUserId.
    /// 
    /// Use case: Frontend applications often need to display only active habits
    /// in the user interface, filtering out completed or archived habits.
    /// </summary>
    [HttpGet("user/{userId}/active")]
    public async Task<ActionResult<IEnumerable<HabitDto>>> GetActiveHabitsByUserId(string userId)
    {
        // Extract authenticated user ID from JWT token
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Ensure user can only access their own habits
        if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
        {
            return Unauthorized(new { message = "You can only access your own habits." });
        }

        var habits = await _habitService.GetActiveHabitsByUserIdAsync(userId);
        return Ok(habits);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Habit by ID
    /// 
    /// Retrieves a specific habit by its ID. The service layer should verify
    /// that the habit belongs to the authenticated user before returning it.
    /// 
    /// Security considerations:
    /// - Habit ownership verification should be implemented in the service layer
    /// - Users should only be able to access habits they own
    /// - Consider adding user ID validation in the service method
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabitById(Guid id)
    {
        var habit = await _habitService.GetHabitByIdAsync(id);
        if (habit == null)
        {
            return NotFound();
        }

        // Verify habit ownership
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (habit.UserId != authenticatedUserId)
        {
            return Unauthorized(new { message = "You can only access your own habits." });
        }

        return Ok(habit);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Get Habit with Entries
    /// 
    /// Retrieves a specific habit along with all its associated entries.
    /// This endpoint provides a complete view of a habit's progress.
    /// 
    /// Security considerations:
    /// - Habit ownership verification is required
    /// - Entries are automatically filtered by habit ownership
    /// - Large datasets should be paginated for performance
    /// </summary>
    [HttpGet("{id}/with-entries")]
    public async Task<ActionResult<HabitDto>> GetHabitWithEntries(Guid id)
    {
        var habit = await _habitService.GetHabitWithEntriesAsync(id);
        if (habit == null)
        {
            return NotFound();
        }

        // Verify habit ownership
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (habit.UserId != authenticatedUserId)
        {
            return Unauthorized(new { message = "You can only access your own habits." });
        }

        return Ok(habit);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Create New Habit
    /// 
    /// Creates a new habit for the authenticated user. The user ID is automatically
    /// set from the JWT token claims, ensuring users can only create habits for themselves.
    /// 
    /// Security features:
    /// - User ID is extracted from JWT token and set in the DTO
    /// - Input validation prevents malicious data
    /// - Habit ownership is automatically established
    /// 
    /// The CreateHabitDto should be modified to not require a UserId field,
    /// as it will be automatically set from the authenticated user's claims.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit([FromBody] CreateHabitDto createHabitDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract authenticated user ID and set it in the DTO
        var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authenticatedUserId))
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        // Set the user ID from the JWT token (override any provided value for security)
        createHabitDto.UserId = authenticatedUserId;

        var habit = await _habitService.CreateHabitAsync(createHabitDto);
        return CreatedAtAction(nameof(GetHabitById), new { id = habit.Id }, habit);
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Update Habit
    /// 
    /// Updates an existing habit. The service layer should verify habit ownership
    /// before allowing any modifications.
    /// 
    /// Security considerations:
    /// - Habit ownership verification in service layer
    /// - Users can only update their own habits
    /// - Input validation prevents malicious updates
    /// - Audit trail should be maintained for habit changes
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<HabitDto>> UpdateHabit(Guid id, [FromBody] CreateHabitDto updateHabitDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Extract authenticated user ID and set it in the DTO
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            // Set the user ID from the JWT token for security
            updateHabitDto.UserId = authenticatedUserId;

            var habit = await _habitService.UpdateHabitAsync(id, updateHabitDto);
            return Ok(habit);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// PROTECTED ENDPOINT - Delete Habit
    /// 
    /// Deletes a habit and all its associated entries. The service layer should
    /// verify habit ownership before allowing deletion.
    /// 
    /// Security considerations:
    /// - Habit ownership verification in service layer
    /// - Users can only delete their own habits
    /// - Consider soft delete pattern to preserve data integrity
    /// - Cascade deletion of related entries should be handled properly
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(Guid id)
    {
        try
        {
            // The service layer should verify habit ownership
            // before allowing deletion
            await _habitService.DeleteHabitAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
} 