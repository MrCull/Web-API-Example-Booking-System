IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> cache = builder.AddRedis("cache");

IResourceBuilder<AzureCosmosDBResource> cosmos = builder.AddAzureCosmosDB("cosmos")
    //.AddDatabase("BookingDb")
    .RunAsEmulator();

//IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice");
IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>("api")
    .WithReference(cosmos)
    .WithReference(cache);



builder.Build().Run();
