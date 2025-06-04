using CQRSMediatr;
using Microsoft.AspNetCore.Authorization;
using xhunter74.CollectionManager.API;
using xhunter74.CollectionManager.API.Permissions.PolicyProvider;
using xhunter74.CollectionManager.API.Settings;
using xhunter74.CollectionManager.Shared.Services;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.Data.Extensions;

public static class AppServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCqrsMediatr(typeof(Startup));

        services.AddScoped<IImageService, LocalImageService>();

        services.AddOptions<AppSettings>()
            .Bind(configuration.GetSection(AppSettings.ConfigSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<StorageSettings>()
            .Bind(configuration.GetSection(StorageSettings.ConfigSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var appSettings = configuration.GetSection(AppSettings.ConfigSection).Get<AppSettings>();
        var storageSettings = configuration.GetSection(StorageSettings.ConfigSection).Get<StorageSettings>();

        switch (appSettings.StorageService)
        {
            case StorageServices.LocalStorageService:
                services.AddScoped<IStorageService, LocalStorageService>(x =>
                {
                    var logger = x.GetRequiredService<ILogger<LocalStorageService>>();
                    return new LocalStorageService(logger, storageSettings.StorageFolder);
                });
                break;
            default:
                throw new NotSupportedException($"Unsupported storage type: {appSettings.StorageService}");
        }

        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
