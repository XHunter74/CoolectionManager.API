using CQRSMediatr.Interfaces;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Features.Collections;

public class CreateCollectionCommand : ICommand<CollectionDto>
{
    public Guid UserId { get; set; }
    public CreateCollectionDto Model { get; set; }
}

public class CreateCollectionCommandHandler : ICommandHandler<CreateCollectionCommand, CollectionDto>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<CreateCollectionCommandHandler> _logger;

    public CreateCollectionCommandHandler(ILogger<CreateCollectionCommandHandler> logger, CollectionsDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CollectionDto> HandleAsync(CreateCollectionCommand command, CancellationToken cancellationToken)
    {
        var entity = new Collection
        {
            Id = Guid.NewGuid(),
            Name = command.Model.Name,
            Description = command.Model.Description,
            OwnerId = command.UserId
        };

        _dbContext.Collections.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
        };
    }
}