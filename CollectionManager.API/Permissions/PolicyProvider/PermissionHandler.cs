using Microsoft.AspNetCore.Authorization;

namespace xhunter74.CollectionManager.API.Permissions.PolicyProvider;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (requirement.PermissionOperator == PermissionOperator.And)
        {
            foreach (var permission in requirement.Permissions)
            {
                var permissionClaim = context.User.Claims
                    .FirstOrDefault(e => e.Type.Equals(AppClaimTypes.UserPermissionClaim) && e.Value == permission);

                if (permissionClaim == null)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        foreach (var permission in requirement.Permissions)
        {
            var permissionClaim = context.User.Claims
                .FirstOrDefault(e => e.Type.Equals(AppClaimTypes.UserPermissionClaim) && e.Value == permission);

            if (permissionClaim != null)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        context.Fail();
        return Task.CompletedTask;
    }
}