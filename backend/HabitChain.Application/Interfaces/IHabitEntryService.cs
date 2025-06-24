using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface IHabitEntryService
{
    Task<IEnumerable<HabitEntryDto>> GetEntriesByHabitIdAsync(Guid habitId);
    Task<IEnumerable<HabitEntryDto>> GetEntriesByDateRangeAsync(Guid habitId, DateTime startDate, DateTime endDate);
    Task<HabitEntryDto?> GetEntryByIdAsync(Guid id);
    Task<HabitEntryDto> CreateEntryAsync(CreateHabitEntryDto createEntryDto);
    Task DeleteEntryAsync(Guid id);
    Task<bool> HasEntryForDateAsync(Guid habitId, DateTime date);
} 