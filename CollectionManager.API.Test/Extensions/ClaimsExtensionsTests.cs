using System.Security.Claims;
using xhunter74.CollectionManager.API.Extensions;

namespace CollectionManager.API.Test.Extensions;

public class ClaimsExtensionsTests
{
    [Fact(DisplayName = "UserId returns Guid from 'sub' claim if authenticated")]
    public void UserId_ReturnsGuid_WhenAuthenticated()
    {
        var guid = Guid.NewGuid();
        var claims = new[] { new Claim("sub", guid.ToString()) };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        Assert.Equal(guid, principal.UserId());
    }

    [Fact(DisplayName = "UserId throws if 'sub' claim missing")]
    public void UserId_Throws_WhenSubMissing()
    {
        var identity = new ClaimsIdentity(new[] { new Claim("other", "val") }, "test");
        var principal = new ClaimsPrincipal(identity);
        Assert.Throws<InvalidDataException>(() => principal.UserId());
    }

    [Fact(DisplayName = "UserId returns Guid.Empty if not authenticated")]
    public void UserId_ReturnsEmpty_WhenNotAuthenticated()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        Assert.Equal(Guid.Empty, principal.UserId());
    }

    [Fact(DisplayName = "UserName returns value from 'preferred_username' claim if authenticated")]
    public void UserName_ReturnsValue_WhenAuthenticated()
    {
        var claims = new[] { new Claim("preferred_username", "testuser") };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        Assert.Equal("testuser", principal.UserName());
    }

    [Fact(DisplayName = "UserName throws if 'preferred_username' claim missing")]
    public void UserName_Throws_WhenPreferredUsernameMissing()
    {
        var identity = new ClaimsIdentity(new[] { new Claim("other", "val") }, "test");
        var principal = new ClaimsPrincipal(identity);
        Assert.Throws<InvalidDataException>(() => principal.UserName());
    }

    [Fact(DisplayName = "UserName returns empty string if not authenticated")]
    public void UserName_ReturnsEmpty_WhenNotAuthenticated()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        Assert.Equal(string.Empty, principal.UserName());
    }
}
