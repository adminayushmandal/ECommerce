using System.Reflection;

namespace ECommerce.Server.Infrastructure
{
    public static class WebApplicationExtension
    {
        extension(WebApplication app)
        {
            RouteGroupBuilder MapGroup(EndpointGroupBase instance)
            {
                var groupName = instance.GroupName ?? instance.GetType().Name;
                return app.MapGroup($"/api/{groupName}")
                    .WithTags(groupName);
            }

            public WebApplication MapEndpoints()
            {
                var assembly = Assembly.GetExecutingAssembly();

                var endpointGroupType = typeof(EndpointGroupBase);

                var endpointGroupTypes = assembly.GetExportedTypes()
                    .Where(t => t.IsSubclassOf(endpointGroupType));

                foreach (var type in endpointGroupTypes)
                {
                    if (Activator.CreateInstance(type) is not EndpointGroupBase instance) continue;

                    var group = app.MapGroup(instance);
                    instance.Map(group);
                }
                return app;
            }
        }
    }
}
