using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ECommerce.Server.Infrastructure
{
    internal sealed class BearerSecuritySchemaOperationTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

            if (authenticationSchemes.Any(x => x.Name == IdentityConstants.BearerScheme))
            {
                var requirement = new Dictionary<string, IOpenApiSecurityScheme>()
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        BearerFormat = "JWT",
                        Description = "Please insert JWT token here.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    }
                };

                document.Components ??= new();
                document.Components.SecuritySchemes = requirement;
            }

            document.Info = new()
            {
                Contact = new()
                {
                    Email = "ayushmandal@mandalarc.net",
                    Name = "Ayush Krishan Mandal",
                },
                Description = "This is an Ecommerce web project.",
                Title = "ECommerce",
                Version = "v1",
            };
        }
    }
}
