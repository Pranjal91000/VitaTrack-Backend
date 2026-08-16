using Microsoft.EntityFrameworkCore;
using VitaTrack.Core.Abstraction;
using VitaTrack.Core.Entities;
using VitaTrack.Infrastructure.Data;

namespace VitaTrack.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext appDbContext) : IUserRepository
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<User?> GetByIdAsync(long userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            _appDbContext.Users.Update(user);
            return await _appDbContext.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
