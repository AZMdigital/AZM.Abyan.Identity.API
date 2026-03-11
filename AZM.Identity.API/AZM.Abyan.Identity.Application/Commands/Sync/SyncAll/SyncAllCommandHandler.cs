using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Sync.SyncAll;

public class SyncAllCommandHandler(ISyncOrchestratorService syncOrchestratorService, IStringLocalizer<SharedResource> localizer) : IRequestHandler<SyncAllCommand, Result<SyncResult>>
{
    private readonly ISyncOrchestratorService _syncOrchestratorService = syncOrchestratorService;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<SyncResult>> Handle(SyncAllCommand request, CancellationToken cancellationToken)
    {
        var result = await _syncOrchestratorService.SyncAllAsync(cancellationToken);
        if (result.Success)
            return Result<SyncResult>.Success(result, _localizer["SyncSuccess"]);
        
        return Result<SyncResult>.Failure(_localizer["SyncFailed"], result);
    }
}
