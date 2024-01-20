using Domain.Aggregates;
using Microsoft.Azure.Cosmos;

namespace Infrastructure.Repository;

public class Repository<T> : IRepository<T> where T : class, IAggregrateRoot
{
    private readonly Container _container;

    public Repository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        DatabaseResponse database = cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
        ContainerResponse containerResponse = database.Database.CreateContainerIfNotExistsAsync(containerName, "/id").GetAwaiter().GetResult();
        _container = containerResponse.Container;
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _container.CreateItemAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        T? aggregateRoot = await GetByIdAsync(entity.Id, cancellationToken);

        if (aggregateRoot is null)
        {
            throw new EntityNotExistsException($"Entity with id {entity.Id} does not exist");
        }

        await _container.UpsertItemAsync(entity, new PartitionKey(entity.Id.ToString()), cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<T>(entity.Id.ToString(), new PartitionKey(entity.Id.ToString()), cancellationToken: cancellationToken);
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _container.ReadItemAsync<T>(id.ToString(), new PartitionKey(id.ToString()), cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
