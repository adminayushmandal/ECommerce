namespace ECommerce.Server
{
    public static class DependencyInjection
    {
        public static void AddWebServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOpenApi();
        }
    }
}
