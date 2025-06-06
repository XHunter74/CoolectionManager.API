using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Services.Interfaces;
using File = xhunter74.CollectionManager.Data.Entity.File;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    private readonly CollectionsDbContext _dbContext;
    private readonly IStorageService _storageService;

    public FilesController(ILogger<FilesController> logger, CollectionsDbContext dbContext,
        IStorageService storageService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _storageService = storageService;
    }

    [HttpGet("{id:guid}", Name = nameof(DownloadFileAsync))]
    public async Task<IActionResult> DownloadFileAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();

        var file = await _dbContext.Files
            .FirstOrDefaultAsync(f => f.Id == id && f.Collection.OwnerId == userId, cancellationToken);

        if (file == null)
        {
            _logger.LogWarning("File with ID {Id} not found for user {UserId}", id, userId);
            return NotFound(new { Message = "File not found" });
        }

        var fileBytes = await _storageService.GetFileAsync(userId, file.Id, cancellationToken);

        if (fileBytes == null || fileBytes.Length == 0)
        {
            _logger.LogWarning("File content for ID {Id} not found in storage for user {UserId}", id, userId);
            return NotFound(new { Message = "File content not found" });
        }

        return File(fileBytes, "application/octet-stream", file.Name);
    }


    [HttpPost("/api/Collections/{collectionId:guid}/[controller]")]
    [ProducesResponseType(typeof(DynamicItemRecord), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UploadFileAsync(Guid collectionId, CancellationToken cancellationToken)
    {
        var userId = User.UserId();

        if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var file = Request.Form.Files[Constants.FileFormFieldName];
        if (file == null)
        {
            return BadRequest("Avatar file not found in form data.");
        }

        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.OwnerId == userId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", collectionId, userId);
            return NotFound(new { Message = "Collection not found" });
        }

        var newFile = new File
        {
            Id = Guid.NewGuid(),
            Name = file.FileName,
            CollectionId = collectionId,
        };

        _dbContext.Files.Add(newFile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] avatarBytes = memoryStream.ToArray();

        await _storageService.UploadFileAsync(userId, newFile.Id, avatarBytes, cancellationToken);

        return CreatedAtRoute(nameof(DownloadFileAsync), new { id = newFile.Id }, new { FileId = newFile.Id });
    }
}
