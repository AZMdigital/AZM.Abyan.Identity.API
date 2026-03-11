using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Client.Update;

public class UpdateClientCommand(string realmName, UpdateClientRequest updateClientRequest)
  : BaseEntity, IRequest<Result<bool>>
{
    public string RealmName { get; } = realmName;
    public UpdateClientRequest UpdateClientRequest { get; } = updateClientRequest;
}
