using VitaTrack.Api.Users.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetProfileAsync(long userId, CancellationToken cancellationToken = default);
        Task<UserProfileDto?> UpdateProfileAsync(long userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    }
}
