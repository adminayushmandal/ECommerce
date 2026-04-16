using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class Role : IdentityRole
{
    public string? Description { get; private set; }
    public virtual ICollection<UserRole> UserRoles { get; private set; } = [];

    public Role()
    {
    }

    public Role(string roleName) : base(roleName)
    {
    }

    public Role(string roleName, string description) : base(roleName)
    {
        Description = description;
    }
}
