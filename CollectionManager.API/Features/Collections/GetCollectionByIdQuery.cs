using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Collections;

public class GetCollectionByIdQuery : IQuery<CollectionDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class GetCollectionByIdQueryHandler : IQueryHandler<GetCollectionByIdQuery, CollectionDto>
{
    private readonly ILogger<GetCollectionByIdQueryHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public GetCollectionByIdQueryHandler(ILogger<GetCollectionByIdQueryHandler> logger, CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    public async Task<CollectionDto> HandleAsync(GetCollectionByIdQuery query, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == query.Id && e.OwnerId == query.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", query.Id, query.UserId);
            throw new NotFoundException($"Collection with ID {query.Id} not found");
        }

        return new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            Image = collection.Image,
        };
    }
}