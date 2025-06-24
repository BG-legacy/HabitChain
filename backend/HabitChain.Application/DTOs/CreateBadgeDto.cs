using HabitChain.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using HabitChain.Application.Validation;

namespace HabitChain.Application.DTOs;

/// <summary>
/// DTO for creating a new badge
/// </summary>
public class CreateBadgeDto
{
    /// <summary>
    /// The name of the badge (required, 3-100 characters)
    /// </summary>
    [Required(ErrorMessage = "Badge name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Badge name must be between 3 and 100 characters.")]
    [SafeString(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Badge name can only contain letters, numbers, spaces, hyphens, and underscores.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the badge (required, 10-500 characters, no HTML)
    /// </summary>
    [Required(ErrorMessage = "Badge description is required.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters.")]
    [NoHtml(ErrorMessage = "Description cannot contain HTML or script tags.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The type of badge
    /// </summary>
    [Required(ErrorMessage = "Badge type is required.")]
    [EnumDataType(typeof(BadgeType), ErrorMessage = "Invalid badge type.")]
    public BadgeType Type { get; set; }

    /// <summary>
    /// Icon name for the badge (optional, 1-50 characters)
    /// </summary>
    [StringLength(50, ErrorMessage = "Icon name cannot exceed 50 characters.")]
    [SafeString(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "Icon name can only contain letters, numbers, hyphens, and underscores.")]
    public string? IconName { get; set; }

    /// <summary>
    /// Whether the badge is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
} 