using HabitChain.Domain.Enums;

namespace HabitChain.Application.DTOs;

public class EncouragementDto
{
    public Guid Id { get; set; }
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public EncouragementType Type { get; set; }
    public Guid? HabitId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public UserDto FromUser { get; set; } = null!;
    public UserDto ToUser { get; set; } = null!;
    public HabitDto? Habit { get; set; }
} 