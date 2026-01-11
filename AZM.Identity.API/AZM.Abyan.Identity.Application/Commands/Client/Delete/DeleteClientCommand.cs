using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Client.Delete
{
    public class DeleteClientCommand(Guid clientId) : IRequest<Result<bool>>
    {
        public Guid ClientId { get; set; } = clientId;
    }

}
