using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Shared.Validation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace xhunter74.CollectionManager.API.Features;

public abstract class BaseFeatureHandler
{
    public BaseFeatureHandler(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }

    public static void ValidateModel(object model)
    {
        var (isValid, errors) = ModelValidator.ValidateModel(model);

        if (!isValid)
        {
            var errorMessages = errors.Select(e => $"{e.MemberNames}: {e.ErrorMessage}");
            throw new BadRequestException(string.Join(", ", errorMessages));

        }
    }
}
