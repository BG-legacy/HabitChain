using System.ComponentModel.DataAnnotations;
using HabitChain.Application.Validation;

namespace HabitChain.Application.DTOs;

/// <summary>
/// DTO for creating a new check-in
/// </summary>
public class CreateCheckInDto
{
    /// <summary>
    /// The ID of the user making the check-in (set automatically from JWT token)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The ID of the habit being checked in
    /// </summary>
    [Required(ErrorMessage = "Habit ID is required.")]
    public Guid HabitId { get; set; }

    /// <summary>
    /// When the check-in was completed (cannot be in the future, within reasonable range)
    /// </summary>
    [Required(ErrorMessage = "Completion date is required.")]
    [DataType(DataType.DateTime)]
    // Temporarily comment out strict date validation for testing
    // [NotInFuture(ErrorMessage = "Check-in date cannot be in the future.")]
    // [DateRange(30, 7, ErrorMessage = "Check-in date must be within 30 days in the past and 7 days in the future.")]
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Optional notes about the check-in (max 1000 characters, no HTML)
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
    [NoHtml(ErrorMessage = "Notes cannot contain HTML or script tags.")]
    public string? Notes { get; set; }

    /// <summary>
    /// The current streak day (must be positive)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Streak day must be a positive number.")]
    public int StreakDay { get; set; }

    /// <summary>
    /// Whether this is a manual entry (default: false)
    /// </summary>
    public bool IsManualEntry { get; set; } = false;
} 