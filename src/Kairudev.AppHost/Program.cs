var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Kairudev_Api>("api");

builder.AddProject<Projects.Kairudev_Web>("web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
