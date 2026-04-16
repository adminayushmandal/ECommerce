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
