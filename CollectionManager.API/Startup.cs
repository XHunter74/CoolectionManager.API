using CQRSMediatr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using System.Reflection;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Settings;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Services;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

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
        {
            options.UseNpgsql(Configuration.GetConnectionString("CollectionsDb"));
            options.UseOpenIddict<Guid>();
        });

        services.AddOptions<StorageSettings>()
            .Bind(Configuration.GetSection(StorageSettings.ConfigSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var storageSettings = Configuration.GetSection(StorageSettings.ConfigSection).Get<StorageSettings>();

        services.AddScoped<IStorageService, LocalStorageService>(x =>
        {
            var logger = x.GetRequiredService<ILogger<LocalStorageService>>();
            return new LocalStorageService(logger, storageSettings.StorageFolder);
        });

        services.AddOptions<IdentitySettings>()
            .Bind(Configuration.GetSection(IdentitySettings.ConfigSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            //TODO Identity options configuration (password, lockout, etc.)
        })
        .AddEntityFrameworkStores<CollectionsDbContext>()
        .AddDefaultTokenProviders();

        var identitySettings = Configuration.GetSection(IdentitySettings.ConfigSection).Get<IdentitySettings>();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<CollectionsDbContext>()
                       .ReplaceDefaultEntities<Guid>();

            })
                .AddServer(options =>
                {
                    // Enable the required endpoints
                    options.SetTokenEndpointUris("/connect/token");

                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();

                    options.UseReferenceAccessTokens();
                    options.UseReferenceRefreshTokens();

                    options.RegisterScopes(OpenIddictConstants.Permissions.Scopes.Email,
                                    OpenIddictConstants.Permissions.Scopes.Profile,
                                    OpenIddictConstants.Permissions.Scopes.Roles);

                    options.SetAccessTokenLifetime(TimeSpan.FromSeconds(identitySettings.AccessTokenLifetime));
                    options.SetRefreshTokenLifetime(TimeSpan.FromSeconds(identitySettings.RefreshTokenLifetime));

                    //TODO - Change in production
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    options.UseAspNetCore().EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
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