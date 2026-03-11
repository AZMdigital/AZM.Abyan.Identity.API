using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClientById;

public class GetClientByIdQueryHandler(IClientRepository repository, IStringLocalizer<SharedResource> localizer) : IRequestHandler<GetClientByKeycloakIdQuery, Result<Guid>>
{
    private readonly IClientRepository _repository = repository;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;

    public async Task<Result<Guid>> Handle(GetClientByKeycloakIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _repository.GetClientByKeycloakIdAsync(request.Id, cancellationToken);

        if (client == null)
            return Result<Guid>.NotFound(_localizer["FormNotFound"]);

        var clientResult = client.Adapt<Domain.Entities.Client>();

        return Result<Guid>.Success(clientResult.Id, _localizer["FormRetrievedSuccessfully"]);
    }
}