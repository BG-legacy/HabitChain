using HabitChain.Domain.Entities;

namespace HabitChain.Domain.Interfaces;

public interface ICheckInRepository : IRepository<CheckIn>
{
    Task<IEnumerable<CheckIn>> GetCheckInsByUserIdAsync(string userId);
    Task<IEnumerable<CheckIn>> GetCheckInsByHabitIdAsync(Guid habitId);
    Task<CheckIn?> GetLatestCheckInForHabitAsync(Guid habitId);
    Task<IEnumerable<CheckIn>> GetCheckInsForDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
    Task<int> GetCurrentStreakAsync(Guid habitId);
} 