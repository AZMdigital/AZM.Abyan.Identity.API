using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Commands.Client.Delete
{
    public class DeleteClientCommandHandler(IClientRepository repository, IStringLocalizer<SharedResource> localizer) : IRequestHandler<DeleteClientCommand, Result<bool>>
    {
        private readonly IClientRepository _repository = repository;
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;

        public async Task<Result<bool>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _repository.DeleteClientAsync(request.ClientId, cancellationToken);

            if (!deleted)
            {
                return Result<bool>.NotFound(_localizer["ClientNotFound"]);
            }

            return Result<bool>.Deleted(true, _localizer["ClientDeletedSuccessfully"]);
        }
    }
}


