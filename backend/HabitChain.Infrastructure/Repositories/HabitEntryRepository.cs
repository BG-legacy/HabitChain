using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Repositories;

public class HabitEntryRepository : Repository<HabitEntry>, IHabitEntryRepository
{
    public HabitEntryRepository(HabitChainDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HabitEntry>> GetEntriesByHabitIdAsync(Guid habitId)
    {
        return await _dbSet
            .Where(e => e.HabitId == habitId)
            .OrderByDescending(e => e.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<HabitEntry>> GetEntriesByDateRangeAsync(Guid habitId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(e => e.HabitId == habitId && 
                       e.CompletedAt >= startDate && 
                       e.CompletedAt <= endDate)
            .OrderByDescending(e => e.CompletedAt)
            .ToListAsync();
    }

    public async Task<bool> HasEntryForDateAsync(Guid habitId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _dbSet
            .AnyAsync(e => e.HabitId == habitId && 
                          e.CompletedAt >= startOfDay && 
                          e.CompletedAt <= endOfDay);
    }
} 