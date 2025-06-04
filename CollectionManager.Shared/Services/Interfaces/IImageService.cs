namespace xhunter74.CollectionManager.Shared.Services.Interfaces;

public interface IImageService
{
    Task<byte[]> ConvertToPngAsync(byte[] sources);
}
