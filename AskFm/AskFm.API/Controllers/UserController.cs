using AskFm.BLL.DTO.UserDTOs;
using AskFm.BLL.Services.UserIdentityService;
using AskFm.DAL;
using AskFm.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace AskFm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UserController : ControllerBase
{
    private static readonly string[] AllowedAvatarExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxAvatarSizeBytes = 5 * 1024 * 1024; // 5 MB

    private IUnitOfWork _unitOfWork;
    private IAuthService  _authService;
    public IUserService _userService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UserController(IUnitOfWork unitOfWork, IAuthService authService, IUserService userService, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _userService = userService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    [Route("profile")]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        var result = await _userService.GetCurrentUserAsync();

        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        var userId = result.Data.Id;
        var followersCount = await _unitOfWork.Follows.CountAsync(f => f.FollowedId == userId && !f.IsDeleted && f.IsActive);
        var followingCount = await _unitOfWork.Follows.CountAsync(f => f.FollowerId == userId && !f.IsDeleted && f.IsActive);

        ReadUserDTO readUserDTO = new ReadUserDTO()
        {
            Id = userId,
            Name = result.Data.Name,
            Email = result.Data.Email,
            AvatarPath = result.Data.AvatarPath,
            Bio = result.Data.Bio,
            followerCount = followersCount,
            followingCount = followingCount,
            LastSeen = result.Data.LastSeen,
        };
        return Ok(readUserDTO);
    }

    [HttpGet]
    [Route("profile/{userId}")]
    public async Task<IActionResult> GetUserAsync(int userId)
    {
        var result = await _userService.GetUserByIdAsync(userId);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpPost]
    [Route("profile/update/{userId}")]
    public async Task<IActionResult> UpdateUserAsync(int userId, UpdateUserDTO updatedUser)
    {
        if (!await _checkCurrentUser(userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Cannot Update this user");
        }
        var result = await _userService.UpdateUserAsync(userId, updatedUser);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        var userRead = await _userService.GetUserByIdAsync(userId);
        return Ok(userRead);
    }

    [HttpDelete]
    [Route("profile/{userId}")]
    public async Task<IActionResult> DeleteUserAsync(int userId)
    {
        if (!await _checkCurrentUser(userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Cannot Remove this user");
        }
        var result = await _userService.DeleteUserAsync(userId);

        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok();
    }

    [HttpPost]
    [Route("profile/{followerId}/follow/{targetUserId}")]
    public async Task<IActionResult> FollowUserAsync(int followerId, int targetUserId)
    {
        if (await _checkCurrentUser(targetUserId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Cannot Follow this user");
        }
        if (!await _checkCurrentUser(followerId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "User can't perform this follow");
        }

        var result = await _userService.FollowUserAsync(followerId, targetUserId);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok();
    }

    [HttpPost]
    [Route("profile/{followerId}/unfollow/{targetUserId}")]
    public async Task<IActionResult> UnFollowUserAsync(int followerId, int targetUserId)
    {
        if (await _checkCurrentUser(targetUserId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Cannot Unfollow the current user");
        }
        if (!await _checkCurrentUser(followerId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "User can't perform this unfollow");
        }

        var result = await _userService.UnfollowUserAsync(followerId, targetUserId);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok();
    }

    [HttpPost]
    [Route("profile/{userId}/avatar")]
    public async Task<IActionResult> UploadAvatarAsync(int userId, IFormFile file)
    {
        if (!await _checkCurrentUser(userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Cannot Update this user");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (file.Length > MaxAvatarSizeBytes)
        {
            return BadRequest("File is too large. Max size is 5 MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedAvatarExtensions.Contains(extension))
        {
            return BadRequest("Unsupported file type");
        }

        var avatarsFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "avatars");
        Directory.CreateDirectory(avatarsFolder);

        var fileName = string.Concat("user-", userId.ToString(), "-", Guid.NewGuid().ToString("N"), extension);
        var filePath = Path.Combine(avatarsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = string.Concat("/avatars/", fileName);
        var result = await _userService.UpdateAvatarAsync(userId, relativePath);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { avatarPath = relativePath });
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> SearchUsersAsync([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _userService.SearchUsersAsync(q, page, pageSize);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet]
    [Route("profile/{userId}/followers")]
    public async Task<IActionResult> GetFollowersAsync(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _userService.GetFollowersAsync(userId, page, pageSize);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet]
    [Route("profile/{userId}/following")]
    public async Task<IActionResult> GetFollowingAsync(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _userService.GetFollowingAsync(userId, page, pageSize);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Data);
    }

    [HttpGet]
    [Route("profile/{targetUserId}/follow-status")]
    public async Task<IActionResult> GetFollowStatusAsync(int targetUserId)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (!currentUser.success)
        {
            return BadRequest(currentUser.Errors);
        }

        var result = await _userService.IsFollowingAsync(currentUser.Data.Id, targetUserId);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        return Ok(new { isFollowing = result.Data });
    }

    [HttpPost]
    [Route("profile/update/pass/{userId}")]
    public async Task<IActionResult> UpdatePassword(int userId, UpdatePasswordDTO udpatePasswordDto)
    {
        if (await _checkCurrentUser(userId))
        {

            return StatusCode(StatusCodes.Status403Forbidden, "Invalid Operation");
        }
        var result = await _userService.UpdatePassword(userId, udpatePasswordDto);

        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }


    [HttpPost]
    [Route("profile/update/email/{userId}")]
    public async Task<IActionResult> RequestEmailChangeAsync(int userId, UpdateEmailDto updateEmailDto)
    {
        if (!await _checkCurrentUser(userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Cannot Update this user");
        }

        var result = await _userService.ResetEmail(userId, updateEmailDto.NewEmail);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Check Your New Email To Confirm The Change");
    }

    [HttpPost]
    [Route("profile/update/email/{userId}/confirm")]
    public async Task<IActionResult> ConfirmEmailChangeAsync(int userId, ConfirmEmailChangeDto confirmEmailChangeDto)
    {
        if (!await _checkCurrentUser(userId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Cannot Update this user");
        }

        var result = await _userService.ConfirmEmailChangeAsync(userId, confirmEmailChangeDto.NewEmail, confirmEmailChangeDto.Token);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Email Updated Successfully");
    }

    //-------------------------------------------------------------------
    // Helper functions
    private async Task<bool> _checkCurrentUser(int userId)
    {
        var current_user = await _userService.GetCurrentUserAsync();
        return current_user.Data.Id == userId;
    }


    /* TODO
    check user not deleted in login
    */

}
