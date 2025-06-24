using HabitChain.Domain.Entities;
using HabitChain.Domain.Enums;

namespace HabitChain.Domain.Interfaces;

public interface IEncouragementRepository : IRepository<Encouragement>
{
    Task<IEnumerable<Encouragement>> GetReceivedEncouragements(string userId);
    Task<IEnumerable<Encouragement>> GetSentEncouragements(string userId);
    Task<IEnumerable<Encouragement>> GetUnreadEncouragements(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(Guid encouragementId);
} 