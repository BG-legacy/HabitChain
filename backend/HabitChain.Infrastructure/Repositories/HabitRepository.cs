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
        var habits = await _dbSet
            .Where(h => h.UserId == userId)
            .Include(h => h.CheckIns)
            .OrderBy(h => h.Name)
            .ToListAsync();
            
        // Debug logging
        foreach (var habit in habits)
        {
            Console.WriteLine($"Repository - Habit: {habit.Name}, CheckIns Count: {habit.CheckIns?.Count ?? 0}");
        }
        
        return habits;
    }

    public async Task<IEnumerable<Habit>> GetActiveHabitsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(h => h.UserId == userId && h.IsActive)
            .Include(h => h.CheckIns)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Habit?> GetHabitWithEntriesAsync(Guid habitId)
    {
        return await _dbSet
            .Include(h => h.Entries)
            .Include(h => h.CheckIns)
            .FirstOrDefaultAsync(h => h.Id == habitId);
    }
} 