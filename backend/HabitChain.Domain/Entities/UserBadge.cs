namespace HabitChain.Domain.Entities;

// This class represents a badge that I've earned. It inherits from BaseEntity, so I get Id, CreatedAt, and UpdatedAt automatically.
public class UserBadge : BaseEntity
{
    // This links the badge to me as a user.
    public string UserId { get; set; } = string.Empty;
    // This links to the specific badge I earned.
    public Guid BadgeId { get; set; }
    // This is the date and time I earned the badge.
    public DateTime EarnedAt { get; set; }
    // This optionally links the badge to a specific habit.
    public Guid? HabitId { get; set; }
    
    // Navigation properties help me link this user badge to other things in the system.
    // This is the user (me) who earned the badge.
    public User User { get; set; } = null!;
    // This is the badge I earned.
    public Badge Badge { get; set; } = null!;
    // This is the habit the badge was earned for (if applicable).
    public Habit? Habit { get; set; }
} 