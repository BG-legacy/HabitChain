namespace HabitChain.Domain.Entities;

// This class represents a single entry (completion) for a habit. It inherits from BaseEntity, so I get Id, CreatedAt, and UpdatedAt automatically.
public class HabitEntry : BaseEntity
{
    // This links the entry to the specific habit I completed.
    public Guid HabitId { get; set; }
    // This is the date and time I completed the habit.
    public DateTime CompletedAt { get; set; }
    // I can add optional notes about this entry.
    public string? Notes { get; set; }
    
    // This is the habit I completed.
    public Habit Habit { get; set; } = null!;
} 