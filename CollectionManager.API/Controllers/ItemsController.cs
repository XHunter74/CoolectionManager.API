using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Records;

namespace xhunter74.CollectionManager.API.Controllers;

/// <summary>
/// Controller for managing items in a collection. Items are stored in MongoDB and belong to a specific collection.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;
    private readonly CollectionsDbContext _dbContext;
    private readonly IMongoDbContext _mongoDbContext;

    /// <summary>
    /// Constructor for ItemsController.
    /// </summary>
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

    /// <summary>
    /// Gets all items for a specific collection owned by the current user.
    /// </summary>
    /// <param name="id">The collection's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of items in the collection.</returns>
    /// <response code="200">Returns the list of items.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpGet("/api/Collections/{id:guid}/[controller]")]
    [ProducesResponseType(typeof(IEnumerable<DynamicItemRecord>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItemsAsync(Guid id, CancellationToken cancellationToken)
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

        var items = await _mongoDbContext.CollectionItems
            .GetAllCollectionItemsAsync(id, cancellationToken);

        return Ok(items);
    }

    /// <summary>
    /// Gets a specific item by its unique identifier, if it belongs to a collection owned by the current user.
    /// </summary>
    /// <param name="id">The item's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The item record.</returns>
    /// <response code="200">Returns the item.</response>
    /// <response code="404">Item or collection not found or not owned by user.</response>
    [HttpGet("{id:guid}", Name = nameof(GetItemByIdAsync))]
    [ProducesResponseType(typeof(DynamicItemRecord), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItemByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();

        var item = await _mongoDbContext.CollectionItems
            .GetByIdAsync(id, cancellationToken);

        if (item == null)
        {
            _logger.LogWarning("Item with ID {Id} not found for user {UserId}", id, userId);
            return NotFound(new { Message = "Item not found" });
        }

        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == item.CollectionId && c.OwnerId == userId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", id, userId);
            return NotFound(new { Message = "Collection not found" });
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a new item in a specific collection owned by the current user.
    /// </summary>
    /// <param name="collectionId">The collection's unique identifier.</param>
    /// <param name="model">The item creation data (array of field name/value pairs).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created item.</returns>
    /// <response code="201">Item created successfully.</response>
    /// <response code="400">Field not found in collection.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpPost("/api/Collections/{collectionId:guid}/[controller]")]
    [ProducesResponseType(typeof(DynamicItemRecord), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateItemAsync(Guid collectionId, [FromBody] CreateItemDto[] model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();

        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.OwnerId == userId, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", collectionId, userId);
            return NotFound(new { Message = "Collection not found" });
        }

        var itemDoc = new DynamicItemRecord { CollectionId = collectionId };

        foreach (var item in model)
        {
            var field = collection.Fields
                .FirstOrDefault(f => f.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase));

            if (field == null)
            {
                _logger.LogWarning("Field with name {FieldName} not found in collection {CollectionId} for user {UserId}", item.Name, collectionId, userId);
                return BadRequest(new { Message = "Field not found in collection" });
            }

            itemDoc.Fields.Add(field.Name, item.Value != null ? (BsonValue)item.Value : BsonNull.Value);
        }

        var newItem = await _mongoDbContext.CollectionItems.AddAsync(itemDoc, cancellationToken);

        return CreatedAtRoute(nameof(GetItemByIdAsync), new { id = newItem.Id }, newItem);
    }
}
