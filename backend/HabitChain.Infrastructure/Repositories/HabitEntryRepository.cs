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
        // Ensure we're working with UTC dates for PostgreSQL compatibility
        var utcStartDate = startDate.Kind switch
        {
            DateTimeKind.Utc => startDate,
            DateTimeKind.Local => startDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(startDate, DateTimeKind.Utc)
        };

        var utcEndDate = endDate.Kind switch
        {
            DateTimeKind.Utc => endDate,
            DateTimeKind.Local => endDate.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(endDate, DateTimeKind.Utc)
        };

        return await _dbSet
            .Where(e => e.HabitId == habitId && 
                       e.CompletedAt >= utcStartDate && 
                       e.CompletedAt <= utcEndDate)
            .OrderByDescending(e => e.CompletedAt)
            .ToListAsync();
    }

    public async Task<bool> HasEntryForDateAsync(Guid habitId, DateTime date)
    {
        // Ensure we're working with UTC dates for PostgreSQL compatibility
        var utcDate = date.Kind switch
        {
            DateTimeKind.Utc => date.Date,
            DateTimeKind.Local => date.ToUniversalTime().Date,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc)
        };

        var startOfDay = utcDate;
        var endOfDay = utcDate.AddDays(1).AddTicks(-1);

        return await _dbSet
            .AnyAsync(e => e.HabitId == habitId && 
                          e.CompletedAt >= startOfDay && 
                          e.CompletedAt <= endOfDay);
    }

    public async Task<IEnumerable<HabitEntry>> GetEntriesByHabitIdsAsync(IEnumerable<Guid> habitIds)
    {
        return await _dbSet
            .Where(e => habitIds.Contains(e.HabitId))
            .OrderByDescending(e => e.CompletedAt)
            .ToListAsync();
    }
} 