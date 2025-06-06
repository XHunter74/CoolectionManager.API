using Moq;
using Microsoft.Extensions.Logging;
using xhunter74.CollectionManager.Shared.Services;

namespace CollectionManager.Shared.Test;

public class LocalStorageServiceTests : IDisposable
{
    private readonly string _testRoot;
    private readonly LocalStorageService _service;
    private readonly Mock<ILogger<LocalStorageService>> _loggerMock = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();
    private readonly byte[] _testData = [1, 2, 3, 4, 5];

    public LocalStorageServiceTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "LocalStorageServiceTests", Guid.NewGuid().ToString());
        _service = new LocalStorageService(_loggerMock.Object, _testRoot);
    }

    [Fact(DisplayName = "UploadFileAsync creates file and can read back the same data")]
    public async Task UploadFileAsync_CreatesFileAndCanReadBack()
    {
        await _service.UploadFileAsync(_userId, _fileId, _testData, CancellationToken.None);
        var result = await _service.GetFileAsync(_userId, _fileId, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(_testData, result);
    }

    [Fact(DisplayName = "GetFileAsync returns null if file does not exist")]
    public async Task GetFileAsync_ReturnsNullIfFileDoesNotExist()
    {
        var result = await _service.GetFileAsync(_userId, _fileId, CancellationToken.None);
        Assert.Null(result);
    }

    [Fact(DisplayName = "DeleteFileAsync deletes file successfully")]
    public async Task DeleteFileAsync_DeletesFile()
    {
        await _service.UploadFileAsync(_userId, _fileId, _testData, CancellationToken.None);
        await _service.DeleteFileAsync(_userId, _fileId, CancellationToken.None);
        var result = await _service.GetFileAsync(_userId, _fileId, CancellationToken.None);
        Assert.Null(result);
    }

    [Fact(DisplayName = "UploadFileAsync throws ArgumentException on null or empty input")]
    public async Task UploadFileAsync_ThrowsOnNullOrEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadFileAsync(_userId, 
            _fileId, null, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadFileAsync(_userId, _fileId, 
            Array.Empty<byte>(), CancellationToken.None));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
            Directory.Delete(_testRoot, true);
    }
}
