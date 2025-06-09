using Microsoft.OpenApi.Models;
using System.Reflection;

namespace xhunter74.CollectionManager.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var version = assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion?.Split('+')[0];
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
        return services;
    }
}
