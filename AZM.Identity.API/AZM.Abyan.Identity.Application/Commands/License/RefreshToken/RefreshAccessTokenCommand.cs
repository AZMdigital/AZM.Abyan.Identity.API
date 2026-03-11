using AZM.Abyan.Identity.Application.DTOs.Auth;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.License.RefreshToken;

public record RefreshAccessTokenCommand(string RefreshToken) : IRequest<RefreshAccessTokenResponse>;