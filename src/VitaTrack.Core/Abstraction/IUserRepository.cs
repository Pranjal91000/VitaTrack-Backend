using VitaTrack.Core.Entities;

namespace VitaTrack.Core.Abstraction
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    }
}
