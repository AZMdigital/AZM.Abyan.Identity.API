using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.Client.GetClientById
{
    public class GetClientByKeycloakIdQuery(Guid id) : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; } = id;
    }
}
