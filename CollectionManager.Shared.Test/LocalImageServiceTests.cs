using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using xhunter74.CollectionManager.Shared.Services;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace CollectionManager.Shared.Test;

public class LocalImageServiceTests
{
    private readonly Mock<ILogger<LocalImageService>> _loggerMock = new();
    private readonly LocalImageService _service;

    public LocalImageServiceTests()
    {
        _service = new LocalImageService(_loggerMock.Object);
    }

    [Fact]
    public async Task ConvertToPngAsync_ThrowsOnNullOrEmpty()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _service.ConvertToPngAsync(null));
        await Assert.ThrowsAsync<BadRequestException>(() => _service.ConvertToPngAsync(Array.Empty<byte>()));
    }

    [Fact]
    public async Task ConvertToPngAsync_ConvertsJpegToPng()
    {
        using var image = new Image<Rgba32>(1, 1);
        image[0, 0] = new Rgba32(0, 0, 0, 255); 
        using var jpegStream = new MemoryStream();
        await image.SaveAsJpegAsync(jpegStream);
        var jpegBytes = jpegStream.ToArray();

        var pngBytes = await _service.ConvertToPngAsync(jpegBytes);
        Assert.NotNull(pngBytes);
        Assert.True(pngBytes.Length > 0);

        using var pngStream = new MemoryStream(pngBytes);
        using var pngImage = await Image.LoadAsync<Rgba32>(pngStream);
        Assert.Equal(1, pngImage.Width);
        Assert.Equal(1, pngImage.Height);
        Assert.Equal(new Rgba32(0, 0, 0, 255), pngImage[0, 0]);
    }
}
