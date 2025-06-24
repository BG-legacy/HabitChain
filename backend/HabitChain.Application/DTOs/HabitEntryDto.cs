namespace HabitChain.Application.DTOs;

public class HabitEntryDto
{
    public Guid Id { get; set; }
    public Guid HabitId { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 