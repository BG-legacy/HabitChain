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
        return await _dbSet
            .Where(c => c.UserId == userId && 
                       c.CompletedAt >= startDate && 
                       c.CompletedAt <= endDate)
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