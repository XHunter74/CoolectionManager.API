using Microsoft.AspNetCore.Mvc;

namespace xhunter74.CollectionManager.API.Permissions;

public class CustomError
{
    public string Error { get; }

    public CustomError(string message)
    {
        Error = message;
    }
}

public class CustomUnauthorizedResult : JsonResult
{
    public CustomUnauthorizedResult(string message, int statusCode) : base(new CustomError(message))
    {
        StatusCode = statusCode;
    }
}