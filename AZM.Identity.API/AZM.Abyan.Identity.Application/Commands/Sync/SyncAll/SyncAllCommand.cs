using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Services;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Sync.SyncAll;

public class SyncAllCommand : IRequest<Result<SyncResult>>
{
}
