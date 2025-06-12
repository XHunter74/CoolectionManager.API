namespace xhunter74.CollectionManager.Data.Mongo.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllCollectionItemsAsync(Guid collectionId, IEnumerable<string>? fieldsToInclude, CancellationToken cancellationToken);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Guid id, T entity, CancellationToken cancellationToken);
}
