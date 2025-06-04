namespace xhunter74.CollectionManager.Data.Mongo.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllCollectionItemsAsync(string collectionId, CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(string id, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(string id, T entity, CancellationToken cancellationToken);
}
