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

    public Task DeleteFileAsync(Guid userId, Guid fileId)
    {
        var filePath = Path.Combine(_storageFolder, userId.ToString(), fileId.ToString());
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

    public async Task<byte[]?> GetFileAsync(Guid userId, Guid fileId)
    {
        var filePath = Path.Combine(_storageFolder, userId.ToString(), fileId.ToString());
        if (!File.Exists(filePath))
        {
            Logger.LogWarning("File with ID {FileId} does not exist at path {FilePath}", fileId, filePath);
            return null;
        }

        try
        {
            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read file with ID {FileId} from path {FilePath}", fileId, filePath);
            throw;
        }
    }

    public async Task UploadFileAsync(Guid userId, Guid fileId, byte[] sources)
    {
        if (sources == null || sources.Length == 0)
            throw new ArgumentException("Source data is null or empty.", nameof(sources));

        CreateFolderIfnotExists(userId.ToString());

        var filePath = Path.Combine(_storageFolder, userId.ToString(), fileId.ToString());

        try
        {
            await File.WriteAllBytesAsync(filePath, sources);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to upload file with ID {FileId} to path {FilePath}", fileId, filePath);
            throw;
        }
    }

    private void CreateFolderIfnotExists(string folderName)
    {
        var folderPath = Path.Combine(_storageFolder, folderName);

        if (Directory.Exists(folderPath))
            return;

        try
        {
            Directory.CreateDirectory(folderPath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create folder with path {FolderPath}", folderPath);
            throw;
        }
    }
}
