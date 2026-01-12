using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Clients;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Domain.Entities.Base;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Client.Create
{
    public class CreateClientCommand : BaseEntity, IRequest<Result<Guid>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public string RealmName { get; set; } = string.Empty;
    }
}
