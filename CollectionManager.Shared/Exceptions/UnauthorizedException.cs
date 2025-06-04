using Microsoft.AspNetCore.Http;

namespace xhunter74.CollectionManager.Shared.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(IEnumerable<string> errors) : base(errors)
    {
    }

    public UnauthorizedException(string error) : base(error)
    {
    }

    public UnauthorizedException()
    {
    }

    public override int HttpStatusCode => StatusCodes.Status401Unauthorized;
}
