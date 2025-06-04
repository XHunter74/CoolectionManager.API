using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Controllers;

/// <summary>
/// Controller for managing fields of a collection. All operations are restricted to the owner of the collection.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollectionFieldsController: ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;
    private readonly CollectionsDbContext _dbContext;

    /// <summary>
    /// Constructor for CollectionFieldsController.
    /// </summary>
    public CollectionFieldsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr,
        CollectionsDbContext dbContext)
    {
        _logger = logger;
        _mediatr = mediatr;
        _dbContext = dbContext;
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
    public async Task<ActionResult<IEnumerable<CollectionFieldDto>>> GetCollectionFields(Guid collectionId, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.OwnerId == userId, cancellationToken);
        if (collection == null)
            return NotFound();
        var fields = collection.Fields.Select(f => new CollectionFieldDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            Type = f.Type,
            IsRequired = f.IsRequired,
            Order = f.Order,
            CollectionId = f.CollectionId
        }).ToList();
        return Ok(fields);
    }

    /// <summary>
    /// Gets a specific field by its unique identifier, if it belongs to a collection owned by the current user.
    /// </summary>
    /// <param name="id">The field's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection field.</returns>
    /// <response code="200">Returns the field.</response>
    /// <response code="404">Field not found or not owned by user.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CollectionFieldDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CollectionFieldDto>> GetFieldById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var field = await _dbContext.CollectionFields
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && f.Collection.OwnerId == userId, cancellationToken);
        if (field == null)
            return NotFound();
        return Ok(new CollectionFieldDto
        {
            Id = field.Id,
            Name = field.Name,
            Description = field.Description,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Order = field.Order,
            CollectionId = field.CollectionId
        });
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
    public async Task<ActionResult<CollectionFieldDto>> Create(Guid collectionId, [FromBody] CreateCollectionFieldDto model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var collection = await _dbContext.Collections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.OwnerId == userId, cancellationToken);
        if (collection == null)
            return NotFound();
        var field = new CollectionField
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            Type = model.Type,
            IsRequired = model.IsRequired,
            Order = model.Order,
            CollectionId = collectionId
        };
        _dbContext.CollectionFields.Add(field);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetFieldById), new { id = field.Id }, new CollectionFieldDto
        {
            Id = field.Id,
            Name = field.Name,
            Description = field.Description,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Order = field.Order,
            CollectionId = field.CollectionId
        });
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
    public async Task<ActionResult<CollectionFieldDto>> Update(Guid id, [FromBody] UpdateCollectionFieldDto model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var field = await _dbContext.CollectionFields
            .FirstOrDefaultAsync(f => f.Id == id && f.Collection.OwnerId == userId, cancellationToken);
        if (field == null)
            return NotFound();
        field.Name = model.Name;
        field.Description = model.Description;
        field.Type = model.Type;
        field.IsRequired = model.IsRequired;
        field.Order = model.Order;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new CollectionFieldDto
        {
            Id = field.Id,
            Name = field.Name,
            Description = field.Description,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Order = field.Order,
            CollectionId = field.CollectionId
        });
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
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var field = await _dbContext.CollectionFields
            .FirstOrDefaultAsync(f => f.Id == id && f.Collection.OwnerId == userId, cancellationToken);
        if (field == null)
            return NotFound();
        _dbContext.CollectionFields.Remove(field);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
