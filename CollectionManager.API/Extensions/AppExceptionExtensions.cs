using xhunter74.CollectionManager.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using Serilog.Events;

namespace xhunter74.CollectionManager.API.Extensions;

public static class AppExceptionExtensions
{
    public static void UseAppExceptionHandler(this IApplicationBuilder appBuilder)
    {
        appBuilder.UseExceptionHandler(x =>
        {
            x.Run(async context =>
            {
                var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (errorFeature != null)
                {
                    if (errorFeature.Error is not AppException error)
                    {
                        if (Log.Logger.IsEnabled(LogEventLevel.Error))
                            Log.Logger.Error(errorFeature.Error,
                                $"{errorFeature.Endpoint?.DisplayName} -> Exception occurred:\r\n");
                        throw errorFeature.Error;
                    }

                    var result = new AppExceptionModel
                    {
                        Type = error.GetType().Name,
                        Message = error.Message
                    };
                    context.Response.StatusCode = error.HttpStatusCode;
                    await context.Response.WriteAsJsonAsync(result);
                }
            });
        });
    }
}
