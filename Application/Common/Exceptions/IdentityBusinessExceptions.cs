namespace Application.Common.Exceptions;

public sealed class DuplicateEmailException(string email)
    : BusinessLogicException($"A user with email '{email}' already exists.", 409)
{
    public string Email { get; } = email;
}

public sealed class UserRegistrationException(string message)
    : BusinessLogicException(message, 400);
