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
    private readonly dynamic[] SystemFields = {
        new { Name= "Name", @Type=FieldTypes.String, IsRequired=true},
        new { Name= "Description", @Type=FieldTypes.String, IsRequired=false},
        new { Name= "Picture", @Type=FieldTypes.Image, IsRequired=true}
    };
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<CreateCollectionCommandHandler> _logger;

    public CreateCollectionCommandHandler(ILogger<CreateCollectionCommandHandler> logger, CollectionsDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CollectionDto> HandleAsync(CreateCollectionCommand command, CancellationToken cancellationToken)
    {
        var newCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = command.Model.Name,
            Description = command.Model.Description,
            Image = command.Model.Image,
            OwnerId = command.UserId
        };

        _dbContext.Collections.Add(newCollection);

        var order = 0;
        foreach (var field in SystemFields)
        {
            var newField = new CollectionField
            {
                CollectionId = newCollection.Id,
                Name = field.Name,
                Type = field.Type,
                IsRequired = field.IsRequired,
                IsSystem = true,
                Order = order
            };
            await _dbContext.CollectionFields.AddAsync(newField, cancellationToken);
            order++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionDto
        {
            Id = newCollection.Id,
            Name = newCollection.Name,
            Description = newCollection.Description,
            Image = newCollection.Image
        };
    }
}