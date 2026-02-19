using AZM.Abyan.Identity.Application.DTOs.Responses;
using MediatR;

namespace AZM.Abyan.Identity.Application.Queries.License.GetAllLicenses;

public class GetAllLicensesQuery : IRequest<Result<List<DTOs.Licenses.LicenseResponse>>>
{
}
