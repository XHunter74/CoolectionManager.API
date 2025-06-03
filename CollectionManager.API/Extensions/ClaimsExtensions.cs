using System.Security.Claims;

namespace xhunter74.CollectionManager.API.Extensions;

public static class ClaimsExtensions
{

    public static Guid UserId(this ClaimsPrincipal user)
    {
        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            var id = user.Claims
                .Where(x => x.Type.Equals("sub", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(id))
                return new Guid(id);
            throw new InvalidDataException("UserId has not specified");
        }

        return Guid.Empty;
    }

    public static string UserName(this ClaimsPrincipal user)
    {
        if (user.Identity.IsAuthenticated)
        {
            var userName = user.Claims
                .Where(x => x.Type.Equals("preferred_username", StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(userName))
                return userName;
            throw new InvalidDataException("UserName has not specified");
        }

        return string.Empty;
    }
}
