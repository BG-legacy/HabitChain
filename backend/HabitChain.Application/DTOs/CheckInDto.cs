namespace HabitChain.Application.DTOs;

public class CheckInDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid HabitId { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int StreakDay { get; set; }
    public bool IsManualEntry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public HabitDto Habit { get; set; } = null!;
} 