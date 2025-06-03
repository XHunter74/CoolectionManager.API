using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.API.Models;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CollectionsDbContext _collectionsDbContext;

    public UsersController(
        ILogger<UsersController> logger,
        UserManager<ApplicationUser> userManager,
        CollectionsDbContext collectionsDbContext)
    {
        _logger = logger;
        _userManager = userManager;
        _collectionsDbContext = collectionsDbContext;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            AddErrors(result);
        }

        return BadRequest(ModelState);
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

}
