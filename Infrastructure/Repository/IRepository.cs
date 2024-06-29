using Domain.Aggregates.TheaterChainAggregate;

namespace Infrastructure.Repository;


/// <summary>
/// <para>
/// A <see cref="IRepository{T}" /> can be used to query and save instances of <typeparamref name="T" />.
/// </para>
/// </summary>
/// <typeparam name="T">The type of entity being operated on by this repository.</typeparam>
public interface IRepository
{
    Task AddAsync(TheaterChain entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TheaterChain entity, CancellationToken cancellationToken = default);
    Task<TheaterChain?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(TheaterChain entity, CancellationToken cancellationToken = default);
}
