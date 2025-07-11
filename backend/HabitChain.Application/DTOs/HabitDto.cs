using HabitChain.Domain.Enums;

namespace HabitChain.Application.DTOs;

public class HabitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public HabitFrequency Frequency { get; set; }
    public bool IsActive { get; set; }
    public string? Color { get; set; }
    public string? IconName { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public int TotalCheckIns { get; set; }
    public double CompletionRate { get; set; } // Overall completion rate as percentage
    public double WeeklyCompletionRate { get; set; } // Last 7 days completion rate
    public double MonthlyCompletionRate { get; set; } // Last 30 days completion rate
    public int TotalPossibleCompletions { get; set; } // Total possible completions based on frequency
    public int TotalActualCompletions { get; set; } // Total actual completions
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 