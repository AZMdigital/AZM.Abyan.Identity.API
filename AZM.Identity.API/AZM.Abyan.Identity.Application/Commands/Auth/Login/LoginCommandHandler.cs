using AZM.Abyan.Identity.Application.DTOs.Auth;
using AZM.Abyan.Identity.Application.DTOs.Responses;
using AZM.Abyan.Identity.Application.Services;
using MediatR;

namespace AZM.Abyan.Identity.Application.Commands.Auth.Login;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthService _authService = authService;

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest { Username = request.Username, Password = request.Password };
        var response = await _authService.LoginAsync(loginRequest, cancellationToken);
        return Result<LoginResponse>.Success(response);
    }
}
