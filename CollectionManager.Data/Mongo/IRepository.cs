namespace xhunter74.CollectionManager.Data.Mongo;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllCollectionItemsAsync(string collectionId);
    Task<T> GetByIdAsync(string id);
    Task<T> AddAsync(T entity);
    Task<bool> RemoveAsync(string id);
    Task<bool> UpdateAsync(string id, T entity);
}
