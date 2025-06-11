using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class GetPossibleValuesQuery : IQuery<IEnumerable<PossibleValue>?>
{
    public Guid FieldId { get; set; }
    public Guid UserId { get; set; }
}

public class GetPossibleValuesQueryHandler : IQueryHandler<GetPossibleValuesQuery, IEnumerable<PossibleValue>?>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<GetPossibleValuesQueryHandler> _logger;

    public GetPossibleValuesQueryHandler(CollectionsDbContext dbContext, ILogger<GetPossibleValuesQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<PossibleValue>?> HandleAsync(GetPossibleValuesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting possible values for field {FieldId}", query.FieldId);

        var field = await _dbContext.CollectionFields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.FieldId && f.Collection.OwnerId == query.UserId, cancellationToken);

        if (field == null)
        {
            _logger.LogWarning("Field {FieldId} not found or not owned by user {UserId}", query.FieldId, query.UserId);
            throw new NotFoundException($"Field {query.FieldId} not found or not owned by user {query.UserId}");
        }

        var values = await _dbContext.PossibleValues
            .AsNoTracking()
            .Where(v => v.CollectionFieldId == query.FieldId)
            .ToListAsync(cancellationToken);

        return values;
    }
}
