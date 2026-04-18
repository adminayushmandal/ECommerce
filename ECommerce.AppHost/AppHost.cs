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

// =======================================================================

// ==================Ollama container for hosting the Ollama API and models.==================
var ollama = builder.AddOllamaLocal("AgentContainer")
    .AddModel("qwen3.6:35b");

// =======================================================================

var modelId = builder.AddParameter("ModelId");

var api = builder.AddProject<Projects.ECommerce_Server>("api")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint("http", opt =>
    {
        opt.DisplayText = "Scalar API reference";
        opt.Url = "/scalar";
    })
    .WithEnvironment("Enma:ModelId", modelId)
    .WithReference(database)
    .WithReference(ollama)
    .WaitFor(ollama)
    .WaitFor(database);

builder.AddJavaScriptApp("ClientApp", "../ClientApp", runScriptName: "start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
