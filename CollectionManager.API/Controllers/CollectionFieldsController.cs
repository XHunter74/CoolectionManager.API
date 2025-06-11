using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.CollectionFields;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

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
        ICqrsMediatr mediatr
        )
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

    /// <summary>
    /// Changes the order of a field in a collection.
    /// </summary>
    /// <param name="id">The field's unique identifier.</param>
    /// <param name="order">The new order value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Order changed successfully.</response>
    /// <response code="404">Field not found or not owned by user.</response>
    [HttpPut("{id:guid}/order")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ChangeOrderAsync(Guid id, [FromQuery] int order, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.SendAsync(new ChangeCollectionFieldOrderCommand
        {
            Id = id,
            UserId = userId,
            Order = order
        }, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Gets all possible values for a specific collection field.
    /// </summary>
    /// <param name="fieldId">The field's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of possible values.</returns>
    /// <response code="200">Returns the list of possible values.</response>
    /// <response code="404">Field not found or not owned by user.</response>
    [HttpGet("{fieldId:guid}/possible-values")]
    [ProducesResponseType(typeof(IEnumerable<PossibleValue>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<PossibleValue>>> GetPossibleValuesAsync(Guid fieldId, CancellationToken cancellationToken)
    {
        var query = new GetPossibleValuesQuery { FieldId = fieldId, UserId = User.UserId() };
        var result = await _mediatr.QueryAsync(query, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific possible value by its unique identifier.
    /// </summary>
    /// <param name="id">The possible value's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The possible value.</returns>
    /// <response code="200">Returns the possible value.</response>
    /// <response code="404">Possible value not found or not owned by user.</response>
    [HttpGet("possible-values/{id:guid}")]
    [ProducesResponseType(typeof(PossibleValue), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PossibleValue>> GetPossibleValueByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPossibleValueByIdQuery { Id = id, UserId = User.UserId() };
        var result = await _mediatr.QueryAsync(query, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Creates a new possible value for a specific collection field.
    /// </summary>
    /// <param name="fieldId">The field's unique identifier.</param>
    /// <param name="model">The possible value data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created possible value.</returns>
    /// <response code="201">Possible value created successfully.</response>
    /// <response code="404">Field not found or not owned by user.</response>
    [HttpPost("{fieldId:guid}/possible-values")]
    [ProducesResponseType(typeof(PossibleValue), 201)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PossibleValue>> CreatePossibleValueAsync(Guid fieldId, [FromBody] string value, CancellationToken cancellationToken)
    {
        var command = new CreatePossibleValueCommand { FieldId = fieldId, Value = value, UserId = User.UserId() };
        var result = await _mediatr.SendAsync(command, cancellationToken);
        if (result == null) return NotFound();
        return CreatedAtAction(nameof(GetPossibleValueByIdAsync), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing possible value.
    /// </summary>
    /// <param name="id">The possible value's unique identifier.</param>
    /// <param name="model">The new value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated possible value.</returns>
    /// <response code="200">Possible value updated successfully.</response>
    /// <response code="404">Possible value not found or not owned by user.</response>
    [HttpPut("possible-values/{id:guid}")]
    [ProducesResponseType(typeof(PossibleValue), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PossibleValue>> UpdatePossibleValueAsync(Guid id, [FromBody] string value, CancellationToken cancellationToken)
    {
        var command = new UpdatePossibleValueCommand { Id = id, Value = value, UserId = User.UserId() };
        var result = await _mediatr.SendAsync(command, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Deletes a possible value.
    /// </summary>
    /// <param name="id">The possible value's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Possible value deleted successfully.</response>
    /// <response code="404">Possible value not found or not owned by user.</response>
    [HttpDelete("possible-values/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePossibleValueAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePossibleValueCommand { Id = id, UserId = User.UserId() };
        var success = await _mediatr.SendAsync(command, cancellationToken);
        if (!success) return NotFound();
        return NoContent();
    }
}
