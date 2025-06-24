using HabitChain.Domain.Entities;

namespace HabitChain.Domain.Interfaces;

public interface IHabitRepository : IRepository<Habit>
{
    Task<IEnumerable<Habit>> GetHabitsByUserIdAsync(string userId);
    Task<IEnumerable<Habit>> GetActiveHabitsByUserIdAsync(string userId);
    Task<Habit?> GetHabitWithEntriesAsync(Guid habitId);
} 