namespace xhunter74.CollectionManager.API;

public static class Constants
{
    public const string AvatarFormFieldName = "avatar";
    public const string FileFormFieldName = "file";
    public const string AvatarFileName = "avatar.png";
    public const string AvatarContentType = "image/png";
    public static readonly string[] CompressesContentTypes =
    [
        "application/json",
        "text/plain",
        "application/octet-stream"
    ];
    public const string IdFieldName = "Id";
    public const string AuthTokenEndpoint = "/api/auth/token";
    public const string DockerEnvironment = "Docker";
}
