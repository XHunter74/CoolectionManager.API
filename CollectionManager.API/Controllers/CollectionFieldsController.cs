using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollectionFieldsController: ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;
    private readonly CollectionsDbContext _dbContext;

    public CollectionFieldsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr,
        CollectionsDbContext dbContext)
    {
        _logger = logger;
        _mediatr = mediatr;
        _dbContext = dbContext;
    }

    [HttpGet("{collectionId:guid}")]
    public async Task<ActionResult<IEnumerable<CollectionFieldDto>>> GetFields(Guid collectionId, 
        CancellationToken cancellationToken)
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

    [HttpGet("field/{id:guid}")]
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

    [HttpPost("{collectionId:guid}")]
    public async Task<ActionResult<CollectionFieldDto>> Create(Guid collectionId, [FromBody] CreateCollectionFieldDto model, 
        CancellationToken cancellationToken)
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

    [HttpPut("field/{id:guid}")]
    public async Task<ActionResult<CollectionFieldDto>> Update(Guid id, [FromBody] UpdateCollectionFieldDto model, 
        CancellationToken cancellationToken)
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

    [HttpDelete("field/{id:guid}")]
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
