using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.Collections;
using xhunter74.CollectionManager.API.Models;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICqrsMediatr _mediatr;

    public CollectionsController(
        ILogger<CollectionsController> logger,
        ICqrsMediatr mediatr)
    {
        _logger = logger;
        _mediatr = mediatr;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new GetCollectionsQuery
        {
            UserId = userId,
        }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
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

    [HttpPost]
    public async Task<ActionResult<CollectionDto>> CreateAsync([FromBody] CreateCollectionDto model, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var newCollection = await _mediatr.SendAsync(new CreateCollectionCommand
        {
            UserId = userId,
            Model = model
        }, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = newCollection.Id }, newCollection);
    }

    [HttpPut("{id:guid}")]
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

    [HttpDelete("{id:guid}")]
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
