using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.API.Features.Authorization;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
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
            ModelState = ModelState
        });

        return result;
    }

}
