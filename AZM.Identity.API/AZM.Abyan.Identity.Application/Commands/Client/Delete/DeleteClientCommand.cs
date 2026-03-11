using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Client.Delete;

public class DeleteClientCommand(string realmName, Guid clientId) : IRequest<Result<bool>>
{
    public string RealmName { get; set; } = realmName;
    public Guid ClientId { get; set; } = clientId;
}
