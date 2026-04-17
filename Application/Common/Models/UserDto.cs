namespace Application.Common.Models;

public record UserDto
{
    public string Id { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public bool IsActive { get; init; }
    public string[] Roles { get; init; } = [];
}
