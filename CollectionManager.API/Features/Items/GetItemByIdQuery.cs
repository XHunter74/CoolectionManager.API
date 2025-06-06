﻿using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Extensions;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Items;

public class GetItemByIdQuery : IQuery<ExpandoObject>
{
    public Guid ItemId { get; set; }
    public Guid UserId { get; set; }
}

public class GetItemByIdQueryHandler : IQueryHandler<GetItemByIdQuery, ExpandoObject>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly IMongoDbContext _mongoDbContext;
    private readonly ILogger<GetItemByIdQueryHandler> _logger;

    public GetItemByIdQueryHandler(CollectionsDbContext dbContext, IMongoDbContext mongoDbContext, ILogger<GetItemByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    public async Task<ExpandoObject> HandleAsync(GetItemByIdQuery query, CancellationToken cancellationToken)
    {
        var item = await _mongoDbContext.CollectionItems
            .GetByIdAsync(query.ItemId, cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("Item with ID {Id} not found for user {UserId}", query.ItemId, query.UserId);
            throw new NotFoundException($"Item '{query.ItemId}' not found");
        }

        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == item.CollectionId && c.OwnerId == query.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with item ID {Id} not found for user {UserId}", query.ItemId, query.UserId);
            throw new NotFoundException($"Collection with item ID '{query.ItemId}' not found");
        }
        return item.ToFlattenedExpando();
    }
}
