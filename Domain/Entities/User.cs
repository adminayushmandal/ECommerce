using Shared.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser, IDatetimeAudit
{
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; private set; } = [];
    public virtual ICollection<Order> Orders { get; private set; } = [];

    public User()
    {
    }

    public User(string displayName, string userName)
    {
        DisplayName = displayName;
        UserName = userName;
        Email = userName;
    }
}
