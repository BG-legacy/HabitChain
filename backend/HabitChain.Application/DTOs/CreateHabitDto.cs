using HabitChain.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using HabitChain.Application.Validation;

namespace HabitChain.Application.DTOs;

/// <summary>
/// DTO for creating a new habit
/// </summary>
public class CreateHabitDto
{
    /// <summary>
    /// The name of the habit (required, 3-100 characters)
    /// </summary>
    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Habit name must be between 3 and 100 characters.")]
    [SafeString(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Habit name can only contain letters, numbers, spaces, hyphens, and underscores.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the habit (max 500 characters, no HTML)
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [NoHtml(ErrorMessage = "Description cannot contain HTML or script tags.")]
    public string? Description { get; set; }

    /// <summary>
    /// The ID of the user who owns this habit (set automatically from JWT token)
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// How often the habit should be performed
    /// </summary>
    [Required(ErrorMessage = "Habit frequency is required.")]
    [EnumDataType(typeof(HabitFrequency), ErrorMessage = "Invalid habit frequency.")]
    public HabitFrequency Frequency { get; set; }

    /// <summary>
    /// Color code for the habit (hex format, optional)
    /// </summary>
    [HexColor(ErrorMessage = "Color must be a valid hex color code (e.g., #FF0000).")]
    public string? Color { get; set; }

    /// <summary>
    /// Icon name for the habit (optional, 1-50 characters)
    /// </summary>
    [StringLength(50, ErrorMessage = "Icon name cannot exceed 50 characters.")]
    [SafeString(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "Icon name can only contain letters, numbers, hyphens, and underscores.")]
    public string? IconName { get; set; }
} 