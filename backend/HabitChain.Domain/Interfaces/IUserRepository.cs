using HabitChain.Domain.Entities;
using System.Linq.Expressions;

namespace HabitChain.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<User?> GetUserWithHabitsAsync(string userId);
    Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate);
    Task<User> AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
} 