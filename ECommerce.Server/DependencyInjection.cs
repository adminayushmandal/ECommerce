using Application.Common.Interfaces;
using Domain.Common.Interfaces;
using ECommerce.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Shared.Constants;

namespace ECommerce.Server
{
    public static class DependencyInjection
    {
        public static void AddWebServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddExceptionHandler<BusinessExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddOpenApi(opt =>
            {
                opt.AddOperationTransformer<IdentityApiOperationTransformer>();
                opt.AddDocumentTransformer<BearerSecuritySchemaOperationTransformer>();
            });
            builder.Services.AddSignalR();

            builder.Services.AddScoped<IUser, CurrentUser>();

            builder.Services.AddSingleton<IKernelAgentServiceProvider, KernelAgentServiceProvider>();
            AddPermissionPolicies(builder.Services.AddAuthorizationBuilder());

            if (!string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("cache")))
            {
                builder.AddRedisClient("cache");
                builder.Services.AddSingleton<IApplicationCache, RedisApplicationCache>();
            }
            else
            {
                builder.Services.AddSingleton<IApplicationCache, NullApplicationCache>();
            }
        }

        private static void AddPermissionPolicies(AuthorizationBuilder authorizationBuilder)
        {
            foreach (var permission in Contracts.All)
            {
                authorizationBuilder.AddPolicy(permission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(Contracts.ClaimType, permission);
                });
            }
        }
    }
}
