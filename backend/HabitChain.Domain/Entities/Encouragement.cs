using HabitChain.Domain.Enums;

namespace HabitChain.Domain.Entities;

// This class represents an encouragement message between users. It inherits from BaseEntity, so I get Id, CreatedAt, and UpdatedAt automatically.
public class Encouragement : BaseEntity
{
    // This is the user who sent the encouragement.
    public string FromUserId { get; set; } = string.Empty;
    // This is the user who received the encouragement.
    public string ToUserId { get; set; } = string.Empty;
    // This is the encouragement message.
    public string Message { get; set; } = string.Empty;
    // This tells me what type of encouragement this is.
    public EncouragementType Type { get; set; }
    // This optionally links the encouragement to a specific habit.
    public Guid? HabitId { get; set; }
    // This tells me if the recipient has read the encouragement.
    public bool IsRead { get; set; } = false;
    
    // Navigation properties help me link this encouragement to other things in the system.
    // This is the user who sent the encouragement.
    public User FromUser { get; set; } = null!;
    // This is the user who received the encouragement.
    public User ToUser { get; set; } = null!;
    // This is the habit the encouragement is about (if applicable).
    public Habit? Habit { get; set; }
} 