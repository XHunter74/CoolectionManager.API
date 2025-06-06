namespace xhunter74.CollectionManager.Shared.Services.Interfaces;

public interface IStorageService
{
    Task UploadFileAsync(Guid userId, Guid fileId, byte[] sources, CancellationToken cancellationToken);
    Task DeleteFileAsync(Guid userId, Guid fileId, CancellationToken cancellationToken);
    Task<byte[]?> GetFileAsync(Guid userId, Guid fileId, CancellationToken cancellationToken);
}
