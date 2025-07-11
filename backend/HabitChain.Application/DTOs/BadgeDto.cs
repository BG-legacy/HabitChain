using HabitChain.Domain.Enums;

namespace HabitChain.Application.DTOs;

public class BadgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public BadgeType Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Rarity { get; set; } = "common";
    public int RequiredValue { get; set; }
    public bool IsActive { get; set; }
    public string ColorTheme { get; set; } = "#667eea";
    public bool IsSecret { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Progress tracking properties
    public bool IsEarned { get; set; } = false;
    public int? Progress { get; set; }
    public int? Target { get; set; }
    public DateTime? EarnedAt { get; set; }
    public Guid? HabitId { get; set; }
} 