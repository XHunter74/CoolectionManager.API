using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.CollectionFields;
using xhunter74.CollectionManager.API.Models;

namespace xhunter74.CollectionManager.API.Controllers;

/// <summary>
/// Controller for managing fields of a collection. All operations are restricted to the owner of the collection.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollectionFieldsController : ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;

    /// <summary>
    /// Constructor for CollectionFieldsController.
    /// </summary>
    public CollectionFieldsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr)
    {
        _logger = logger;
        _mediatr = mediatr;
    }

    /// <summary>
    /// Gets all fields for a specific collection owned by the current user.
    /// </summary>
    /// <param name="collectionId">The collection's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of collection fields.</returns>
    /// <response code="200">Returns the list of fields.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpGet("/api/Collections/{collectionId:guid}/[controller]")]
    [ProducesResponseType(typeof(IEnumerable<CollectionFieldDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<CollectionFieldDto>>> GetCollectionFieldsAsync(Guid collectionId, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetCollectionFieldsQuery
        {
            CollectionId = collectionId,
            UserId = userId
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific field by its unique identifier, if it belongs to a collection owned by the current user.
    /// </summary>
    /// <param name="id">The field's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection field.</returns>
    /// <response code="200">Returns the field.</response>
    /// <response code="404">Field not found or not owned by user.</response>
    [HttpGet("{id:guid}", Name = nameof(GetFieldByIdAsync))]
    [ProducesResponseType(typeof(CollectionFieldDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CollectionFieldDto>> GetFieldByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetFieldByIdQuery
        {
            Id = id,
            UserId = userId
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new field for a specific collection owned by the current user.
    /// </summary>
    /// <param name="collectionId">The collection's unique identifier.</param>
    /// <param name="model">The field creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created field.</returns>
    /// <response code="201">Field created successfully.</response>
    /// <response code="404">Collection not found or not owned by user.</response>
    [HttpPost("/api/Collections/{collectionId:guid}/[controller]")]
    [ProducesResponseType(typeof(CollectionFieldDto), 201)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CollectionFieldDto>> CreateAsync(Guid collectionId,
        [FromBody] CreateCollectionFieldDto model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.SendAsync(new CreateCollectionFieldCommand
        {
            CollectionId = collectionId,
            UserId = userId,
            Model = model,
        }, cancellationToken);

        return CreatedAtRoute(nameof(GetFieldByIdAsync), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing field if it belongs to a collection owned by the current user.
    /// </summary>
    /// <param name="id">The field's unique identifier.</param>
    /// <param name="model">The field update data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated field.</returns>
    /// <response code="200">Field updated successfully.</response>
    /// <response code="404">Field not found or not owned by user.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CollectionFieldDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CollectionFieldDto>> UpdateAsync(Guid id, [FromBody] UpdateCollectionFieldDto model,
        CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.SendAsync(new UpdateCollectionFieldCommand
        {
            Id = id,
            UserId = userId,
            Model = model,
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a field if it belongs to a collection owned by the current user.
    /// </summary>
    /// <param name="id">The field's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Field deleted successfully.</response>
    /// <response code="404">Field not found or not owned by user.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.SendAsync(new DeleteCollectionFieldCommand
        {
            Id = id,
            UserId = userId,
        }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns all available field types.
    /// </summary>
    /// <returns>List of field types.</returns>
    [HttpGet("types")]
    [ProducesResponseType(typeof(FieldTypeDto[]), 200)]
    public IActionResult GetFieldTypes()
    {
        var types = Enum.GetValues<Data.Entity.FieldTypes>()
            .Cast<Data.Entity.FieldTypes>()
            .Select(e => new FieldTypeDto
            {
                Value = (int)e,
                Name = e.ToString()
            })
            .ToArray();

        return Ok(types);
    }
}
