using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitChain.Infrastructure.Repositories;

public class EncouragementRepository : Repository<Encouragement>, IEncouragementRepository
{
    public EncouragementRepository(HabitChainDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Encouragement>> GetReceivedEncouragements(string userId)
    {
        return await _dbSet
            .Where(e => e.ToUserId == userId)
            .Include(e => e.FromUser)
            .Include(e => e.Habit)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Encouragement>> GetSentEncouragements(string userId)
    {
        return await _dbSet
            .Where(e => e.FromUserId == userId)
            .Include(e => e.ToUser)
            .Include(e => e.Habit)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Encouragement>> GetUnreadEncouragements(string userId)
    {
        return await _dbSet
            .Where(e => e.ToUserId == userId && !e.IsRead)
            .Include(e => e.FromUser)
            .Include(e => e.Habit)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _dbSet
            .CountAsync(e => e.ToUserId == userId && !e.IsRead);
    }

    public async Task MarkAsReadAsync(Guid encouragementId)
    {
        var encouragement = await _dbSet.FindAsync(encouragementId);
        if (encouragement != null)
        {
            encouragement.IsRead = true;
            encouragement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
} 