using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands;

public sealed record RegisterUserCommand(
    string DisplayName,
    string Email,
    string Password,
    string? PhoneNumber) : IRequest<UserDto>;

public sealed class RegisterUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new UserRegistrationException("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new UserRegistrationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UserRegistrationException("Password is required.");
        }

        return await identityService.RegisterUserAsync(
            request.DisplayName.Trim(),
            request.Email.Trim(),
            request.Password,
            string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            cancellationToken);
    }
}
