using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Features.Users;

public class CreateUserCommand : ICommand<RegisterUserResultDto>
{
    public RegisterUserDto Model { get; set; }
}

public class CreateUserHandler : BaseFeatureHandler, ICommandHandler<CreateUserCommand, RegisterUserResultDto>
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

    public async Task<RegisterUserResultDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {

        ValidateModel(command.Model);

        var user = await _userManager.FindByNameAsync(command.Model.Email);
        if (user != null)
        {
            throw new ConflictException("User already exists with this email address.");
        }

        user = new ApplicationUser { UserName = command.Model.Email, Email = command.Model.Email };
        var result = await _userManager.CreateAsync(user, command.Model.Password);
        if (result.Succeeded)
        {
            return new RegisterUserResultDto { Id = user.Id, Name = user.UserName };
        }

        var errormessage = string.Join(", ", result.Errors.Select(e => e.Description));

        throw new BadRequestException($"User creation failed: {errormessage}");
    }

}
