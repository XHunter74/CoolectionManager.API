using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Features.Authorization;

public class CreateUserCommand : ICommand<IActionResult>
{
    public RegisterUserDto Model { get; set; }
    public ModelStateDictionary ModelState { get; set; }
}

public class CreateUserHandler : BaseFeatureHandler, ICommandHandler<CreateUserCommand, IActionResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CollectionsDbContext _dbContext;

    public CreateUserHandler(
        ILogger<CreateUserHandler> logger,
        UserManager<ApplicationUser> userManager,
        CollectionsDbContext dbContext
        ) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (command.ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(command.Model.Email);
            if (user != null)
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            user = new ApplicationUser { UserName = command.Model.Email, Email = command.Model.Email };
            var result = await _userManager.CreateAsync(user, command.Model.Password);
            if (result.Succeeded)
            {
                return new OkResult();
            }
            command.ModelState = AddErrors(command.ModelState, result);
        }

        return new BadRequestObjectResult(command.ModelState);
    }

    private static ModelStateDictionary AddErrors(ModelStateDictionary modelState, IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(string.Empty, error.Description);
        }
        return modelState;
    }
}
