using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Role.GetClientRoles;

public class GetClientRolesQuery(string realmName, string clientId) : IRequest<Result<List<ClientRoleResponse>>>
{
    public string RealmName { get; set; } = realmName;
    public string ClientId { get; set; } = clientId;
}
