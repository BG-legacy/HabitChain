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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 