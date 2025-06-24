namespace HabitChain.Domain.Entities;

// This class represents a check-in for a habit. It inherits from BaseEntity, so I get Id, CreatedAt, and UpdatedAt automatically.
public class CheckIn : BaseEntity
{
    // This links the check-in to me as a user.
    public string UserId { get; set; } = string.Empty;
    // This links the check-in to a specific habit.
    public Guid HabitId { get; set; }
    // This tells me when I completed the habit.
    public DateTime CompletedAt { get; set; }
    // I can optionally add notes about this check-in.
    public string? Notes { get; set; }
    // This tells me what day of my streak this was.
    public int StreakDay { get; set; }
    // This tells me if I manually entered this check-in (vs automatic).
    public bool IsManualEntry { get; set; } = false;
    
    // Navigation properties help me link this check-in to other things in the system.
    // This is the user who made the check-in (me).
    public User User { get; set; } = null!;
    // This is the habit that was checked in.
    public Habit Habit { get; set; } = null!;
} 