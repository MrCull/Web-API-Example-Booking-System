IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> cache = builder.AddRedis("cache");

IResourceBuilder<AzureCosmosDBResource> cosmos = builder.AddAzureCosmosDB("cosmos")
    .RunAsEmulator();

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>("api")
    .WithReference(cosmos)
    .WithReference(cache);



builder.Build().Run();
