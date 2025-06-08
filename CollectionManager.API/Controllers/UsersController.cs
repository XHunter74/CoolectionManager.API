using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Extensions;
using xhunter74.CollectionManager.API.Features.Files;
using xhunter74.CollectionManager.API.Features.Users;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.API.Permissions.PolicyProvider;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Controllers;

/// <summary>
/// Controller for user management operations such as registration, profile retrieval, avatar upload, and password reset.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CollectionsDbContext _collectionsDbContext;
    private readonly ICqrsMediatr _mediatr;

    /// <summary>
    /// Constructor for UsersController.
    /// </summary>
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

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="model">The registration data.</param>
    /// <returns>The created user result.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Invalid registration data.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterUserResultDto), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserDto model)
    {
        var result = await _mediatr.SendAsync(new CreateUserCommand
        {
            Model = model,
        });

        return CreatedAtRoute(nameof(GetUserByIdAsync), new { id = result.Id }, result);
    }

    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <returns>The user profile.</returns>
    /// <response code="200">Returns the user profile.</response>
    /// <response code="400">User not found or invalid id.</response>
    [HttpGet("{id:guid}", Name = nameof(GetUserByIdAsync))]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [PermissionAuthorize(Permissions.Permissions.ViewUser)]
    public async Task<IActionResult> GetUserByIdAsync(Guid id)
    {
        return BadRequest("Not implemented");
    }

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    /// <returns>The user profile.</returns>
    /// <response code="200">Returns the user profile.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    public async Task<IActionResult> GetUserAsync()
    {
        var result = await _mediatr.QueryAsync(new GetUserProfileQuery
        {
            UserId = User.UserId()
        });

        return Ok(result);
    }

    /// <summary>
    /// Uploads a new avatar for the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Expects a multipart/form-data request with the avatar file in the 'avatar' field.
    /// </remarks>
    /// <returns>Status of the upload operation.</returns>
    /// <response code="201">Avatar uploaded successfully.</response>
    /// <response code="400">No file uploaded or avatar file not found in form data.</response>
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(201)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> UploadAvatarAsync()
    {
        if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var file = Request.Form.Files[Constants.AvatarFormFieldName];
        if (file == null)
        {
            return BadRequest("Avatar file not found in form data.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] avatarBytes = memoryStream.ToArray();

        _ = await _mediatr.SendAsync(new UploadUserImageCommand
        {
            UserId = User.UserId(),
            Sources = avatarBytes
        });

        return Created();
    }

    /// <summary>
    /// Gets the avatar of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns the avatar image file for the authenticated user.
    /// </remarks>
    /// <returns>The avatar file as a binary stream.</returns>
    /// <response code="200">Returns the avatar file.</response>
    /// <response code="401">Unauthorized. The user is not authenticated.</response>
    /// <response code="404">Avatar not found for the user.</response>
    [HttpGet("avatar")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserAvatarAsync()
    {
        var userId = User.UserId();
        var query = new DownloadUserImageQuery
        {
            UserId = userId
        };
        var avatar = await _mediatr.QueryAsync(query);
        return File(avatar, Constants.AvatarContentType, Constants.AvatarFileName);
    }

    /// <summary>
    /// Generates a password reset link and sends it to the user's email address.
    /// </summary>
    /// <param name="userName">The username (email) of the user requesting password reset.</param>
    /// <returns>Status of the operation.</returns>
    /// <response code="200">Reset link generated and sent.</response>
    /// <response code="400">Invalid username or user not found.</response>
    [HttpPost("generate-reset-password-link")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> GenerateResetPasswordLinkAsync([FromQuery] string userName)
    {
        var command = new GenerateResetPasswordLinkCommand
        {
            UserName = userName
        };
        _ = await _mediatr.SendAsync(command);
        return Ok();
    }

    /// <summary>
    /// Resets the user's password using a reset token.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="token">The password reset token.</param>
    /// <returns>Status of the password reset operation.</returns>
    /// <remarks>
    /// The new password should be provided in the form field 'newPassword'.
    /// </remarks>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid user ID, token, or new password.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(string), 400)]
    public async Task<IActionResult> ResetPasswordAsync([FromQuery] Guid userId, [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token) || userId == Guid.Empty)
        {
            return BadRequest("Invalid user ID or token.");
        }

        string newPassword = null;
        try
        {
            newPassword = Request.Form["newPassword"].ToString();
        }
        catch
        {
            //Do nothing
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return BadRequest("New password cannot be empty.");
        }

        var command = new ResetPasswordCommand
        {
            UserId = userId,
            Token = token,
            NewPassword = newPassword
        };

        _ = await _mediatr.SendAsync(command);

        return Ok();
    }
}
