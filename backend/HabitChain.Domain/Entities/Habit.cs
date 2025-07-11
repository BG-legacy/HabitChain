using HabitChain.Domain.Enums;

namespace HabitChain.Domain.Entities;

// This class represents a habit that I want to track. It inherits from BaseEntity, so I get Id, CreatedAt, and UpdatedAt automatically.
public class Habit : BaseEntity
{
    // This is the name I give to my habit.
    public string Name { get; set; } = string.Empty;
    // I can optionally describe my habit here.
    public string? Description { get; set; }
    // This tells me how often I want to do this habit (daily, weekly, etc.).
    public HabitFrequency Frequency { get; set; }
    // This links the habit to me as a user.
    public string UserId { get; set; } = string.Empty;
    // I use this to mark if my habit is currently active.
    public bool IsActive { get; set; } = true;
    // I can pick a color for my habit in the UI.
    public string? Color { get; set; } // For UI customization
    // I can pick an icon for my habit in the UI.
    public string? IconName { get; set; } // For UI customization
    // This tracks my current streak for this habit.
    public int CurrentStreak { get; set; } = 0;
    // This keeps track of my longest streak ever for this habit.
    public int LongestStreak { get; set; } = 0;
    // This tells me the last time I completed this habit.
    public DateTime? LastCompletedAt { get; set; }
    // This is the target number of days for this habit.
    public int TargetDays { get; set; } = 1;
    
    // Navigation properties help me link this habit to other things in the system.
    // This is the user who owns the habit (me).
    public User User { get; set; } = null!;
    // These are all the entries (completions) for this habit.
    public ICollection<HabitEntry> Entries { get; set; } = new List<HabitEntry>();
    // These are all the check-ins for this habit.
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    // These are all the badges I've earned for this habit.
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    // These are all the encouragements related to this habit.
    public ICollection<Encouragement> Encouragements { get; set; } = new List<Encouragement>();
} 