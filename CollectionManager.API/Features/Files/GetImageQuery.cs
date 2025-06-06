﻿using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.API.Features.Files;

public class GetImageQuery : IQuery<byte[]>
{
    public Guid UserId { get; set; }
}

public class GetImageQueryHandler : BaseFeatureHandler, IQueryHandler<GetImageQuery, byte[]>
{
    private readonly IStorageService _storageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetImageQueryHandler(
        ILogger<GetImageQueryHandler> logger,
        IStorageService storageService,
        UserManager<ApplicationUser> userManager
        ) : base(logger)
    {
        _storageService = storageService;
        _userManager = userManager;
    }

    public async Task<byte[]> HandleAsync(GetImageQuery query, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User does not exists with this User Id.");
        }
        if (!user.Avatar.HasValue)
            throw new NotFoundException("User does not have an avatar.");

        var avatar = await _storageService.GetFileAsync(user.Id, user.Avatar.Value, cancellationToken);
        return avatar!;
    }

}
