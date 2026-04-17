using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

internal sealed class IdentityService(
    ApplicationDbContext dbContext,
    UserManager<User> userManager,
    IMapper mapper) : IIdentityService
{
    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken ct)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        return user is null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        ct.ThrowIfCancellationRequested();
        return await userManager.IsInRoleAsync(user, role);
    }
}
