using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Extensions;

namespace xhunter74.CollectionManager.API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(e =>
        {
            e.AddSerilog();
        });
        services.AddControllers();

        services.AddDbContext<CollectionsDbContext>(options =>
        {
            options.UseNpgsql(Configuration.GetConnectionString("CollectionsDb"));
            options.UseOpenIddict<Guid>();
        });

        services.AddMongoDbContext(Configuration);

        services.AddAppServices(Configuration);

        services.AddIdentityServices(Configuration);

        services.AddSwaggerServices();

        // Allow all origins CORS policy
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder builder, IWebHostEnvironment env)
    {
        builder.UseResponseCompression();

        var logger = builder.ApplicationServices.GetRequiredService<ILogger<Startup>>();

        logger.LogInformation("Current environment is '{0}'", env.EnvironmentName);

        if (env.IsDevelopment() || env.EnvironmentName.Equals(Constants.DockerEnvironment, StringComparison.InvariantCultureIgnoreCase))
        {
            logger.LogInformation("Running in development environment. Enabling developer exception page and Swagger UI.");
            builder.UseDeveloperExceptionPage();
            builder.UseSwagger();
            var assembly = Assembly.GetEntryAssembly();
            builder.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", assembly.FullName.Split(",").First()));
        }

        builder.UseAppExceptionHandler();

        builder.UseRouting();

        //TODO - Add CORS policy configuration
        builder.UseCors("CorsPolicy");

        builder.UseAuthentication();
        builder.UseAuthorization();

        builder.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}