using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Authorization;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.Files;
using xhunter74.CollectionManager.Data.Mongo.Records;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly ILogger<FilesController> _logger;
    private readonly ICqrsMediatr _mediatr;

    public ImagesController(ILogger<FilesController> logger, ICqrsMediatr mediatr)
    {
        _logger = logger;
        _mediatr = mediatr;
    }

    [HttpGet("/api/Collections/{collectionId:guid}/[controller]/{id:guid}", Name = nameof(DownloadImageAsync))]
    [UrlTokenAuthorizationFilter]
    public async Task<IActionResult> DownloadImageAsync(Guid collectionId, Guid id, CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        var result = await _mediatr.QueryAsync(new DownloadCollectionImageQuery
        {
            ImageId = id,
            UserId = userId,
            CollectionId = collectionId
        }, cancellationToken);
        return File(result.sources, "application/octet-stream", result.fileName);
    }

    [HttpPost("/api/Collections/{collectionId:guid}/[controller]")]
    [ProducesResponseType(typeof(CollectionItemRecord), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [Authorize]
    public async Task<IActionResult> UploadImageAsync(Guid collectionId, CancellationToken cancellationToken)
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

        var newFileId = await _mediatr.SendAsync(new UploadCollectionImageCommand
        {
            CollectionId = collectionId,
            UserId = userId,
            FileName = file.FileName,
            Sources = fileBytes
        }, cancellationToken);

        var url = Url.Action(nameof(DownloadImageAsync), "Images", new { id = newFileId }, Request.Scheme, Request.Host.ToString());
        Response.Headers.Location = url;
        return StatusCode(201, new { FileId = newFileId });
    }
}
