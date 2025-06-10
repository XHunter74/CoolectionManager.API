using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.Collections;
using xhunter74.CollectionManager.API.Models;

namespace xhunter74.CollectionManager.API.Controllers;

/// <summary>
/// Controller for managing user collections.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;

    /// <summary>
    /// Constructor for CollectionsController.
    /// </summary>
    public CollectionsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr)
    {
        _logger = logger;
        _mediatr = mediatr;
    }

    /// <summary>
    /// Gets all collections for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of collections.</returns>
    /// <response code="200">Returns the list of collections.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CollectionDto>), 200)]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetCollectionsQuery
        {
            UserId = userId,
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a collection by its unique identifier for the current user.
    /// </summary>
    /// <param name="id">The collection's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection.</returns>
    /// <response code="200">Returns the collection.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    [ProducesResponseType(typeof(CollectionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CollectionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetCollectionByIdQuery
        {
            Id = id,
            UserId = userId
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new collection for the current user.
    /// </summary>
    /// <param name="model">The collection creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created collection.</returns>
    /// <response code="201">Collection created successfully.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CollectionDto), 201)]
    public async Task<ActionResult<CollectionDto>> CreateAsync([FromBody] CreateCollectionDto model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var newCollection = await _mediatr.SendAsync(new CreateCollectionCommand
        {
            UserId = userId,
            Model = model
        }, cancellationToken);
        return CreatedAtRoute(nameof(GetByIdAsync), new { id = newCollection.Id }, newCollection);
    }

    /// <summary>
    /// Updates an existing collection for the current user.
    /// </summary>
    /// <param name="id">The collection's unique identifier.</param>
    /// <param name="model">The collection update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated collection.</returns>
    /// <response code="200">Collection updated successfully.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CollectionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CollectionDto>> Update(Guid id, [FromBody] UpdateCollectionDto model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.SendAsync(new UpdateCollectionCommand
        {
            Id = id,
            UserId = userId,
            Model = model
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a collection for the current user.
    /// </summary>
    /// <param name="id">The collection's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Collection deleted successfully.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.SendAsync(new DeleteCollectionCommand
        {
            Id = id,
            UserId = userId,
        }, cancellationToken);
        return NoContent();
    }
}
