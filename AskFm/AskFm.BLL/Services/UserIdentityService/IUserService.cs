using AskFm.BLL.DTO;
using AskFm.BLL.DTO.UserDTOs;
using AskFm.DAL.Models;

namespace AskFm.BLL.Services.UserIdentityService;

public interface IUserService
{
    Task<ServiceResult<bool>> UpdateUserAsync(int userId, UpdateUserDTO updatedUser);
    Task<ServiceResult<bool>> DeleteUserAsync(int userId);
    Task<ServiceResult<bool>> FollowUserAsync(int followerId, int targetUserId);
    Task<ServiceResult<bool>> UnfollowUserAsync(int followerId, int targetUserId);
    Task<ServiceResult<bool>> UpdateLastSeenAsync(int userId);
    Task<ServiceResult<ReadUserDTO>> GetUserByIdAsync(int userId);
    Task<ServiceResult<ApplicationUser>> GetCurrentUserAsync();
    Task<ServiceResult<bool>> UpdatePassword(int userId, UpdatePasswordDTO updatePasswordDto);
    Task<ServiceResult<bool>> ResetEmail(int userId, string updatedEmail);
    Task<ServiceResult<bool>> ConfirmEmailChangeAsync(int userId, string updatedEmail, string token);

    Task<ServiceResult<PagedResponseDto<FollowUserDto>>> GetFollowersAsync(int userId, int page, int pageSize);
    Task<ServiceResult<PagedResponseDto<FollowUserDto>>> GetFollowingAsync(int userId, int page, int pageSize);
    Task<ServiceResult<bool>> IsFollowingAsync(int followerId, int targetUserId);


    /*
GET Users only for now

 confirm email
 Helper Function: getCurrentUserId
*/
}
