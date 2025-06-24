namespace HabitChain.Domain.Entities;

// This abstract class gives me a unique Id and timestamps for when my data was created and last updated.
public abstract class BaseEntity
{
    // This is the unique identifier for my entity.
    public Guid Id { get; set; }
    // This is when my entity was created.
    public DateTime CreatedAt { get; set; }
    // This is when my entity was last updated.
    public DateTime UpdatedAt { get; set; }
} 