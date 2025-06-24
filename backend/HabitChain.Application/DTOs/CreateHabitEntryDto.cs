using System.ComponentModel.DataAnnotations;
using HabitChain.Application.Validation;

namespace HabitChain.Application.DTOs;

/// <summary>
/// DTO for creating a new habit entry
/// </summary>
public class CreateHabitEntryDto
{
    /// <summary>
    /// The ID of the habit this entry belongs to
    /// </summary>
    [Required(ErrorMessage = "Habit ID is required.")]
    public Guid HabitId { get; set; }

    /// <summary>
    /// When the habit was completed (cannot be in the future, within reasonable range)
    /// </summary>
    [Required(ErrorMessage = "Completion date is required.")]
    [DataType(DataType.DateTime)]
    [NotInFuture(ErrorMessage = "Completion date cannot be in the future.")]
    [DateRange(30, 1, ErrorMessage = "Completion date must be within 30 days in the past and 1 day in the future.")]
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Optional notes about the habit completion (max 500 characters, no HTML)
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    [NoHtml(ErrorMessage = "Notes cannot contain HTML or script tags.")]
    public string? Notes { get; set; }
} 