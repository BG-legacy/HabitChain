using HabitChain.Domain.Enums;

namespace HabitChain.Application.DTOs;

public class BadgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public BadgeType Type { get; set; }
    public int RequiredValue { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 