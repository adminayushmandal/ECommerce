namespace ECommerce.Server.Infrastructure
{
    public abstract class EndpointGroupBase
    {
        public virtual string? GroupName { get; set; }
        public abstract void Map(RouteGroupBuilder groupBuilder);
    }
}
