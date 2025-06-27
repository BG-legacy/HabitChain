using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

namespace HabitChain.Application.Services;

public class AiRecommendationService : IAiRecommendationService
{
    private readonly IHabitRepository _habitRepository;
    private readonly ICheckInRepository _checkInRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClient _openAIClient;

    public AiRecommendationService(
        IHabitRepository habitRepository,
        ICheckInRepository checkInRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IConfiguration configuration)
    {
        _habitRepository = habitRepository;
        _checkInRepository = checkInRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _configuration = configuration;
        
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
                    _configuration["OpenAISettings:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }
        
        _openAIClient = new OpenAIClient(apiKey);
    }

    public async Task<List<HabitRecommendationDto>> GetHabitRecommendationsAsync(string userId)
    {
        var userAnalysis = await GetUserHabitAnalysisAsync(userId);
        
        if (!userAnalysis.CurrentHabits.Any())
        {
            return await GetStarterHabitsAsync();
        }

        var prompt = BuildRecommendationPrompt(userAnalysis);
        var recommendations = await GenerateRecommendationsAsync(prompt);
        
        return recommendations;
    }

    public async Task<UserHabitAnalysisDto> GetUserHabitAnalysisAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        var habits = await _habitRepository.GetHabitsByUserIdAsync(userId);
        var checkIns = await _checkInRepository.GetCheckInsByUserIdAsync(userId);
        
        var recentCheckIns = checkIns
            .Where(c => c.CompletedAt >= DateTime.UtcNow.AddDays(-30))
            .OrderByDescending(c => c.CompletedAt)
            .Take(50)
            .ToList();

        var patterns = AnalyzeHabitPatterns(habits, recentCheckIns);
        
        return new UserHabitAnalysisDto
        {
            UserId = userId,
            UserName = $"{user.FirstName} {user.LastName}",
            CurrentHabits = _mapper.Map<List<HabitSummaryDto>>(habits),
            RecentCheckIns = _mapper.Map<List<CheckInSummaryDto>>(recentCheckIns),
            Patterns = patterns
        };
    }

    public async Task<string> GetPersonalizedMotivationAsync(string userId)
    {
        var userAnalysis = await GetUserHabitAnalysisAsync(userId);
        
        var prompt = $@"
You are a supportive habit coach. Based on this user's habit data, provide a personalized, encouraging message (max 200 words):

User: {userAnalysis.UserName}
Active Habits: {userAnalysis.CurrentHabits.Count}
Total Check-ins (30 days): {userAnalysis.RecentCheckIns.Count}
Average Completion Rate: {userAnalysis.Patterns.AverageCompletionRate:P0}
Strong Categories: {string.Join(", ", userAnalysis.Patterns.StrongCategories)}
Weak Categories: {string.Join(", ", userAnalysis.Patterns.WeakCategories)}

Make it personal, encouraging, and actionable. Focus on their progress and potential for growth.";

        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? 
                   _configuration["OpenAISettings:Model"] ?? "gpt-4o-mini";
        var maxTokens = Environment.GetEnvironmentVariable("OPENAI_MAX_TOKENS") ?? 
                       _configuration["OpenAISettings:MaxTokens"] ?? "500";
        var response = await _openAIClient.GetChatClient(model).CompleteChatAsync(
            new[] { new UserChatMessage(prompt) },
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = int.Parse(maxTokens)
            });

        return response.Value.Content[0].Text ?? "Keep up the great work on your habits!";
    }

    public async Task<List<HabitRecommendationDto>> GetComplementaryHabitsAsync(string userId, Guid habitId)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null)
        {
            throw new ArgumentException("Habit not found");
        }

        var userAnalysis = await GetUserHabitAnalysisAsync(userId);
        
        var prompt = $@"
Based on this user's current habit and their overall pattern, suggest 3-5 complementary habits that would work well together:

Current Habit: {habit.Name} ({habit.Description})
Frequency: {habit.Frequency}
User's Strong Categories: {string.Join(", ", userAnalysis.Patterns.StrongCategories)}
User's Preferred Time: {userAnalysis.Patterns.PreferredTime}

Suggest habits that:
1. Complement the current habit
2. Build on their strengths
3. Fit their schedule and preferences
4. Are realistic and achievable

Return as JSON array with properties: name, description, reasoning, frequency, targetDays, category, confidence, suggestedTime, difficulty";

        var recommendations = await GenerateRecommendationsAsync(prompt);
        return recommendations;
    }

    private async Task<List<HabitRecommendationDto>> GetStarterHabitsAsync()
    {
        var prompt = @"
Suggest 5 beginner-friendly habits for someone just starting their habit journey. 
Focus on simple, achievable habits that build momentum.

Return as JSON array with properties: name, description, reasoning, frequency, targetDays, category, confidence, suggestedTime, difficulty";

        return await GenerateRecommendationsAsync(prompt);
    }

    private string BuildRecommendationPrompt(UserHabitAnalysisDto analysis)
    {
        var currentHabits = string.Join("\n", analysis.CurrentHabits.Select(h => 
            $"- {h.Name}: {h.Frequency}, {h.CurrentStreak} day streak, {h.TotalCheckIns} total check-ins"));

        var recentActivity = string.Join("\n", analysis.RecentCheckIns.Take(10).Select(c => 
            $"- {c.HabitName}: {c.CompletedAt:MM/dd} (Day {c.StreakDay})"));

        return $@"
Based on this user's habit data, suggest 3-5 personalized habit recommendations:

User: {analysis.UserName}
Current Habits ({analysis.CurrentHabits.Count}):
{currentHabits}

Recent Activity (last 30 days):
{recentActivity}

Patterns:
- Strong Categories: {string.Join(", ", analysis.Patterns.StrongCategories)}
- Weak Categories: {string.Join(", ", analysis.Patterns.WeakCategories)}
- Preferred Time: {analysis.Patterns.PreferredTime}
- Average Completion Rate: {analysis.Patterns.AverageCompletionRate:P0}

Suggest habits that:
1. Build on their existing strengths
2. Address areas for improvement
3. Fit their schedule and preferences
4. Are realistic and achievable
5. Complement their current routine

Return as JSON array with properties: name, description, reasoning, frequency, targetDays, category, confidence, suggestedTime, difficulty";
    }

    private async Task<List<HabitRecommendationDto>> GenerateRecommendationsAsync(string prompt)
    {
        try
        {
            var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? 
                       _configuration["OpenAISettings:Model"] ?? "gpt-4o-mini";
            var maxTokens = Environment.GetEnvironmentVariable("OPENAI_MAX_TOKENS") ?? 
                           _configuration["OpenAISettings:MaxTokens"] ?? "1000";
            var response = await _openAIClient.GetChatClient(model).CompleteChatAsync(
                new[] { new UserChatMessage(prompt) },
                new ChatCompletionOptions
                {
                    MaxOutputTokenCount = int.Parse(maxTokens)
                });

            var content = response.Value.Content[0].Text ?? string.Empty;
            if (string.IsNullOrEmpty(content))
            {
                return GetFallbackRecommendations();
            }

            // Try to parse JSON response
            try
            {
                var recommendations = JsonSerializer.Deserialize<List<HabitRecommendationDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return recommendations ?? GetFallbackRecommendations();
            }
            catch (JsonException)
            {
                // If JSON parsing fails, return fallback recommendations
                return GetFallbackRecommendations();
            }
        }
        catch (Exception ex)
        {
            // Log the error (in a real application, use proper logging)
            Console.WriteLine($"Error generating AI recommendations: {ex.Message}");
            return GetFallbackRecommendations();
        }
    }

    private HabitPatternsDto AnalyzeHabitPatterns(IEnumerable<Habit> habits, IEnumerable<CheckIn> checkIns)
    {
        var patterns = new HabitPatternsDto();
        
        if (!habits.Any())
        {
            return patterns;
        }

        // Analyze categories (simple keyword-based for now)
        var categories = new Dictionary<string, int>();
        foreach (var habit in habits)
        {
            var category = DetermineCategory(habit.Name, habit.Description);
            categories[category] = categories.GetValueOrDefault(category, 0) + 1;
        }

        var sortedCategories = categories.OrderByDescending(x => x.Value).ToList();
        patterns.StrongCategories = sortedCategories.Take(3).Select(x => x.Key).ToList();
        patterns.WeakCategories = sortedCategories.Skip(Math.Max(0, sortedCategories.Count - 3)).Select(x => x.Key).ToList();

        // Analyze preferred time
        var checkInHours = checkIns.Select(c => c.CompletedAt.Hour).ToList();
        if (checkInHours.Any())
        {
            var averageHour = (int)checkInHours.Average();
            patterns.PreferredTime = averageHour < 12 ? "Morning" : averageHour < 17 ? "Afternoon" : "Evening";
        }

        // Analyze frequency preferences
        var frequencyCounts = habits.GroupBy(h => h.Frequency).OrderByDescending(g => g.Count());
        patterns.PreferredFrequency = frequencyCounts.FirstOrDefault()?.Key.ToString() ?? "Daily";

        // Calculate completion rate
        var totalPossible = habits.Sum(h => h.Frequency == Domain.Enums.HabitFrequency.Daily ? 30 : 
                                          h.Frequency == Domain.Enums.HabitFrequency.Weekly ? 4 : 1);
        var totalCompleted = checkIns.Count();
        patterns.AverageCompletionRate = totalPossible > 0 ? (double)totalCompleted / totalPossible : 0;

        patterns.TotalActiveHabits = habits.Count(h => h.IsActive);
        patterns.TotalCompletedHabits = habits.Count(h => h.CheckIns.Any());

        return patterns;
    }

    private string DetermineCategory(string name, string? description)
    {
        var text = $"{name} {description}".ToLower();
        
        if (text.Contains("exercise") || text.Contains("workout") || text.Contains("gym") || text.Contains("run"))
            return "Fitness";
        if (text.Contains("read") || text.Contains("book") || text.Contains("study"))
            return "Learning";
        if (text.Contains("meditation") || text.Contains("mindfulness") || text.Contains("yoga"))
            return "Wellness";
        if (text.Contains("water") || text.Contains("drink") || text.Contains("hydration"))
            return "Health";
        if (text.Contains("sleep") || text.Contains("bed") || text.Contains("rest"))
            return "Sleep";
        if (text.Contains("journal") || text.Contains("write") || text.Contains("reflect"))
            return "Reflection";
        if (text.Contains("clean") || text.Contains("organize") || text.Contains("tidy"))
            return "Organization";
        if (text.Contains("code") || text.Contains("program") || text.Contains("develop"))
            return "Productivity";
        
        return "General";
    }

    private List<HabitRecommendationDto> GetFallbackRecommendations()
    {
        return new List<HabitRecommendationDto>
        {
            new HabitRecommendationDto
            {
                Name = "Morning Hydration",
                Description = "Drink a glass of water first thing in the morning",
                Reasoning = "A simple habit that improves health and creates a positive start to your day",
                Frequency = "Daily",
                TargetDays = 1,
                Category = "Health",
                Confidence = 0.9,
                SuggestedTime = "Morning",
                Difficulty = "Easy"
            },
            new HabitRecommendationDto
            {
                Name = "Evening Reflection",
                Description = "Spend 5 minutes reflecting on your day and planning tomorrow",
                Reasoning = "Helps build self-awareness and sets you up for success",
                Frequency = "Daily",
                TargetDays = 1,
                Category = "Reflection",
                Confidence = 0.8,
                SuggestedTime = "Evening",
                Difficulty = "Easy"
            },
            new HabitRecommendationDto
            {
                Name = "Daily Reading",
                Description = "Read for at least 15 minutes each day",
                Reasoning = "Builds knowledge and creates a consistent learning habit",
                Frequency = "Daily",
                TargetDays = 1,
                Category = "Learning",
                Confidence = 0.7,
                SuggestedTime = "Flexible",
                Difficulty = "Medium"
            }
        };
    }
} 