using AskFm.BLL.DTO;

namespace AskFm.BLL.Services;

public interface IUserModerationService
{
    Task<ServiceResult<bool>> BlockUserAsync(int userId, int blockedUserId);
    Task<ServiceResult<bool>> UnblockUserAsync(int userId, int blockedUserId);
    Task<ServiceResult<PagedResponseDto<UserBlockResponseDto>>> GetBlockedUsersAsync(int userId, int page, int pageSize);
    Task<ServiceResult<bool>> IsBlockedAsync(int userId, int targetUserId);
    Task<ServiceResult<bool>> MuteUserAsync(int userId, int mutedUserId);
    Task<ServiceResult<bool>> UnmuteUserAsync(int userId, int mutedUserId);
}
