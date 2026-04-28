using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddMediatR(configuration =>
                configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            builder.Services.AddAutoMapper(opt =>
            {
                opt.AddMaps(Assembly.GetExecutingAssembly());
            });

            builder.Services.AddScoped<OrderCheckoutService>();
            builder.Services.AddHostedService<Common.Mappings.AutoMapperValidationHostedService>();
        }
    }
}
