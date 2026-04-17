namespace Application.Common.Models;

public sealed record LookupDto
{
    public UserDto? CurrentUser { get; init; }
    public bool IsAuthenticated => CurrentUser is not null;
}
