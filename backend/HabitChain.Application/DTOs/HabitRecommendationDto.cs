namespace HabitChain.Application.DTOs;

public class HabitRecommendationDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int TargetDays { get; set; } = 1;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.0;
    public List<string> RelatedHabits { get; set; } = new List<string>();
    public string SuggestedTime { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
}

public class UserHabitAnalysisDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<HabitSummaryDto> CurrentHabits { get; set; } = new List<HabitSummaryDto>();
    public List<CheckInSummaryDto> RecentCheckIns { get; set; } = new List<CheckInSummaryDto>();
    public HabitPatternsDto Patterns { get; set; } = new HabitPatternsDto();
    public List<HabitRecommendationDto> Recommendations { get; set; } = new List<HabitRecommendationDto>();
}

public class HabitSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalCheckIns { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class CheckInSummaryDto
{
    public Guid HabitId { get; set; }
    public string HabitName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int StreakDay { get; set; }
}

public class HabitPatternsDto
{
    public List<string> StrongCategories { get; set; } = new List<string>();
    public List<string> WeakCategories { get; set; } = new List<string>();
    public string PreferredTime { get; set; } = string.Empty;
    public string PreferredFrequency { get; set; } = string.Empty;
    public double AverageCompletionRate { get; set; }
    public int TotalActiveHabits { get; set; }
    public int TotalCompletedHabits { get; set; }
} 