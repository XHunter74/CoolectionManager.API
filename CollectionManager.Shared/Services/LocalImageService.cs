using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.Shared.Services;

public class LocalImageService : BaseService, IImageService
{
    public LocalImageService(ILogger<LocalImageService> logger) : base(logger)
    {
    }

    public async Task<byte[]> ConvertToPngAsync(byte[] sources)
    {
        if (sources == null || sources.Length == 0)
            throw new BadRequestException("Input byte array is null or empty");

        using var inputStream = new MemoryStream(sources);

        using var image = await Image.LoadAsync<Rgba32>(inputStream);

        using var outStream = new MemoryStream();
        await image.SaveAsPngAsync(outStream, new PngEncoder());

        return outStream.ToArray();
    }
}
