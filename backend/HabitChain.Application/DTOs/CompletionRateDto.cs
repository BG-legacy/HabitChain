namespace HabitChain.Application.DTOs;

public class CompletionRateDto
{
    public Guid HabitId { get; set; }
    public string HabitName { get; set; } = string.Empty;
    public double OverallCompletionRate { get; set; }
    public double WeeklyCompletionRate { get; set; }
    public double MonthlyCompletionRate { get; set; }
    public int TotalPossibleCompletions { get; set; }
    public int TotalActualCompletions { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public bool IsActive { get; set; }
}

public class UserCompletionRateDto
{
    public string UserId { get; set; } = string.Empty;
    public double OverallCompletionRate { get; set; }
    public double WeeklyCompletionRate { get; set; }
    public double MonthlyCompletionRate { get; set; }
    public int TotalHabits { get; set; }
    public int ActiveHabits { get; set; }
    public int CompletedHabitsToday { get; set; }
    public List<CompletionRateDto> HabitCompletionRates { get; set; } = new List<CompletionRateDto>();
} 