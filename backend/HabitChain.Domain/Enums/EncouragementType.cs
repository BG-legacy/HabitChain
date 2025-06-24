namespace HabitChain.Domain.Enums;

// This enum tells me what kind of encouragement message I am sending or receiving.
public enum EncouragementType
{
    General = 1,        // I get this for general encouragement
    Milestone = 2,      // I get this for reaching a milestone
    Streak = 3,         // I get this for maintaining a streak
    Comeback = 4,       // I get this for getting back on track
    Achievement = 5     // I get this for earning a badge
}