using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;

namespace xhunter74.CollectionManager.API.Controllers;

//TODO Need to refactor this controller to use CQRS pattern
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;
    private readonly CollectionsDbContext _dbContext;
    private readonly IMongoDbContext _mongoDbContext;

    public ItemsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr,
        CollectionsDbContext dbContext,
        IMongoDbContext mongoDbContext
        )
    {
        _logger = logger;
        _mediatr = mediatr;
        _dbContext = dbContext;
        _mongoDbContext = mongoDbContext;
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> CreateItemAsync(Guid id, [FromBody] CreateItemDto[] model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();

        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", id, userId);
            return NotFound(new { Message = "Collection not found" });
        }

        var itemDoc = new BsonDocument
        {
            { "collectionId", id.ToString() },
            { "createdBy", userId.ToString() },
            { "createdAt", DateTime.UtcNow }
        };

        foreach (var item in model)
        {
            var field = collection.Fields
                .FirstOrDefault(f => f.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase));

            if (field == null)
            {
                _logger.LogWarning("Field with name {FieldName} not found in collection {CollectionId} for user {UserId}", item.Name, id, userId);
                return BadRequest(new { Message = "Field not found in collection" });
            }

            itemDoc.Add(field.Name, item.Value != null ? (BsonValue)item.Value : BsonNull.Value);
        }

        await _mongoDbContext.CollectionItems.AddAsync(itemDoc);

        return Created();
        //return CreatedAtRoute(nameof(GetItemByIdAsync), new { id = result.Id }, result);
    }
}
