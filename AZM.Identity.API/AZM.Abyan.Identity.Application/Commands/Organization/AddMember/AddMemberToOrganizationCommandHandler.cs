using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Organization.AddMember;

public class AddMemberToOrganizationCommandHandler(IOrganizationService organizationService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<AddMemberToOrganizationCommand, Result<bool>>
{
    private readonly IOrganizationService _organizationService = organizationService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<bool>> Handle(AddMemberToOrganizationCommand request, CancellationToken cancellationToken)
    {
        await _organizationService.AddMemberToOrganizationAsync(request.RealmName, request.OrganizationId, request.UserId, cancellationToken);
        return Result<bool>.Success(true, _localizer["OperationSuccess"]);
    }
}
