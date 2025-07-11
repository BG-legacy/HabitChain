using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitChain.WebAPI.Controllers;

[ApiController]
[Route("api/ai-recommendations")]
[Authorize]
public class AiRecommendationsController : ControllerBase
{
    private readonly IAiRecommendationService _aiRecommendationService;

    public AiRecommendationsController(IAiRecommendationService aiRecommendationService)
    {
        _aiRecommendationService = aiRecommendationService;
    }

    /// <summary>
    /// Get AI-generated habit recommendations for the current user
    /// </summary>
    [HttpGet("habits")]
    public async Task<ActionResult<List<HabitRecommendationDto>>> GetHabitRecommendations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var recommendations = await _aiRecommendationService.GetHabitRecommendationsAsync(userId);
            
            return Ok(new
            {
                success = true,
                data = recommendations,
                message = "Habit recommendations generated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating habit recommendations",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get detailed analysis of user's habit patterns and data
    /// </summary>
    [HttpGet("analysis")]
    public async Task<ActionResult<UserHabitAnalysisDto>> GetUserAnalysis()
    {
        try
        {
            var userId = GetCurrentUserId();
            var analysis = await _aiRecommendationService.GetUserHabitAnalysisAsync(userId);
            
            return Ok(new
            {
                success = true,
                data = analysis,
                message = "User analysis retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error retrieving user analysis",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get personalized motivation message based on user's habit data
    /// </summary>
    [HttpGet("motivation")]
    public async Task<ActionResult<string>> GetPersonalizedMotivation()
    {
        try
        {
            var userId = GetCurrentUserId();
            var motivation = await _aiRecommendationService.GetPersonalizedMotivationAsync(userId);
            
            return Ok(new
            {
                success = true,
                data = motivation,
                message = "Personalized motivation generated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating motivation",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get complementary habits that work well with a specific habit
    /// </summary>
    [HttpGet("complementary/{habitId}")]
    public async Task<ActionResult<List<HabitRecommendationDto>>> GetComplementaryHabits(Guid habitId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var recommendations = await _aiRecommendationService.GetComplementaryHabitsAsync(userId, habitId);
            
            return Ok(new
            {
                success = true,
                data = recommendations,
                message = "Complementary habits generated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error generating complementary habits",
                error = ex.Message
            });
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
} 