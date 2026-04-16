var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ECommerce_Server>("ecommerce-server");

builder.Build().Run();
