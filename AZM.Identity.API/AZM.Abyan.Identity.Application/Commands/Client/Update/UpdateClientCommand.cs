using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Client.Update
{
    public class UpdateClientCommand(UpdateClientRequest updateClientRequest)
      : BaseEntity, IRequest<Result<bool>>
    {
        public UpdateClientRequest UpdateClientRequest { get; } = updateClientRequest;
    }
}
