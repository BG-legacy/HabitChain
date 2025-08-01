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
            Console.WriteLine("🤖 AI Recommendations endpoint called");
            var userId = GetCurrentUserId();
            Console.WriteLine($"🤖 User ID: {userId}");
            
            // Check if OpenAI key is available
            var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            Console.WriteLine($"🤖 OpenAI Key available: {!string.IsNullOrEmpty(openAiKey)}");
            Console.WriteLine($"🤖 OpenAI Key length: {openAiKey?.Length ?? 0}");
            
            var recommendations = await _aiRecommendationService.GetHabitRecommendationsAsync(userId);
            Console.WriteLine($"🤖 Generated {recommendations.Count} recommendations");
            
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
    /// Create a new habit from an AI recommendation
    /// </summary>
    [HttpPost("create-habit")]
    public async Task<ActionResult<HabitDto>> CreateHabitFromRecommendation([FromBody] HabitRecommendationDto recommendation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var createdHabit = await _aiRecommendationService.CreateHabitFromRecommendationAsync(userId, recommendation);
            
            return Ok(new
            {
                success = true,
                data = createdHabit,
                message = $"Habit '{recommendation.Name}' created successfully from AI recommendation"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error creating habit from recommendation",
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