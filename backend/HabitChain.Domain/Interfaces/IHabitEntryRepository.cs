using HabitChain.Domain.Entities;

namespace HabitChain.Domain.Interfaces;

public interface IHabitEntryRepository : IRepository<HabitEntry>
{
    Task<IEnumerable<HabitEntry>> GetEntriesByHabitIdAsync(Guid habitId);
    Task<IEnumerable<HabitEntry>> GetEntriesByDateRangeAsync(Guid habitId, DateTime startDate, DateTime endDate);
    Task<bool> HasEntryForDateAsync(Guid habitId, DateTime date);
    Task<IEnumerable<HabitEntry>> GetEntriesByHabitIdsAsync(IEnumerable<Guid> habitIds);
} 