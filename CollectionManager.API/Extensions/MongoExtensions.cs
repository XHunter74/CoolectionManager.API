using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SixLabors.ImageSharp;
using xhunter74.CollectionManager.API.Settings;
using xhunter74.CollectionManager.Data.Mongo;

namespace xhunter74.CollectionManager.API.Extensions;

public static class MongoExtensions
{
    public static IServiceCollection AddMongoDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.ConfigSection));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<IMongoDbContext>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoDbContext(client, settings.DatabaseName);
        });
        return services;
    }
}
