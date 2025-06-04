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

        services.AddAppServices(Configuration);

        services.AddIdentityServices(Configuration);

        services.AddSwaggerServices();
    }

    public void Configure(IApplicationBuilder builder, IWebHostEnvironment env)
    {
        builder.UseResponseCompression();

        if (env.IsDevelopment())
        {
            builder.UseDeveloperExceptionPage();
            builder.UseSwagger();
            var assembly = Assembly.GetEntryAssembly();
            builder.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", assembly.FullName.Split(",").First()));
        }

        builder.UseAppExceptionHandler();

        //TODO - Add CORS policy configuration
        builder.UseCors("CorsPolicy");

        builder.UseRouting();

        builder.UseAuthentication();
        builder.UseAuthorization();

        builder.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}