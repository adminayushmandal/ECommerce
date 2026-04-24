var builder = DistributedApplication.CreateBuilder(args);

// Postgres container with pgAdmin, and a persistent data volume. The database will be created on startup if it doesn't exist.
var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector", "pg17-trixie")
    .WithHostPort(54332)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("postgres_volume_data")
    .WithPgAdmin();

string databaseName = "EcommerceDb";
var database = postgres.AddDatabase(databaseName);

var ollama = builder.AddOllamaLocal("ollama");

var gemma4Chat = ollama.AddModel("gemma4:e2b");
var embeddingGemma = ollama.AddModel("embeddinggemma:300m");

var api = builder.AddProject<Projects.ECommerce_Server>("api")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("http", opt =>
    {
        opt.DisplayText = "Scalar API reference";
        opt.Url = "/scalar";
    })
    .WithEnvironment("Enma:ChatModel", "gemma4:e2b")
    .WithEnvironment("Enma:EmbeddingModel", "embeddinggemma:300m")
    .WithEnvironment("Enma:OllamaEndpoint", ollama.GetEndpoint("http"))
    .WithReference(database)
    .WithReference(gemma4Chat)
    .WithReference(embeddingGemma)
    .WaitFor(gemma4Chat)
    .WaitFor(embeddingGemma)
    .WaitFor(database);

builder.AddJavaScriptApp("ClientApp", "../ClientApp", runScriptName: "start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
