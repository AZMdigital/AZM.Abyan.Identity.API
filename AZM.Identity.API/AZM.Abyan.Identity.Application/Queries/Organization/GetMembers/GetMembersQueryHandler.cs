using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Users;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Organization.GetMembers;

public class GetMembersQueryHandler(IOrganizationService organizationService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetMembersQuery, Result<List<UserResponse>>>
{
    private readonly IOrganizationService _organizationService = organizationService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<List<UserResponse>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _organizationService.GetOrganizationMembersAsync(request.RealmName, request.OrganizationId, cancellationToken);
        return Result<List<UserResponse>>.Success(members);
    }
}
