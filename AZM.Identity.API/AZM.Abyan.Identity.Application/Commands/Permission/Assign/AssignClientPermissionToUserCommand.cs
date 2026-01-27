using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.DTOs.Roles;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Permission.Assign
{
    public class AssignClientPermissionToUserCommand : IRequest<Result<bool>>
    {
        public AssignPermissionRequest AssignPermissionRequest { get; set; } = null!;
        public string Realm { get; set; } = string.Empty;
    }
}