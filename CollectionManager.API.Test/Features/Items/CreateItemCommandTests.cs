using xhunter74.CollectionManager.API.Features.Items;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace CollectionManager.API.Test.Features.Items;

public class CreateItemCommandTests : BaseConnectorTest<CreateItemCommandHandler>
{
    private readonly CreateItemCommandHandler _handler;

    public CreateItemCommandTests()
    {
        _handler = new CreateItemCommandHandler(CollectionsDbContext, MongoDbContextMock, LoggerMock.Object);
    }


    [Fact(DisplayName = "HandleAsync throws NotFoundException if collection does not exist")]
    public async Task HandleAsync_ThrowsNotFoundException_WhenCollectionDoesNotExist()
    {
        var command = new CreateItemCommand
        {
            CollectionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Model = new CreateItemDto()
        };
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync throws BadRequestException if required field is missing")]
    public async Task HandleAsync_ThrowsBadRequestException_WhenRequiredFieldIsMissing()
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Description = "Test Collection",
            OwnerId = Guid.NewGuid(),
            Fields = new List<CollectionField> { new CollectionField { DisplayName = "Field1", IsRequired = true } }
        };

        CollectionsDbContext.Collections.Add(collection);
        CollectionsDbContext.SaveChanges();

        var command = new CreateItemCommand
        {
            CollectionId = collection.Id,
            UserId = collection.OwnerId,
            Model = new CreateItemDto()
        };

        await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync returns ItemDto when item is created successfully")]
    public async Task HandleAsync_ReturnsItemDto_WhenItemIsCreatedSuccessfully()
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Name = "Test Collection",
            Description = "Test Collection",
            Fields = new List<CollectionField> { new CollectionField { DisplayName = "Field1", IsRequired = false, Id = Guid.NewGuid() } }
        };

        CollectionsDbContext.Collections.Add(collection);
        CollectionsDbContext.SaveChanges();

        var field = collection.Fields.First();
        var command = new CreateItemCommand
        {
            CollectionId = collection.Id,
            UserId = collection.OwnerId,
            Model = new CreateItemDto
            {
                DisplayName = "Test Item",
                Picture = null,
                Values = new[] { new CreateItemValue { FieldId = field.Id, Value = "val" } }
            }
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(collection.Id, result.CollectionId);
        Assert.Equal("Test Item", result.DisplayName);
        Assert.Contains(result.Values, v => v.FieldId == field.Id && (string)v.Value == "val");
    }
}
