using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.API.Extensions;

namespace xhunter74.CollectionManager.API.Controllers;

//TODO Need to refactor this controller to use CQRS pattern
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;
    private readonly CollectionsDbContext _dbContext;

    public CollectionsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr,
        CollectionsDbContext dbContext)
    {
        _logger = logger;
        _mediatr = mediatr;
        this._dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetAll()
    {
        var userId = User.UserId();
        var collections = await _dbContext.Collections
            .Where(c => c.OwnerId == userId)
            .Select(c => new CollectionDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
            })
            .ToListAsync();
        return Ok(collections);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CollectionDto>> GetById(Guid id)
    {
        var userId = User.UserId();
        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == userId);

        if (collection == null)
            return NotFound();

        return Ok(new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
        });
    }


    [HttpPost]
    public async Task<ActionResult<CollectionDto>> Create([FromBody] CreateCollectionDto dto)
    {
        var userId = User.UserId();

        var entity = new Collection
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId
        };

        _dbContext.Collections.Add(entity);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new CollectionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CollectionDto>> Update(Guid id, [FromBody] UpdateCollectionDto dto)
    {
        var userId = User.UserId();
        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == userId);

        if (collection == null)
            return NotFound();

        collection.Name = dto.Name;
        collection.Description = dto.Description;

        await _dbContext.SaveChangesAsync();

        return Ok(new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.UserId();
        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == userId);

        if (collection == null)
            return NotFound();

        _dbContext.Collections.Remove(collection);

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
