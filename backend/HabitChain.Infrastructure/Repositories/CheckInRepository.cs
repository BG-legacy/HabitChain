using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Repositories;

public class CheckInRepository : Repository<CheckIn>, ICheckInRepository
{
    public CheckInRepository(HabitChainDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CheckIn>> GetCheckInsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.Habit)
            .OrderByDescending(c => c.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CheckIn>> GetCheckInsByHabitIdAsync(Guid habitId)
    {
        return await _dbSet
            .Where(c => c.HabitId == habitId)
            .OrderByDescending(c => c.CompletedAt)
            .ToListAsync();
    }

    public async Task<CheckIn?> GetLatestCheckInForHabitAsync(Guid habitId)
    {
        return await _dbSet
            .Where(c => c.HabitId == habitId)
            .OrderByDescending(c => c.CompletedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CheckIn>> GetCheckInsForDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
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
            .Where(c => c.UserId == userId && 
                       c.CompletedAt >= utcStartDate && 
                       c.CompletedAt <= utcEndDate)
            .Include(c => c.Habit)
            .OrderBy(c => c.CompletedAt)
            .ToListAsync();
    }

    public async Task<int> GetCurrentStreakAsync(Guid habitId)
    {
        var latestCheckIn = await GetLatestCheckInForHabitAsync(habitId);
        return latestCheckIn?.StreakDay ?? 0;
    }
} 