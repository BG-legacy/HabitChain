using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Repositories;

public class HabitRepository : Repository<Habit>, IHabitRepository
{
    public HabitRepository(HabitChainDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Habit>> GetHabitsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(h => h.UserId == userId)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Habit>> GetActiveHabitsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(h => h.UserId == userId && h.IsActive)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Habit?> GetHabitWithEntriesAsync(Guid habitId)
    {
        return await _dbSet
            .Include(h => h.Entries)
            .FirstOrDefaultAsync(h => h.Id == habitId);
    }
} 