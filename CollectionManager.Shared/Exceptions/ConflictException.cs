using Microsoft.AspNetCore.Http;

namespace xhunter74.CollectionManager.Shared.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(IEnumerable<string> errors)
        : base(errors) { }

    public ConflictException(string error)
        : base(error)
    {
    }

    public ConflictException()
    {
    }

    public override int HttpStatusCode => StatusCodes.Status409Conflict;
}