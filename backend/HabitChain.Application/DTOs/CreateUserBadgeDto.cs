using System.ComponentModel.DataAnnotations;
using HabitChain.Application.Validation;

namespace HabitChain.Application.DTOs;

/// <summary>
/// DTO for creating a new user badge assignment
/// </summary>
public class CreateUserBadgeDto
{
    /// <summary>
    /// The ID of the user earning the badge (set automatically from JWT token)
    /// </summary>
    [Required(ErrorMessage = "User ID is required.")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the badge being earned
    /// </summary>
    [Required(ErrorMessage = "Badge ID is required.")]
    public Guid BadgeId { get; set; }

    /// <summary>
    /// When the badge was earned (cannot be in the future, within reasonable range)
    /// </summary>
    [Required(ErrorMessage = "Earned date is required.")]
    [DataType(DataType.DateTime)]
    [NotInFuture(ErrorMessage = "Earned date cannot be in the future.")]
    [DateRange(365, 1, ErrorMessage = "Earned date must be within 365 days in the past and 1 day in the future.")]
    public DateTime EarnedAt { get; set; }
} 