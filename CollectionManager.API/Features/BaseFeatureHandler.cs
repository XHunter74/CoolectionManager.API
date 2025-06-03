namespace xhunter74.CollectionManager.API.Features;

public abstract class BaseFeatureHandler
{
    public BaseFeatureHandler(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }
}
