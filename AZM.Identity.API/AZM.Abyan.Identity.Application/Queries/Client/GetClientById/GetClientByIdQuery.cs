using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClientById;

public class GetClientByKeycloakIdQuery(Guid id) : IRequest<Result<Guid>>
{
    public Guid Id { get; set; } = id;
    // Note: This query now uses the Keycloak ID which is the same as the entity Id
}
