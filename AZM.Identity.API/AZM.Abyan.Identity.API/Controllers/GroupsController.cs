// Commented out - will be needed later
/*
using AZM.Abyan.Identity.Application.DTOs.Groups;
using AZM.Abyan.Identity.Application.DTOs.Users;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupResponse>> GetGroupById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var group = await _groupService.GetGroupByIdAsync(id, cancellationToken);
            if (group == null)
                return NotFound(new { message = $"Group with id {id} not found" });

            return Ok(group);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateGroup([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.CreateGroupAsync(request, cancellationToken);
            return Ok(new { message = $"Group '{request.Name}' created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateGroup(string id, [FromBody] UpdateGroupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.UpdateGroupAsync(id, request, cancellationToken);
            return Ok(new { message = $"Group {id} updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGroup(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.DeleteGroupAsync(id, cancellationToken);
            return Ok(new { message = $"Group {id} deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<UserResponse>>> GetGroupMembers(string id, CancellationToken cancellationToken)
    {
        try
        {
            var members = await _groupService.GetGroupMembersAsync(id, cancellationToken);
            return Ok(members);
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

    [HttpDelete("{id}/members/{userId}")]
    public async Task<ActionResult> RemoveUserFromGroup(string id, string userId, CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.RemoveUserFromGroupAsync(userId, id, cancellationToken);
            return Ok(new { message = $"User {userId} removed from group {id} successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
*/

