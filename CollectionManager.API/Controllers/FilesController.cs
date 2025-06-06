using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.Data.Mongo.Records;
using CQRSMediatr.Interfaces;
using xhunter74.CollectionManager.API.Features.Files;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    private readonly ICqrsMediatr _mediatr;

    public FilesController(ILogger<FilesController> logger, ICqrsMediatr mediatr)
    {
        _logger = logger;
        _mediatr = mediatr;
    }

    [HttpGet("{id:guid}", Name = nameof(DownloadFileAsync))]
    public async Task<IActionResult> DownloadFileAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new DownloadFileQuery
        {
            FileId = id,
            UserId = userId
        }, cancellationToken);
        return File(result.sources, "application/octet-stream", result.fileName);
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
            return BadRequest("File not found in form data.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        byte[] fileBytes = memoryStream.ToArray();

        var newFileId = await _mediatr.SendAsync(new UploadFileCommand
        {
            CollectionId = collectionId,
            UserId = userId,
            FileName = file.FileName,
            Sources = fileBytes
        }, cancellationToken);

        return CreatedAtRoute(nameof(DownloadFileAsync), new { id = newFileId }, new { FileId = newFileId });
    }
}
