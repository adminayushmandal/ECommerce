using Application.Common.Models;

namespace Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<UserDto?> GetUserAsync(string userId, CancellationToken ct);
        Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct);
    }
}
