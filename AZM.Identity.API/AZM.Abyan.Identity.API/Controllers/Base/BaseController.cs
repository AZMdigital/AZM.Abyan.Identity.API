using AZM.Abyan.Identity.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.API.Controllers.Base;

public abstract class BaseController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    private IStringLocalizer<SharedResource>? localizer;
    protected IStringLocalizer<SharedResource> Localizer =>
        localizer ??= HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
}