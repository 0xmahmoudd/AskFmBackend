using AskFm.BLL.DTO;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;

namespace AskFm.BLL.Services;

public class UserModerationService : IUserModerationService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserModerationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<bool>> BlockUserAsync(int userId, int blockedUserId)
    {
        if (userId == blockedUserId)
            return await ServiceResult<bool>.Failure(new List<string> { "You cannot block yourself" });

        var targetUser = await _unitOfWork.Users.GetByIdAsync(blockedUserId);
        if (targetUser == null)
            return await ServiceResult<bool>.Failure(new List<string> { "Target user not found" });

        var existingBlock = await _unitOfWork.UserBlocks.FindAsync(b => b.BlockerId == userId && b.BlockedId == blockedUserId);
        if (existingBlock != null)
            return await ServiceResult<bool>.Failure(new List<string> { "User is already blocked" });

        var block = new UserBlock
        {
            BlockerId = userId,
            BlockedId = blockedUserId
        };

        await _unitOfWork.UserBlocks.AddAsync(block);
        
        // Remove follow relationship if exists
        var follow1 = await _unitOfWork.Follows.FindAsync(f => f.FollowerId == userId && f.FollowedId == blockedUserId);
        if (follow1 != null) await _unitOfWork.Follows.RemoveAsync(follow1);
        
        var follow2 = await _unitOfWork.Follows.FindAsync(f => f.FollowerId == blockedUserId && f.FollowedId == userId);
        if (follow2 != null) await _unitOfWork.Follows.RemoveAsync(follow2);

        await _unitOfWork.SaveAsync();
        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> UnblockUserAsync(int userId, int blockedUserId)
    {
        var block = await _unitOfWork.UserBlocks.FindAsync(b => b.BlockerId == userId && b.BlockedId == blockedUserId);
        if (block == null)
            return await ServiceResult<bool>.Failure(new List<string> { "User is not blocked" });

        await _unitOfWork.UserBlocks.RemoveAsync(block);
        await _unitOfWork.SaveAsync();
        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<PagedResponseDto<UserBlockResponseDto>>> GetBlockedUsersAsync(int userId, int page, int pageSize)
    {
        int skipCount = (page - 1) * pageSize;
        
        var blocks = await _unitOfWork.UserBlocks.GetPagedAsync(
            skipCount,
            pageSize + 1,
            b => b.CreatedAt,
            false,
            b => b.BlockerId == userId,
            new[] { "Blocked" }
        );

        bool hasMore = blocks.Count > pageSize;
        var trimmed = blocks.Take(pageSize).ToList();

        var dtos = trimmed.Select(b => new UserBlockResponseDto
        {
            Id = b.Id,
            BlockedUserId = b.BlockedId,
            BlockedUserName = b.Blocked?.Name,
            CreatedAt = b.CreatedAt
        }).ToList();

        var response = new PagedResponseDto<UserBlockResponseDto>
        {
            Items = dtos,
            PageNumber = page,
            PageSize = pageSize,
            HasMore = hasMore
        };

        return await ServiceResult<PagedResponseDto<UserBlockResponseDto>>.Success(response);
    }

    public async Task<ServiceResult<bool>> IsBlockedAsync(int userId, int targetUserId)
    {
        // Check both directions
        var block = await _unitOfWork.UserBlocks.FindAsync(
            b => (b.BlockerId == userId && b.BlockedId == targetUserId) || 
                 (b.BlockerId == targetUserId && b.BlockedId == userId)
        );
        
        return await ServiceResult<bool>.Success(block != null);
    }

    public async Task<ServiceResult<bool>> MuteUserAsync(int userId, int mutedUserId)
    {
        if (userId == mutedUserId)
            return await ServiceResult<bool>.Failure(new List<string> { "You cannot mute yourself" });

        var targetUser = await _unitOfWork.Users.GetByIdAsync(mutedUserId);
        if (targetUser == null)
            return await ServiceResult<bool>.Failure(new List<string> { "Target user not found" });

        var existingMute = await _unitOfWork.UserMutes.FindAsync(m => m.MuterId == userId && m.MutedId == mutedUserId);
        if (existingMute != null)
            return await ServiceResult<bool>.Failure(new List<string> { "User is already muted" });

        var mute = new UserMute
        {
            MuterId = userId,
            MutedId = mutedUserId
        };

        await _unitOfWork.UserMutes.AddAsync(mute);
        await _unitOfWork.SaveAsync();
        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> UnmuteUserAsync(int userId, int mutedUserId)
    {
        var mute = await _unitOfWork.UserMutes.FindAsync(m => m.MuterId == userId && m.MutedId == mutedUserId);
        if (mute == null)
            return await ServiceResult<bool>.Failure(new List<string> { "User is not muted" });

        await _unitOfWork.UserMutes.RemoveAsync(mute);
        await _unitOfWork.SaveAsync();
        return await ServiceResult<bool>.Success(true);
    }
}
