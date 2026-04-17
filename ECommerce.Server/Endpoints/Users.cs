using Domain.Entities;

namespace ECommerce.Server.Endpoints
{
    public class Users : EndpointGroupBase
    {
        public override void Map(RouteGroupBuilder groupBuilder)
        {
            groupBuilder.MapIdentityApi<User>();
        }
    }
}
