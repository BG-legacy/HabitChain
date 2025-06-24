using HabitChain.Domain.Entities;
using HabitChain.Domain.Interfaces;
using HabitChain.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HabitChain.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HabitChainDbContext _context;
    private readonly DbSet<User> _dbSet;

    public UserRepository(HabitChainDbContext context)
    {
        _context = context;
        _dbSet = context.Set<User>();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }

    public async Task<User?> GetUserWithHabitsAsync(string userId)
    {
        return await _dbSet
            .Include(u => u.Habits)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<User> AddAsync(User entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(User entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }
} 