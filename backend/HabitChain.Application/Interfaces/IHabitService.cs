using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface IHabitService
{
    Task<IEnumerable<HabitDto>> GetHabitsByUserIdAsync(string userId);
    Task<IEnumerable<HabitDto>> GetActiveHabitsByUserIdAsync(string userId);
    Task<HabitDto?> GetHabitByIdAsync(Guid id);
    Task<HabitDto?> GetHabitByIdLightweightAsync(Guid id);
    Task<HabitDto> CreateHabitAsync(CreateHabitDto createHabitDto);
    Task<HabitDto> UpdateHabitAsync(Guid id, CreateHabitDto updateHabitDto);
    Task DeleteHabitAsync(Guid id);
    Task<HabitDto?> GetHabitWithEntriesAsync(Guid habitId);
    Task<UserCompletionRateDto> GetUserCompletionRatesAsync(string userId);
    Task UpdateHabitCompletionAsync(Guid habitId, DateTime completedAt);
} 