using Moq;
using Microsoft.Extensions.Logging;
using xhunter74.CollectionManager.Shared.Services;

namespace CollectionManager.Shared.Test;

public class LocalStorageServiceTests : IDisposable
{
    private readonly string _testRoot;
    private readonly LocalStorageService _service;
    private readonly Mock<ILogger<LocalStorageService>> _loggerMock;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _fileId = Guid.NewGuid();
    private readonly byte[] _testData = new byte[] { 1, 2, 3, 4, 5 };

    public LocalStorageServiceTests()
    {
        _loggerMock = new Mock<ILogger<LocalStorageService>>();
        _testRoot = Path.Combine(Path.GetTempPath(), "LocalStorageServiceTests", Guid.NewGuid().ToString());
        _service = new LocalStorageService(_loggerMock.Object, _testRoot);
    }

    [Fact]
    public async Task UploadFileAsync_CreatesFileAndCanReadBack()
    {
        await _service.UploadFileAsync(_userId, _fileId, _testData);
        var result = await _service.GetFileAsync(_userId, _fileId);
        Assert.NotNull(result);
        Assert.Equal(_testData, result);
    }

    [Fact]
    public async Task GetFileAsync_ReturnsNullIfFileDoesNotExist()
    {
        var result = await _service.GetFileAsync(_userId, _fileId);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteFileAsync_DeletesFile()
    {
        await _service.UploadFileAsync(_userId, _fileId, _testData);
        await _service.DeleteFileAsync(_userId, _fileId);
        var result = await _service.GetFileAsync(_userId, _fileId);
        Assert.Null(result);
    }

    [Fact]
    public async Task UploadFileAsync_ThrowsOnNullOrEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadFileAsync(_userId, _fileId, null));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadFileAsync(_userId, _fileId, Array.Empty<byte>()));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testRoot))
            Directory.Delete(_testRoot, true);
    }
}
