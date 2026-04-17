using Application.Common.Interfaces;
using System.Security.Claims;

namespace ECommerce.Server.Services
{
    public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
    {
        public string? Id => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string[] Roles => httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value).ToArray() ?? [];
    }
}
