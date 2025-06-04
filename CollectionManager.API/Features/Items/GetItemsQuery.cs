using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Items;

public class GetItemsQuery : IQuery<IEnumerable<DynamicItemRecord>>
{
    public Guid CollectionId { get; set; }
    public Guid UserId { get; set; }
}

public class GetItemsQueryHandler : IQueryHandler<GetItemsQuery, IEnumerable<DynamicItemRecord>>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly IMongoDbContext _mongoDbContext;
    private readonly ILogger<GetItemsQueryHandler> _logger;

    public GetItemsQueryHandler(
        CollectionsDbContext dbContext,
        IMongoDbContext mongoDbContext,
        ILogger<GetItemsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<DynamicItemRecord>> HandleAsync(GetItemsQuery query, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == query.CollectionId && c.OwnerId == query.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", query.CollectionId, query.UserId);
            throw new NotFoundException($"Collection '{query.CollectionId}' not found");
        }
        var items = await _mongoDbContext.CollectionItems
            .GetAllCollectionItemsAsync(query.CollectionId, cancellationToken);
        return items;
    }
}