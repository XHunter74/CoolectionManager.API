using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Features.Collections;

public class GetCollectionsQuery : IQuery<IEnumerable<CollectionDto>>
{
    public Guid UserId { get; set; }
}

public class GetCollectionsQueryHandler : IQueryHandler<GetCollectionsQuery, IEnumerable<CollectionDto>>
{
    private readonly CollectionsDbContext _dbContext;

    public GetCollectionsQueryHandler(CollectionsDbContext dbContext) => _dbContext = dbContext;

    public async Task<IEnumerable<CollectionDto>> HandleAsync(GetCollectionsQuery query, CancellationToken cancellationToken)
    {
        var collections = await _dbContext.Collections
            .Where(c => c.OwnerId == query.UserId)
            .AsNoTracking()
            .Select(c => new CollectionDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
            })
            .ToListAsync(cancellationToken);
        return collections;
    }
}