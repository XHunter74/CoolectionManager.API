using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.Items;
using xhunter74.CollectionManager.API.Models;
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

    /// <summary>
    /// Constructor for ItemsController.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="mediatr">CQRS Mediator instance.</param>
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
    /// <param name="id">The collection's unique identifier.</param>
    /// <param name="fields">Optional query parameter to filter returned fields for each item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of items in the collection.</returns>
    /// <response code="200">Returns the list of items.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpGet("/api/Collections/{id:guid}/[controller]")]
    [ProducesResponseType(typeof(IEnumerable<CollectionItemRecord>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItemsAsync(
        [FromRoute] Guid id,
        [FromQuery] string[]? fields,
        CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetItemsQuery
        {
            CollectionId = id,
            UserId = userId,
            Fields = fields
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific item by its unique identifier, if it belongs to a collection owned by the current user.
    /// </summary>
    /// <param name="id">The item's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The item if found.</returns>
    /// <response code="200">Returns the item.</response>
    /// <response code="404">Item not found or not owned by user.</response>
    [HttpGet("{id:guid}", Name = nameof(GetItemByIdAsync))]
    [ProducesResponseType(typeof(CollectionItemRecord), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItemByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
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
    /// <param name="collectionId">The collection's unique identifier.</param>
    /// <param name="model">The item creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created item.</returns>
    /// <response code="201">Item created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpPost("/api/Collections/{collectionId:guid}/[controller]")]
    [ProducesResponseType(typeof(CollectionItemRecord), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateItemAsync(
        [FromRoute] Guid collectionId,
        [FromBody] CreateItemDto model,
        CancellationToken cancellationToken)
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

        return CreatedAtRoute(nameof(GetItemByIdAsync), new { id = newItem.Id }, newItem);
    }

    /// <summary>
    /// Updates an existing item for the current user.
    /// </summary>
    /// <param name="id">The item's unique identifier.</param>
    /// <param name="model">The item update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated item.</returns>
    /// <response code="200">Item updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Item not found or not owned by user.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CollectionItemRecord), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateItemAsync(
        [FromRoute] Guid id,
        [FromBody] CreateItemDto model,
        CancellationToken cancellationToken)
    {
        var userId = User.UserId();

        var updatedItem = await _mediatr.SendAsync(new UpdateItemCommand
        {
            ItemId = id,
            UserId = userId,
            Model = model
        }, cancellationToken);

        if (updatedItem == null)
        {
            return BadRequest("Failed to create item. Please check the provided data.");
        }

        return Ok(updatedItem);
    }
}
