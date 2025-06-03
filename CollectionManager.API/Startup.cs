using CQRSMediatr;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Serilog;
using xhunter74.CollectionManager.API.Data;
using xhunter74.CollectionManager.API.Extensions;
using Microsoft.AspNetCore.Identity;

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
        services.AddCqrsMediatr(typeof(Startup));
        services.AddDbContext<CollectionsDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("CollectionsDb")));

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            //TODO Identity options configuration (password, lockout, etc.)
        })
        .AddEntityFrameworkStores<CollectionsDbContext>()
        .AddDefaultTokenProviders();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<CollectionsDbContext>();
            })
            .AddServer(options =>
            {
                options.AllowPasswordFlow();
                options.AllowRefreshTokenFlow();
                options.SetTokenEndpointUris("/connect/token");
                options.AcceptAnonymousClients();
                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var version = assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;
                var assemblyName = assembly.GetName().Name;

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = assemblyName,
                    Version = version ?? "v1"
                });

                var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile, true);
            }
        });
    }

    public void Configure(IApplicationBuilder builder, IWebHostEnvironment env)
    {
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