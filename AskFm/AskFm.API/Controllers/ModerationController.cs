using System.Security.Claims;
using AskFm.BLL.DTO;
using AskFm.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AskFm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ModerationController : ControllerBase
{
    private readonly IUserModerationService _moderationService;

    public ModerationController(IUserModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException("Invalid user token");
        return userId;
    }

    [HttpPost("block/{userId}")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _moderationService.BlockUserAsync(currentUserId, userId);
        if (!result.success) return BadRequest(result.Errors);
        return Ok("User blocked successfully");
    }

    [HttpDelete("block/{userId}")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _moderationService.UnblockUserAsync(currentUserId, userId);
        if (!result.success) return BadRequest(result.Errors);
        return Ok("User unblocked successfully");
    }

    [HttpGet("blocked")]
    public async Task<IActionResult> GetBlockedUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _moderationService.GetBlockedUsersAsync(currentUserId, page, pageSize);
        if (!result.success) return BadRequest(result.Errors);
        return Ok(result.Data);
    }

    [HttpPost("mute/{userId}")]
    public async Task<IActionResult> MuteUser(int userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _moderationService.MuteUserAsync(currentUserId, userId);
        if (!result.success) return BadRequest(result.Errors);
        return Ok("User muted successfully");
    }

    [HttpDelete("mute/{userId}")]
    public async Task<IActionResult> UnmuteUser(int userId)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _moderationService.UnmuteUserAsync(currentUserId, userId);
        if (!result.success) return BadRequest(result.Errors);
        return Ok("User unmuted successfully");
    }
}
