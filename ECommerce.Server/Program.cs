using Application;
using Infrastructure.Data;
using Serilog;
using System.Security.Claims;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ECommerce.Server");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    builder.AddServiceDefaults();

    var connectionString = builder.Configuration.GetConnectionString("EcommerceDb")
        ?? throw new ApplicationException("No connection string found for 'EcommerceDb'.");

    builder.AddApplicationServices();
    builder.AddInfrastructure(connectionString);
    builder.AddWebServices();

    var app = builder.Build();

    app.MapDefaultEndpoints();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(opt =>
        {
            opt.WithTitle("Ecommercerce API Reference");
            opt.WithTheme(ScalarTheme.DeepSpace);
        });
    }

    await app.AddSeedAsync();

    app.UseExceptionHandler();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("EndpointName", httpContext.GetEndpoint()?.DisplayName);

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                diagnosticContext.Set("UserId", userId);
            }
        };
    });

    app.UseHttpsRedirection();

    app.MapEndpoints();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "ECommerce.Server terminated unexpectedly during startup or execution");
}
finally
{
    await Log.CloseAndFlushAsync();
}
