using Microsoft.Extensions.Logging;

namespace xhunter74.CollectionManager.Shared.Services;

public class BaseService
{
    public ILogger Logger { get; }

    public BaseService(ILogger logger)
    {
        Logger = logger;
    }
}
