using Microsoft.AspNetCore.Http;

namespace xhunter74.CollectionManager.Shared.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(IEnumerable<string> errors) : base(errors)
    {
    }

    public NotFoundException(string error) : base(error)
    {
    }

    public NotFoundException()
    {
    }

    public override int HttpStatusCode => StatusCodes.Status404NotFound;
}
