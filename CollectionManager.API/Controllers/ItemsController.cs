using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.Items;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Mongo.Extensions;
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

    public ItemsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr)
    {
        _logger = logger;
        _mediatr = mediatr;
    }

    /// <summary>
    /// Gets all items for a specific collection owned by the current user.
    /// </summary>
    [HttpGet("/api/Collections/{id:guid}/[controller]")]
    [ProducesResponseType(typeof(IEnumerable<CollectionItemRecord>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItemsAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetItemsQuery
        {
            CollectionId = id,
            UserId = userId,
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific item by its unique identifier, if it belongs to a collection owned by the current user.
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetItemByIdAsync))]
    [ProducesResponseType(typeof(CollectionItemRecord), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItemByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetItemByIdQuery
        {
            ItemId = id,
            UserId = userId,
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new item in a specific collection owned by the current user.
    /// </summary>
    [HttpPost("/api/Collections/{collectionId:guid}/[controller]")]
    [ProducesResponseType(typeof(CollectionItemRecord), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateItemAsync(Guid collectionId, [FromBody] CreateItemDto[] model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();

        var newItem = await _mediatr.SendAsync(new CreateItemCommand
        {
            CollectionId = collectionId,
            UserId = userId,
            Model = model
        }, cancellationToken);

        if (newItem == null)
        {
            return BadRequest("Failed to create item. Please check the provided data.");
        }

        var newItemId = newItem.GetFieldValue(Constants.IdFieldName);

        return CreatedAtRoute(nameof(GetItemByIdAsync), new { id = newItemId }, newItem);
    }
}
