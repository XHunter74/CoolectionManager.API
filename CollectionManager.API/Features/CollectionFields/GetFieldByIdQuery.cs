using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class GetFieldByIdQuery : IQuery<CollectionFieldDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class GetFieldByIdQueryHandler : IQueryHandler<GetFieldByIdQuery, CollectionFieldDto>
{
    private readonly ILogger<GetFieldByIdQueryHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public GetFieldByIdQueryHandler(ILogger<GetFieldByIdQueryHandler> logger, CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CollectionFieldDto> HandleAsync(GetFieldByIdQuery query, CancellationToken cancellationToken)
    {
        var field = await _dbContext.CollectionFields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.Id && f.Collection.OwnerId == query.UserId, cancellationToken);

        if (field == null)
        {
            _logger.LogWarning("Field with ID {Id} not found for user {UserId}", query.Id, query.UserId);
            throw new NotFoundException($"Field with ID {query.Id} not found");
        }

        return new CollectionFieldDto
        {
            Id = field.Id,
            Name = field.Name,
            Description = field.Description,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Order = field.Order,
            CollectionId = field.CollectionId
        };
    }
}