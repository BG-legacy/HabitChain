namespace HabitChain.Application.DTOs;

public class UserBadgeDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid BadgeId { get; set; }
    public DateTime EarnedAt { get; set; }
    public Guid? HabitId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public BadgeDto Badge { get; set; } = null!;
    public HabitDto? Habit { get; set; }
} 