using HabitChain.Domain.Enums;

namespace HabitChain.Domain.Entities;

// This class represents a badge that I can earn for my achievements. It inherits from BaseEntity, so I get Id, CreatedAt, and UpdatedAt automatically.
public class Badge : BaseEntity
{
    // This is the name of the badge.
    public string Name { get; set; } = string.Empty;
    // This is a description of what the badge means.
    public string Description { get; set; } = string.Empty;
    // This is the URL for the badge's icon.
    public string IconUrl { get; set; } = string.Empty;
    // This tells me what type of badge it is (streak, total, etc.).
    public BadgeType Type { get; set; }
    // This is the value I need to reach to earn this badge (like 7 for a 7-day streak).
    public int RequiredValue { get; set; } // e.g., 7 for "7-day streak"
    // I use this to mark if the badge is currently available.
    public bool IsActive { get; set; } = true;
    
    // Navigation properties help me link this badge to other things in the system.
    // These are all the user badges that have earned this badge.
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
} 