using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;

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

    public async Task<UserDto> RegisterUserAsync(string displayName, string email, string password, string? phoneNumber, CancellationToken ct)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new DuplicateEmailException(email);
        }

        var user = new User(displayName, email)
        {
            PhoneNumber = phoneNumber
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new UserRegistrationException(BuildErrorMessage(createResult));
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, ApplicationRoles.Customer);
        if (!addToRoleResult.Succeeded)
        {
            throw new UserRegistrationException(BuildErrorMessage(addToRoleResult));
        }

        ct.ThrowIfCancellationRequested();

        var createdUser = await dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == user.Id, ct)
            ?? user;

        return mapper.Map<UserDto>(createdUser);
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

    private static string BuildErrorMessage(IdentityResult identityResult)
    {
        var errors = identityResult.Errors
            .Select(error => $"{error.Code}: {error.Description}")
            .ToArray();

        return errors.Length == 0
            ? "The identity operation failed."
            : string.Join("; ", errors);
    }
}
