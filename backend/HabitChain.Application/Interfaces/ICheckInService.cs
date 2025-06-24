using HabitChain.Application.DTOs;

namespace HabitChain.Application.Interfaces;

public interface ICheckInService
{
    Task<IEnumerable<CheckInDto>> GetCheckInsByUserIdAsync(string userId);
    Task<IEnumerable<CheckInDto>> GetCheckInsByHabitIdAsync(Guid habitId);
    Task<IEnumerable<CheckInDto>> GetCheckInsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<CheckInDto?> GetCheckInByIdAsync(Guid id);
    Task<CheckInDto> CreateCheckInAsync(CreateCheckInDto createCheckInDto);
    Task<CheckInDto> UpdateCheckInAsync(Guid id, CreateCheckInDto updateCheckInDto);
    Task DeleteCheckInAsync(Guid id);
    Task<bool> HasCheckInForDateAsync(string userId, Guid habitId, DateTime date);
} 