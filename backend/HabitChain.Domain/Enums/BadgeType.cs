namespace HabitChain.Domain.Enums;

// This enum tells me what kind of badge I can earn.
public enum BadgeType
{
    Streak = 1,           // I earn this for consecutive days
    Total = 2,            // I earn this for total check-ins
    Consistency = 3,      // I earn this for being consistent (like a percentage)
    Special = 4,          // I earn this for special achievements
    Milestone = 5,        // I earn this for reaching specific milestones
    Social = 6,           // I earn this for social interactions
    Creation = 7,         // I earn this for creating habits
    TimeBased = 8,        // I earn this for time-based achievements
    Challenge = 9,        // I earn this for completing challenges
    Seasonal = 10,        // I earn this for seasonal achievements
    Rarity = 11,          // I earn this for rare achievements
    Chain = 12            // I earn this for building habit chains
} 