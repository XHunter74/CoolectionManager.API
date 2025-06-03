using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.Users;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private const string AvatarFormFieldName = "avatar";
    private readonly ILogger<UsersController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CollectionsDbContext _collectionsDbContext;
    private readonly ICqrsMediatr _mediatr;

    public UsersController(
        ILogger<UsersController> logger,
        UserManager<ApplicationUser> userManager,
        CollectionsDbContext collectionsDbContext,
        ICqrsMediatr mediatr)
    {
        _logger = logger;
        _userManager = userManager;
        _collectionsDbContext = collectionsDbContext;
        _mediatr = mediatr;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
    {
        var result = await _mediatr.SendAsync(new CreateUserCommand
        {
            Model = model,
        });

        return CreatedAtRoute(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}", Name = nameof(GetUserById))]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        return Ok("Not implemented");
    }


    [HttpGet]
    public async Task<IActionResult> GetUserAsync()
    {
        return Ok("Not implemented");
    }


    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatarAsync()
    {
        if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var file = Request.Form.Files[AvatarFormFieldName];
        if (file == null)
        {
            return BadRequest("Avatar file not found in form data.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] avatarBytes = memoryStream.ToArray();

        _ = await _mediatr.SendAsync(new UploadAvatarCommand
        {
            UserId = User.UserId(),
            Sources = avatarBytes
        });

        return Created();
    }
}
