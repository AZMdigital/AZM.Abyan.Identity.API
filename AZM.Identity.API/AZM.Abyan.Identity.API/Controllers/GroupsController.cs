using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AZM.Abyan.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GroupResponse>>> GetGroups(CancellationToken cancellationToken)
    {
        try
        {
            var groups = await _groupService.GetGroupsAsync(cancellationToken);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("add-user")]
    public async Task<ActionResult> AddUserToGroup([FromBody] AddUserToGroupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.AddUserToGroupAsync(request, cancellationToken);
            return Ok(new { message = $"User {request.UserId} added to group {request.GroupId} successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

