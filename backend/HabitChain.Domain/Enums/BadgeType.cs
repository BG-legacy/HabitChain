namespace HabitChain.Domain.Enums;

// This enum tells me what kind of badge I can earn.
public enum BadgeType
{
    Streak = 1,       // I earn this for consecutive days
    Total = 2,        // I earn this for total check-ins
    Consistency = 3,  // I earn this for being consistent (like a percentage)
    Special = 4       // I earn this for special achievements
} 