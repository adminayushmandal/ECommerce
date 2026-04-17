var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(54332)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ECommerce.Postgres.Data")
    .WithPgAdmin();

string databaseName = "EcommerceDb";

var database = postgres.AddDatabase(databaseName);

builder.AddProject<Projects.ECommerce_Server>("api")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("http", opt =>
    {
        opt.DisplayText = "Scalar API reference";
        opt.Url = "/scalar";
    })
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
