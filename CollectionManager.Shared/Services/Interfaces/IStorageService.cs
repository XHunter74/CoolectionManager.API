namespace xhunter74.CollectionManager.Shared.Services.Interfaces;

public interface IStorageService
{
    Task UploadFileAsync(Guid userId, Guid fileId, byte[] sources);
    Task DeleteFileAsync(Guid userId, Guid fileId);
    Task<byte[]?> GetFileAsync(Guid userId, Guid fileId);
}
