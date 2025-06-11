using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class CreatePossibleValueCommand : ICommand<PossibleValueDto>
{
    public Guid FieldId { get; set; }
    public string Value { get; set; }
    public Guid UserId { get; set; }
}

public class CreatePossibleValueCommandHandler : ICommandHandler<CreatePossibleValueCommand, PossibleValueDto>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<CreatePossibleValueCommandHandler> _logger;

    public CreatePossibleValueCommandHandler(CollectionsDbContext dbContext, ILogger<CreatePossibleValueCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PossibleValueDto> HandleAsync(CreatePossibleValueCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating possible value for field {FieldId}", command.FieldId);

        var field = await _dbContext.CollectionFields
            .FirstOrDefaultAsync(f => f.Id == command.FieldId && f.Collection.OwnerId == command.UserId, cancellationToken);

        if (field == null)
        {
            _logger.LogWarning("Field {FieldId} not found or not owned by user {UserId}", command.FieldId, command.UserId);
            throw new NotFoundException($"Field {command.FieldId} not found or not owned by user {command.UserId}");
        }

        var possibleValue = new PossibleValue
        {
            Id = Guid.NewGuid(),
            Value = command.Value,
            CollectionFieldId = command.FieldId
        };

        _dbContext.PossibleValues.Add(possibleValue);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created possible value {PossibleValueId} for field {FieldId}", possibleValue.Id, command.FieldId);

        var result = new PossibleValueDto
        {
            Id = possibleValue.Id,
            Value = possibleValue.Value
        };

        return result;
    }
}
