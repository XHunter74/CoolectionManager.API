using Serilog;
using xhunter74.CollectionManager.API;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.Data.Extensions;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));

        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();

        var env = app.Services.GetRequiredService<IWebHostEnvironment>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application starting in '{Environment}' environment", env.EnvironmentName);

        startup.Configure(app, env);

        await app.ApplyDbMigrations();

        await app.SeedIdentityEntities();

        app.Run();
    }
}
