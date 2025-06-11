using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class GetPossibleValueByIdQuery : IQuery<PossibleValue?>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class GetPossibleValueByIdQueryHandler : IQueryHandler<GetPossibleValueByIdQuery, PossibleValue?>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<GetPossibleValueByIdQueryHandler> _logger;

    public GetPossibleValueByIdQueryHandler(CollectionsDbContext dbContext, ILogger<GetPossibleValueByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PossibleValue?> HandleAsync(GetPossibleValueByIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting possible value {PossibleValueId}", query.Id);

        var value = await _dbContext.PossibleValues
            .Include(v => v.CollectionField).ThenInclude(v => v.Collection)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == query.Id, cancellationToken);

        if (value == null || value.CollectionField.Collection.OwnerId != query.UserId)
        {
            _logger.LogWarning("Possible value {PossibleValueId} not found or not owned by user", query.Id);
            throw new NotFoundException($"Possible value {query.Id} not found or not owned by user");
        }

        return value;
    }
}
