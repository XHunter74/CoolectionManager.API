using Microsoft.AspNetCore.Authorization;

namespace xhunter74.CollectionManager.API.Permissions.PolicyProvider;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(PermissionOperator permissionOperator, string[] permissions)
    {
        if (permissions.Length == 0)
            throw new ArgumentException("At least one permission is required.", nameof(permissions));

        PermissionOperator = permissionOperator;
        Permissions = permissions;
    }

    public PermissionOperator PermissionOperator { get; }

    public string[] Permissions { get; }
}