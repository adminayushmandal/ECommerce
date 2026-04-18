using Application.Common.Interfaces;
using ECommerce.Server.Services;

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

            builder.Services.AddScoped<IUser, CurrentUser>();

        }
    }
}
