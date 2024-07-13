using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Infrastructure.Repository;

public class Repository : IRepository
{
    private readonly Container _container;

    public Repository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        DatabaseResponse database = cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
        ContainerResponse containerResponse = database.Database.CreateContainerIfNotExistsAsync(containerName, "/id").GetAwaiter().GetResult();
        _container = containerResponse.Container;

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    public async Task AddAsync(TheaterChain entity, CancellationToken cancellationToken = default)
    {
        await _container.CreateItemAsync(item: entity, partitionKey: new PartitionKey(entity.Id.ToString()), cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(TheaterChain entity, CancellationToken cancellationToken = default)
    {
        TheaterChain? aggregateRoot = await GetByIdAsync(entity.Id, cancellationToken);

        if (aggregateRoot is null)
        {
            throw new EntityNotExistsException($"Entity with id {entity.Id} does not exist");
        }

        await _container.UpsertItemAsync(entity, new PartitionKey(entity.Id.ToString()), cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(TheaterChain entity, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<TheaterChain>(entity.Id.ToString(), new PartitionKey(entity.Id.ToString()), cancellationToken: cancellationToken);
    }

    public async Task<TheaterChain?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        TheaterChain? theaterChain;
        try
        {
            theaterChain = await _container.ReadItemAsync<TheaterChain>(id.ToString(), new PartitionKey(id.ToString()), cancellationToken: cancellationToken);

            // Iterate over each theater and its showtimes to assign movies
            foreach (Theater theater in theaterChain.Theaters)
            {
                theater.Initialize(theaterChain.Movies);
            }

        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return theaterChain;
    }
}
