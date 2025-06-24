using HabitChain.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using HabitChain.Application.Validation;

namespace HabitChain.Application.DTOs;

/// <summary>
/// DTO for creating a new encouragement
/// </summary>
public class CreateEncouragementDto
{
    /// <summary>
    /// The ID of the user sending the encouragement (set automatically from JWT token)
    /// </summary>
    [Required(ErrorMessage = "Sender user ID is required.")]
    public string FromUserId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user receiving the encouragement
    /// </summary>
    [Required(ErrorMessage = "Recipient user ID is required.")]
    public string ToUserId { get; set; } = string.Empty;

    /// <summary>
    /// The encouragement message (required, 1-500 characters, no HTML)
    /// </summary>
    [Required(ErrorMessage = "Encouragement message is required.")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 500 characters.")]
    [NoHtml(ErrorMessage = "Message cannot contain HTML or script tags.")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The type of encouragement
    /// </summary>
    [Required(ErrorMessage = "Encouragement type is required.")]
    [EnumDataType(typeof(EncouragementType), ErrorMessage = "Invalid encouragement type.")]
    public EncouragementType Type { get; set; }

    /// <summary>
    /// Optional habit ID that the encouragement is related to
    /// </summary>
    public Guid? HabitId { get; set; }
} 