using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IHostApplicationBuilder builder, string connectionString)
        {
            builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();
            builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            builder.Services.AddOptions<PayPalSdkOptions>()
                .Bind(builder.Configuration.GetSection(PayPalSdkOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "PayPal client id is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "PayPal client secret is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.CurrencyCode) && options.CurrencyCode.Length == 3, "PayPal currency code must be a three-letter ISO code.")
                .ValidateOnStart();

            builder.Services.AddSingleton(provider =>
                PayPalCheckoutGateway.CreateClient(provider.GetRequiredService<IOptions<PayPalSdkOptions>>().Value));
            builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString, o => o.UseVector());
                options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            builder.Services.AddScoped<IStoreRepository, StoreRepository>();
            builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPayPalCheckoutGateway, PayPalCheckoutGateway>();
            builder.Services.AddScoped<IProductVectorIndexingService, ProductVectorIndexingService>();
            builder.Services.AddHostedService<ProductVectorSyncHostedService>();

            builder.Services.AddIdentityApiEndpoints<User>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthorizationBuilder();

            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddScoped<DatabaseSeedingService>();
        }
    }
}
