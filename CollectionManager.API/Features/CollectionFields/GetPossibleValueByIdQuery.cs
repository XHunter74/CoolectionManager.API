using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class GetPossibleValueByIdQuery : IQuery<PossibleValueDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class GetPossibleValueByIdQueryHandler : IQueryHandler<GetPossibleValueByIdQuery, PossibleValueDto>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<GetPossibleValueByIdQueryHandler> _logger;

    public GetPossibleValueByIdQueryHandler(CollectionsDbContext dbContext, ILogger<GetPossibleValueByIdQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PossibleValueDto> HandleAsync(GetPossibleValueByIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting possible value {PossibleValueId}", query.Id);

        var possibleValue = await _dbContext.PossibleValues
            .Include(v => v.CollectionField).ThenInclude(v => v.Collection)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == query.Id, cancellationToken);

        if (possibleValue == null || possibleValue.CollectionField.Collection.OwnerId != query.UserId)
        {
            _logger.LogWarning("Possible value {PossibleValueId} not found or not owned by user", query.Id);
            throw new NotFoundException($"Possible value {query.Id} not found or not owned by user");
        }

        var result = new PossibleValueDto
        {
            Id = possibleValue.Id,
            Value = possibleValue.Value
        };

        return result;
    }
}
