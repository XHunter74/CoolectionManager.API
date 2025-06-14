using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class GetCollectionFieldsQuery : IQuery<IEnumerable<CollectionFieldDto>>
{
    public Guid CollectionId { get; set; }
    public Guid UserId { get; set; }
}

public class GetCollectionFieldsQueryHandler : IQueryHandler<GetCollectionFieldsQuery, IEnumerable<CollectionFieldDto>>
{
    private readonly ILogger<GetCollectionFieldsQueryHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public GetCollectionFieldsQueryHandler(ILogger<GetCollectionFieldsQueryHandler> logger, CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CollectionFieldDto>> HandleAsync(GetCollectionFieldsQuery query, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CollectionId && c.OwnerId == query.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {CollectionId} not found for user {UserId}", query.CollectionId, query.UserId);
            throw new NotFoundException($"Collection with ID {query.CollectionId} not found");
        }

        var fields = collection.Fields.Select(f => new CollectionFieldDto
        {
            Id = f.Id,
            DisplayName = f.Name,
            IsSystem = f.IsSystem,
            Type = f.Type,
            IsRequired = f.IsRequired,
            Order = f.Order,
            CollectionId = f.CollectionId
        }).ToList();

        return fields;
    }
}