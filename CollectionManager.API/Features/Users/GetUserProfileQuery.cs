using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Features.Users;

public class GetUserProfileQuery : IQuery<UserProfileDto>
{
    public Guid UserId { get; set; }
}

public class GetUserProfileQueryHandler : BaseFeatureHandler, IQueryHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserProfileQueryHandler(
        ILogger<GetUserProfileQueryHandler> logger,
        UserManager<ApplicationUser> userManager
        ) : base(logger)
    {
        _userManager = userManager;
    }

    public async Task<UserProfileDto> HandleAsync(GetUserProfileQuery query, CancellationToken cancellationToken)
    {


        var user = await _userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User does not exists with this User Id.");
        }

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Name = user.UserName,
            Email = user.Email,
            Avatar = user.Avatar.HasValue ? user.Avatar.Value : null,
            Created = user.Created,
            Updated = user.Updated
        };

        return profile;
    }

}
