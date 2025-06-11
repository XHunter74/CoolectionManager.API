using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class UpdatePossibleValueCommand : ICommand<PossibleValueDto>
{
    public Guid Id { get; set; }
    public PossibleValueDto Model { get; set; }
    public Guid UserId { get; set; }
}

public class UpdatePossibleValueCommandHandler : ICommandHandler<UpdatePossibleValueCommand, PossibleValueDto>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<UpdatePossibleValueCommandHandler> _logger;

    public UpdatePossibleValueCommandHandler(CollectionsDbContext dbContext, ILogger<UpdatePossibleValueCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PossibleValueDto> HandleAsync(UpdatePossibleValueCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating possible value {PossibleValueId}", command.Id);

        var possibleValue = await _dbContext.PossibleValues
            .Include(v => v.CollectionField).ThenInclude(v => v.Collection)
            .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken);

        if (possibleValue == null || possibleValue.CollectionField.Collection.OwnerId != command.UserId)
        {
            _logger.LogWarning("Possible value {PossibleValueId} not found or not owned by user", command.Id);
            throw new NotFoundException($"Possible value {command.Id} not found or not owned by user");
        }

        possibleValue.Value = command.Model.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated possible value {PossibleValueId}", command.Id);

        var result = new PossibleValueDto
        {
            Id = command.Id,
            Value = possibleValue.Value
        };

        return result;
    }
}
