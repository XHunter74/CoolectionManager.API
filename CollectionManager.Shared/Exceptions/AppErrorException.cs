using Microsoft.AspNetCore.Http;

namespace xhunter74.CollectionManager.Shared.Exceptions;

public class AppErrorException : AppException
{
    public AppErrorException(IEnumerable<string> errors) : base(errors)
    {
    }

    public AppErrorException(string error) : base(error)
    {
    }

    public override int HttpStatusCode => StatusCodes.Status500InternalServerError;
}