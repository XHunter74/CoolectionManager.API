using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenIddict.Validation;

namespace xhunter74.CollectionManager.API.Authorization;

public class UrlTokenAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
{
    public UrlTokenAuthorizationFilter()
    {
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        if (!request.Query.TryGetValue("token", out var token) || string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        try
        {
            var validationService = context.HttpContext.RequestServices.GetRequiredService<OpenIddictValidationService>();
            var result = await validationService.ValidateAccessTokenAsync(token);
            if (!result.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }
        catch
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
