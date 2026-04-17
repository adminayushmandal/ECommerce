# ECommerce
In this branch I've added the package for the Postgres, where it can download the image of postgres from the docker.

### Package command
- By cli - `dotnet add package Aspire.Hosting.PostgreSQL --version 13.1.0`
- By Package Manager Console - `NuGet\Install-Package Aspire.Hosting.PostgreSQL -Version 13.1.0`

### Update the previous template:
**before**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ECommerce_Server>("ecommerce-api");

builder.Build().Run();
```
**after**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(54332)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ECommerce.Postgres.Data");

string databaseName = "EcommerceDb";

var database = postgres.AddDatabase(databaseName);

builder.AddProject<Projects.ECommerce_Server>("api")
    .WithHttpsEndpoint(5001, name: "api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
```
While debugging it starts the AspireNet dashboard and you can see the Postgres container and the Database with `EcommerceDb` name under that container.
