using AutoMapper;
using HabitChain.Application.DTOs;
using HabitChain.Application.Interfaces;
using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Domain.Enums;
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
    private readonly OpenAIClient? _openAIClient;

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
        
        Console.WriteLine($"🤖 OpenAI API Key found: {!string.IsNullOrEmpty(apiKey)}");
        Console.WriteLine($"🤖 API Key length: {apiKey?.Length ?? 0}");
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            _openAIClient = new OpenAIClient(apiKey);
            Console.WriteLine("✅ OpenAI Client initialized successfully");
        }
        else
        {
            _openAIClient = null;
            Console.WriteLine("❌ OpenAI Client not initialized - no API key found");
        }
    }

    public async Task<List<HabitRecommendationDto>> GetHabitRecommendationsAsync(string userId)
    {
        Console.WriteLine($"🤖 GetHabitRecommendationsAsync called for user: {userId}");
        var userAnalysis = await GetUserHabitAnalysisAsync(userId);
        Console.WriteLine($"🤖 User has {userAnalysis.CurrentHabits.Count} habits");
        
        if (!userAnalysis.CurrentHabits.Any())
        {
            Console.WriteLine("🤖 No habits found, calling GetStarterHabitsAsync");
            var starterHabits = await GetStarterHabitsAsync();
            Console.WriteLine($"🤖 GetStarterHabitsAsync returned {starterHabits.Count} habits");
            return starterHabits;
        }

        Console.WriteLine("🤖 User has habits, generating personalized recommendations");
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
        
        if (_openAIClient == null)
        {
            return "OpenAI is not configured. Please set up your API key to get personalized motivation.";
        }
        
        var prompt = $@"
You are a supportive habit coach. Based on this user's habit data, provide a personalized, encouraging message (max 200 words):

User: {userAnalysis.UserName}
Active Habits: {userAnalysis.CurrentHabits.Count}
Total Check-ins (30 days): {userAnalysis.RecentCheckIns.Count}
Average Completion Rate: {userAnalysis.Patterns.AverageCompletionRate:P0}
Strong Categories: {string.Join(", ", userAnalysis.Patterns.StrongCategories)}
Weak Categories: {string.Join(", ", userAnalysis.Patterns.WeakCategories)}

Make it personal, encouraging, and actionable. Focus on their progress and potential for growth.";

        try
        {
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

            return response.Value.Content[0].Text ?? "Unable to generate motivation at this time.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating AI motivation: {ex.Message}");
            return "Unable to generate motivation at this time. Please try again later.";
        }
    }

    public async Task<List<HabitRecommendationDto>> GetComplementaryHabitsAsync(string userId, Guid habitId)
    {
        var habit = await _habitRepository.GetByIdAsync(habitId);
        if (habit == null)
        {
            throw new ArgumentException("Habit not found");
        }

        var userAnalysis = await GetUserHabitAnalysisAsync(userId);
        var habitCategory = DetermineCategory(habit.Name, habit.Description);
        
        var prompt = $@"
Based on this user's current habit and their overall pattern, suggest 5-7 complementary habits that would work well together. Provide VARIED suggestions that include:

Current Habit: {habit.Name} ({habit.Description})
Category: {habitCategory}
Frequency: {habit.Frequency}
User's Strong Categories: {string.Join(", ", userAnalysis.Patterns.StrongCategories)}
User's Weak Categories: {string.Join(", ", userAnalysis.Patterns.WeakCategories)}
User's Preferred Time: {userAnalysis.Patterns.PreferredTime}
Completion Rate: {userAnalysis.Patterns.AverageCompletionRate:P0}

Suggest a VARIED mix that includes:
1. 1-2 habits that directly complement the current habit (same category or related)
2. 1-2 habits that balance the current habit (different category but synergistic)
3. 1 habit that addresses a weak area for the user
4. 1 habit that builds on their strengths
5. 1 habit that introduces a completely new category they haven't explored

Consider their completion rate - if high, suggest more challenging combinations. If low, suggest easier, confidence-building habits.

Make each suggestion unique and explain how it specifically works with their current habit.

Return as JSON array with properties: name, description, reasoning, frequency, targetDays, category, confidence, suggestedTime, difficulty

IMPORTANT: 
- frequency must be one of: 'Daily', 'Weekly', 'Monthly', 'Custom'
- targetDays must be a NUMBER (integer), not a string:
  * For Daily habits: use 7 (every day of the week)
  * For Weekly habits: use 1 (once per week)  
  * For Monthly habits: use 1 (once per month)
- difficulty must be one of: 'Easy', 'Moderate', 'Hard'
- confidence should be a decimal between 0.0 and 1.0
- suggestedTime must be one of: 'Morning', 'Afternoon', 'Evening'";

        var recommendations = await GenerateRecommendationsAsync(prompt);
        return recommendations;
    }

    public async Task<HabitDto> CreateHabitFromRecommendationAsync(string userId, HabitRecommendationDto recommendation)
    {
        // Convert recommendation to CreateHabitDto
        var createHabitDto = new CreateHabitDto
        {
            Name = recommendation.Name,
            Description = recommendation.Description,
            UserId = userId,
            Frequency = ConvertFrequencyStringToEnum(recommendation.Frequency),
            Color = GetColorForCategory(recommendation.Category),
            IconName = GetIconForCategory(recommendation.Category)
        };

        // Create the habit using the existing habit service
        var habit = _mapper.Map<Habit>(createHabitDto);
        habit.Id = Guid.NewGuid();
        habit.IsActive = true;
        habit.CurrentStreak = 0;
        habit.LongestStreak = 0;
        habit.TargetDays = recommendation.TargetDays;
        
        var createdHabit = await _habitRepository.AddAsync(habit);
        
        return _mapper.Map<HabitDto>(createdHabit);
    }

    private HabitFrequency ConvertFrequencyStringToEnum(string frequency)
    {
        return frequency.ToLower() switch
        {
            "daily" => HabitFrequency.Daily,
            "weekly" => HabitFrequency.Weekly,
            "monthly" => HabitFrequency.Monthly,
            "custom" => HabitFrequency.Custom,
            _ => HabitFrequency.Daily
        };
    }

    private string GetColorForCategory(string category)
    {
        return category.ToLower() switch
        {
            "fitness" => "#FF6B6B",
            "health" => "#4ECDC4",
            "learning" => "#45B7D1",
            "wellness" => "#96CEB4",
            "sleep" => "#FFEAA7",
            "reflection" => "#DDA0DD",
            "organization" => "#98D8C8",
            "productivity" => "#F7DC6F",
            "social" => "#FF9FF3",
            "creativity" => "#54A0FF",
            "finance" => "#5F27CD",
            "nutrition" => "#00D2D3",
            "career" => "#FF9F43",
            "hobbies" => "#10AC84",
            "spirituality" => "#A55EEA",
            "relationships" => "#FD79A8",
            _ => "#667eea"
        };
    }

    private string GetIconForCategory(string category)
    {
        return category.ToLower() switch
        {
            "fitness" => "dumbbell",
            "health" => "heart",
            "learning" => "book",
            "wellness" => "leaf",
            "sleep" => "moon",
            "reflection" => "brain",
            "organization" => "clipboard",
            "productivity" => "zap",
            "social" => "users",
            "creativity" => "palette",
            "finance" => "dollar-sign",
            "nutrition" => "apple",
            "career" => "briefcase",
            "hobbies" => "gamepad",
            "spirituality" => "pray",
            "relationships" => "heart",
            _ => "target"
        };
    }

    private async Task<List<HabitRecommendationDto>> GetStarterHabitsAsync()
    {
        Console.WriteLine($"🤖 GetStarterHabitsAsync: OpenAI client is null: {_openAIClient == null}");
        
        if (_openAIClient == null)
        {
            Console.WriteLine("🤖 OpenAI client is null, returning empty list");
            return new List<HabitRecommendationDto>(); // Return empty list when OpenAI not configured
        }

        Console.WriteLine("🤖 OpenAI client available, generating starter habits");
        var prompt = @"
Suggest 5-7 beginner-friendly habits for someone just starting their habit journey. 
Provide a VARIED mix that covers different life areas:

1. Physical health (exercise, nutrition, sleep)
2. Mental wellness (mindfulness, learning, creativity)
3. Personal development (organization, productivity, reflection)
4. Social/relationships (communication, connection)
5. Lifestyle (hobbies, self-care, spirituality)

Make each suggestion unique and achievable for beginners. Vary the difficulty levels and categories.

Return as JSON array with properties: name, description, reasoning, frequency, targetDays, category, confidence, suggestedTime, difficulty

IMPORTANT: 
- frequency must be one of: 'Daily', 'Weekly', 'Monthly', 'Custom'
- targetDays must be a NUMBER (integer), not a string:
  * For Daily habits: use 7 (every day of the week)
  * For Weekly habits: use 1 (once per week)  
  * For Monthly habits: use 1 (once per month)
- difficulty must be one of: 'Easy', 'Moderate', 'Hard'
- confidence should be a decimal between 0.0 and 1.0";

        return await GenerateRecommendationsAsync(prompt);
    }

    private string BuildRecommendationPrompt(UserHabitAnalysisDto analysis)
    {
        var currentHabits = string.Join("\n", analysis.CurrentHabits.Select(h => 
            $"- {h.Name}: {h.Frequency}, {h.CurrentStreak} day streak, {h.TotalCheckIns} total check-ins"));

        var recentActivity = string.Join("\n", analysis.RecentCheckIns.Take(10).Select(c => 
            $"- {c.HabitName}: {c.CompletedAt:MM/dd} (Day {c.StreakDay})"));

        // Analyze user's current habit categories
        var habitCategories = analysis.CurrentHabits
            .Select(h => DetermineCategory(h.Name, h.Description))
            .GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToList();

        var missingCategories = GetMissingCategories(habitCategories);
        var weakAreas = analysis.Patterns.WeakCategories;
        var strongAreas = analysis.Patterns.StrongCategories;

        return $@"
Based on this user's habit data, suggest 5-7 personalized habit recommendations that are varied and address different aspects of their life:

User: {analysis.UserName}
Current Habits ({analysis.CurrentHabits.Count}):
{currentHabits}

Recent Activity (last 30 days):
{recentActivity}

Patterns:
- Strong Categories: {string.Join(", ", strongAreas)}
- Weak Categories: {string.Join(", ", weakAreas)}
- Missing Categories: {string.Join(", ", missingCategories)}
- Preferred Time: {analysis.Patterns.PreferredTime}
- Average Completion Rate: {analysis.Patterns.AverageCompletionRate:P0}

Provide a VARIED mix of recommendations that include:
1. 1-2 habits that build on their strengths (categories they excel in)
2. 1-2 habits that address their weak areas
3. 1-2 habits from missing categories they haven't explored
4. 1 habit that complements their current routine
5. 1 habit that challenges them slightly beyond their comfort zone

Consider their completion rate ({analysis.Patterns.AverageCompletionRate:P0}) - if it's high, suggest more challenging habits. If it's low, suggest easier, confidence-building habits.

Make each recommendation unique and specific to their situation. Vary the difficulty levels, categories, and reasoning.

Return as JSON array with properties: name, description, reasoning, frequency, targetDays, category, confidence, suggestedTime, difficulty

IMPORTANT: 
- frequency must be one of: 'Daily', 'Weekly', 'Monthly', 'Custom'
- targetDays must be a NUMBER (integer), not a string:
  * For Daily habits: use 7 (every day of the week)
  * For Weekly habits: use 1 (once per week)  
  * For Monthly habits: use 1 (once per month)
- difficulty must be one of: 'Easy', 'Moderate', 'Hard'
- confidence should be a decimal between 0.0 and 1.0
- suggestedTime must be one of: 'Morning', 'Afternoon', 'Evening'";
    }

    private List<string> GetMissingCategories(List<string> currentCategories)
    {
        var allCategories = new List<string> 
        { 
            "Fitness", "Health", "Learning", "Wellness", "Sleep", 
            "Reflection", "Organization", "Productivity", "Social", 
            "Creativity", "Finance", "Mindfulness", "Nutrition", 
            "Career", "Relationships", "Hobbies", "Spirituality" 
        };
        
        return allCategories.Except(currentCategories, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private async Task<List<HabitRecommendationDto>> GenerateRecommendationsAsync(string prompt)
    {
        if (_openAIClient == null)
        {
            return new List<HabitRecommendationDto>(); // Return empty list when OpenAI not configured
        }
        
        try
        {
            var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? 
                       _configuration["OpenAISettings:Model"] ?? "gpt-4o-mini";
            var maxTokens = Environment.GetEnvironmentVariable("OPENAI_MAX_TOKENS") ?? 
                           _configuration["OpenAISettings:MaxTokens"] ?? "1000";
            
            Console.WriteLine($"🤖 Making OpenAI API call with model: {model}, maxTokens: {maxTokens}");
            Console.WriteLine($"🤖 Prompt length: {prompt.Length} characters");
            
            var response = await _openAIClient.GetChatClient(model).CompleteChatAsync(
                new[] { new UserChatMessage(prompt) },
                new ChatCompletionOptions
                {
                    MaxOutputTokenCount = int.Parse(maxTokens)
                });

            Console.WriteLine($"🤖 OpenAI API call completed successfully");
            var content = response.Value.Content[0].Text ?? string.Empty;
            Console.WriteLine($"🤖 OpenAI response length: {content.Length} characters");
            Console.WriteLine($"🤖 OpenAI response preview: {content.Substring(0, Math.Min(200, content.Length))}...");
            
            if (string.IsNullOrEmpty(content))
            {
                Console.WriteLine("🤖 OpenAI returned empty content");
                return new List<HabitRecommendationDto>(); // Return empty list instead of fallbacks
            }

            // Try to parse JSON response
            try
            {
                Console.WriteLine("🤖 Attempting to parse JSON response");
                
                // Remove markdown code block wrapper if present
                var jsonContent = content;
                if (content.StartsWith("```json"))
                {
                    jsonContent = content.Replace("```json", "").Replace("```", "").Trim();
                    Console.WriteLine("🤖 Removed markdown wrapper from response");
                }
                else if (content.StartsWith("```"))
                {
                    jsonContent = content.Replace("```", "").Trim();
                    Console.WriteLine("🤖 Removed markdown wrapper from response");
                }
                
                var recommendations = JsonSerializer.Deserialize<List<HabitRecommendationDto>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Console.WriteLine($"🤖 Successfully parsed {recommendations?.Count ?? 0} recommendations");
                return recommendations ?? new List<HabitRecommendationDto>(); // Return empty list instead of fallbacks
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"🤖 JSON parsing failed: {ex.Message}");
                Console.WriteLine($"🤖 Raw response: {content}");
                return new List<HabitRecommendationDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🤖 Error generating AI recommendations: {ex.Message}");
            Console.WriteLine($"🤖 Exception details: {ex}");
            return new List<HabitRecommendationDto>(); // Return empty list instead of fallbacks
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
        
        // Physical Health & Fitness
        if (text.Contains("exercise") || text.Contains("workout") || text.Contains("gym") || 
            text.Contains("run") || text.Contains("jog") || text.Contains("walk") ||
            text.Contains("cardio") || text.Contains("strength") || text.Contains("yoga") ||
            text.Contains("pilates") || text.Contains("swim") || text.Contains("bike") ||
            text.Contains("dance") || text.Contains("sport"))
            return "Fitness";
            
        // Health & Nutrition
        if (text.Contains("water") || text.Contains("drink") || text.Contains("hydration") ||
            text.Contains("vitamin") || text.Contains("supplement") || text.Contains("medication") ||
            text.Contains("checkup") || text.Contains("doctor") || text.Contains("health"))
            return "Health";
            
        // Learning & Education
        if (text.Contains("read") || text.Contains("book") || text.Contains("study") ||
            text.Contains("learn") || text.Contains("course") || text.Contains("language") ||
            text.Contains("skill") || text.Contains("practice") || text.Contains("research"))
            return "Learning";
            
        // Mental Wellness & Mindfulness
        if (text.Contains("meditation") || text.Contains("mindfulness") || text.Contains("breathing") ||
            text.Contains("gratitude") || text.Contains("positive") || text.Contains("mental") ||
            text.Contains("therapy") || text.Contains("counseling") || text.Contains("self-care"))
            return "Wellness";
            
        // Sleep & Rest
        if (text.Contains("sleep") || text.Contains("bed") || text.Contains("rest") ||
            text.Contains("nap") || text.Contains("wake") || text.Contains("dream"))
            return "Sleep";
            
        // Reflection & Journaling
        if (text.Contains("journal") || text.Contains("write") || text.Contains("reflect") ||
            text.Contains("diary") || text.Contains("log") || text.Contains("note"))
            return "Reflection";
            
        // Organization & Planning
        if (text.Contains("clean") || text.Contains("organize") || text.Contains("tidy") ||
            text.Contains("plan") || text.Contains("schedule") || text.Contains("declutter") ||
            text.Contains("sort") || text.Contains("arrange"))
            return "Organization";
            
        // Productivity & Work
        if (text.Contains("code") || text.Contains("program") || text.Contains("develop") ||
            text.Contains("work") || text.Contains("project") || text.Contains("task") ||
            text.Contains("focus") || text.Contains("pomodoro") || text.Contains("time"))
            return "Productivity";
            
        // Social & Relationships
        if (text.Contains("call") || text.Contains("text") || text.Contains("message") ||
            text.Contains("friend") || text.Contains("family") || text.Contains("social") ||
            text.Contains("meet") || text.Contains("connect") || text.Contains("relationship"))
            return "Social";
            
        // Creativity & Arts
        if (text.Contains("draw") || text.Contains("paint") || text.Contains("create") ||
            text.Contains("art") || text.Contains("music") || text.Contains("craft") ||
            text.Contains("design") || text.Contains("write") || text.Contains("poem"))
            return "Creativity";
            
        // Finance & Money
        if (text.Contains("budget") || text.Contains("save") || text.Contains("money") ||
            text.Contains("finance") || text.Contains("invest") || text.Contains("expense") ||
            text.Contains("track") || text.Contains("spend"))
            return "Finance";
            
        // Nutrition & Diet
        if (text.Contains("eat") || text.Contains("meal") || text.Contains("food") ||
            text.Contains("diet") || text.Contains("nutrition") || text.Contains("cook") ||
            text.Contains("breakfast") || text.Contains("lunch") || text.Contains("dinner"))
            return "Nutrition";
            
        // Career & Professional
        if (text.Contains("career") || text.Contains("job") || text.Contains("work") ||
            text.Contains("professional") || text.Contains("network") || text.Contains("resume") ||
            text.Contains("interview") || text.Contains("promotion"))
            return "Career";
            
        // Hobbies & Interests
        if (text.Contains("hobby") || text.Contains("game") || text.Contains("play") ||
            text.Contains("collect") || text.Contains("garden") || text.Contains("photography") ||
            text.Contains("travel") || text.Contains("explore"))
            return "Hobbies";
            
        // Spirituality & Religion
        if (text.Contains("pray") || text.Contains("worship") || text.Contains("spiritual") ||
            text.Contains("church") || text.Contains("temple") || text.Contains("meditation") ||
            text.Contains("blessing") || text.Contains("gratitude"))
            return "Spirituality";
            
        // Relationships & Family
        if (text.Contains("partner") || text.Contains("spouse") || text.Contains("marriage") ||
            text.Contains("parent") || text.Contains("child") || text.Contains("family") ||
            text.Contains("date") || text.Contains("romance"))
            return "Relationships";
        
        return "General";
    }


} 