using Microsoft.Extensions.Logging;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.Shared.Services;

public class LocalStorageService : BaseService, IStorageService
{
    private readonly string _storageFolder;

    public LocalStorageService(ILogger<LocalStorageService> logger, string storageFolder) : base(logger)
    {
        _storageFolder = storageFolder;
        if (!Directory.Exists(_storageFolder))
        {
            Logger.LogInformation("Storage folder {StorageFolder} does not exist. Creating it.", _storageFolder);
            Directory.CreateDirectory(_storageFolder);
        }
    }

    public Task DeleteFileAsync(Guid fileId)
    {
        var filePath = Path.Combine(_storageFolder, fileId.ToString());
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            Logger.LogWarning("File with ID {FileId} does not exist at path {FilePath}", fileId, filePath);
        }
        return Task.CompletedTask;
    }

    public async Task<Guid> UploadFileAsync(byte[] sources)
    {
        if (sources == null || sources.Length == 0)
            throw new ArgumentException("Source data is null or empty.", nameof(sources));

        var fileId = Guid.NewGuid();
        var filePath = Path.Combine(_storageFolder, fileId.ToString());

        try
        {
            await File.WriteAllBytesAsync(filePath, sources);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to upload file with ID {FileId} to path {FilePath}", fileId, filePath);
            throw;
        }

        return fileId;
    }
}
