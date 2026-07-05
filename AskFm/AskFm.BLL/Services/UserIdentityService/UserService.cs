using System.Security.Claims;
using AskFm.BLL.DTO;
using AskFm.BLL.DTO.UserDTOs;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AskFm.BLL.Services.UserIdentityService;

public class UserService : IUserService
{
    private IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;


    public UserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,  IHttpContextAccessor httpContextAccessor, IEmailSender emailSender, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _emailSender = emailSender;
        _configuration = configuration;
    }


    public async Task<ServiceResult<bool>> UpdateUserAsync(int userId, UpdateUserDTO updatedUser)
    {
        var res = await CheckNullObjectAsync<bool,UpdateUserDTO>(updatedUser);
        if (!res.success) return res;

        var AppUserToUpdate = await _unitOfWork.Users.GetByIdAsync(userId);
        if (AppUserToUpdate == null)
        {
            return await ServiceResult<bool>.Failure(new List<string> { "User not found" });
        }
        AppUserToUpdate.Name =  updatedUser.Name;
        AppUserToUpdate.Bio =  updatedUser.Bio;
        AppUserToUpdate.AvatarPath = updatedUser.AvatarPath;

        await _unitOfWork.Users.UpdateAsync(AppUserToUpdate);
        await _unitOfWork.SaveAsync();
        return await ServiceResult<bool>.Success();
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(int userId)
    {
        var appUser = await _unitOfWork.Users.GetByIdAsync(userId);
        var res = await CheckNullObjectAsync<bool,ApplicationUser>(appUser);
        if (!res.success) return res;

        await _unitOfWork.Users.RemoveAsync(appUser);
        await _unitOfWork.SaveAsync();
        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> FollowUserAsync(int followerId, int targetUserId)
    {

        if (followerId == targetUserId)
        {
            return await ServiceResult<bool>.Failure(new List<string> { "Invalid user" });
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var userFollower = await _unitOfWork.Users.GetByIdAsync(followerId);
            var userFollowerNullRes = await CheckNullObjectAsync<bool,ApplicationUser>(userFollower);
            if (!userFollowerNullRes.success) return userFollowerNullRes;

            var targetUser = await _unitOfWork.Users.GetByIdAsync(targetUserId);
            var targetUserNullRes = await CheckNullObjectAsync<bool,ApplicationUser>(targetUser);
            if (!targetUserNullRes.success) return targetUserNullRes;

            var followExist = await _unitOfWork.Follows.GetAll()
                .FirstOrDefaultAsync(f => f.FollowedId == targetUserId
                                     && f.FollowerId == followerId);

            if (followExist == null)
            {
                Follow follow = new Follow()
                {
                    FollowerId = followerId,
                    FollowedId = targetUserId,

                };

                userFollower.FollowingCount++;
                targetUser.FollowersCount++;
                await _unitOfWork.Follows.AddAsync(follow);

            }
            else
            {
                followExist.IsActive = true;
                if (followExist.IsDeleted)
                {
                    userFollower.FollowingCount++;
                    targetUser.FollowersCount++;
                    followExist.IsDeleted = false;
                }

                await _unitOfWork.Follows.UpdateAsync(followExist);
            }

            await _unitOfWork.Users.UpdateAsync(userFollower);
            await _unitOfWork.Users.UpdateAsync(targetUser);
            await _unitOfWork.SaveAsync();
            await transaction.CommitAsync();
            return await ServiceResult<bool>.Success(true);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return await ServiceResult<bool>.Failure(new List<string>() { "Invalid Follow Operation" });
        }
    }

    public async Task<ServiceResult<bool>> UnfollowUserAsync(int followerId, int targetUserId)
    {
        if (followerId == targetUserId)
        {
            return await ServiceResult<bool>.Failure(new List<string> { "Invalid user" });
        }
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var userFollower = await _unitOfWork.Users.GetByIdAsync(followerId);
            var userFollowerNullRes = await CheckNullObjectAsync<bool,ApplicationUser>(userFollower);
            if (!userFollowerNullRes.success) return userFollowerNullRes;

            var targetUser = await _unitOfWork.Users.GetByIdAsync(targetUserId);
            var targetUserNullRes = await CheckNullObjectAsync<bool,ApplicationUser>(targetUser);
            if (!targetUserNullRes.success) return targetUserNullRes;


            var followExist = await _unitOfWork.Follows.GetAll()
                .FirstOrDefaultAsync(f => f.FollowedId == targetUserId
                                          && f.FollowerId == followerId && !f.IsDeleted);

            if (followExist != null)
            {
                followExist.IsDeleted = true;
                followExist.IsDeleted = true;
                followExist.IsActive = false;
                if (userFollower.FollowingCount > 0) userFollower.FollowingCount--;
                if (targetUser.FollowersCount > 0)   targetUser.FollowersCount--;
                await _unitOfWork.Follows.UpdateAsync(followExist);
                await _unitOfWork.Users.UpdateAsync(userFollower);
                await _unitOfWork.Users.UpdateAsync(targetUser);
                await _unitOfWork.SaveAsync();
            }

            transaction.Commit();

            return await ServiceResult<bool>.Success(true);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return await ServiceResult<bool>.Failure(new List<string>() { "Invalid unfollow operation" });
        }
    }

    public async Task<ServiceResult<bool>> UpdateLastSeenAsync(int userId)
    {
        var appUser =await _unitOfWork.Users.GetByIdAsync(userId);
        var res = await CheckNullObjectAsync<bool,ApplicationUser>(appUser);
        if (!res.success) return res;
        await using var  transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            appUser.LastSeen = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(appUser);
            await _unitOfWork.SaveAsync();
            await transaction.CommitAsync();
            return await ServiceResult<bool>.Success(true);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return await ServiceResult<bool>.Failure(new List<string>() { "Invalid update operation" });
        }

    }

    public async Task<ServiceResult<ReadUserDTO>> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var res = await CheckNullObjectAsync<ReadUserDTO, ApplicationUser>(user);
        if (!res.success) return res;

        return await ServiceResult<ReadUserDTO>.Success(new ReadUserDTO()
        {
            Name = user.Name,
            Email = user.Email,
            LastSeen = user.LastSeen,
            Bio = user.Bio,
            AvatarPath = user.AvatarPath,
            followerCount = user.FollowersCount
        });
    }

    public async Task<ServiceResult<ApplicationUser>> GetCurrentUserAsync()
    {
        string email = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value;
        if (string.IsNullOrEmpty(email))
        {
            var errors = new List<string>()
            {
                "Can't Access Current user"
            };
            return await ServiceResult<ApplicationUser>.Failure(errors);
        }
        var currentAppUser = await _userManager.FindByEmailAsync(email);

        return await ServiceResult<ApplicationUser>.Success(currentAppUser);
    }

    public async Task<ServiceResult<bool>> UpdatePassword(int userId, UpdatePasswordDTO updatePasswordDto)
    {
        var nullPassRes = await CheckNullObjectAsync<bool, UpdatePasswordDTO>(updatePasswordDto);
        if(!nullPassRes.success) return nullPassRes;
        var appUser = await _unitOfWork.Users.GetByIdAsync(userId);
        var res = await CheckNullObjectAsync<bool, ApplicationUser>(appUser);
        if (!res.success) return res;

        var passwordValid = await _userManager.CheckPasswordAsync(appUser, updatePasswordDto.CurrentPassword);
        if (!passwordValid)
        {
            var errors = new List<string> { "Invalid Password." };
            return await ServiceResult<bool>.Failure(errors);
        }
        if (updatePasswordDto.CurrentPassword==updatePasswordDto.UpdatedPassword)
        {
            var errors = new List<string> { "It is the same old password." };
            return await ServiceResult<bool>.Failure(errors);
        }

        var result = await _userManager.ChangePasswordAsync(appUser, updatePasswordDto.CurrentPassword, updatePasswordDto.UpdatedPassword);
        if (!result.Succeeded)
        {
            var errors = new List<string> { "Cannot Update Current Password." };
            return await ServiceResult<bool>.Failure(errors);
        }
        await _userManager.UpdateAsync(appUser);
        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ResetEmail(int userId, string updatedEmail)
    {
        var userApp = await _unitOfWork.Users.GetByIdAsync(userId);
        var res = await CheckNullObjectAsync<bool, ApplicationUser>(userApp);
        if(!res.success) return res;

        var existingUser = await _userManager.FindByEmailAsync(updatedEmail);
        if (existingUser != null)
        {
            return await ServiceResult<bool>.Failure(new List<string> { "Email already in use." });
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(userApp, updatedEmail);
        var confirmationUrl =
            $"{_configuration.GetValue<string>("ClientUrl")}/app/User/confirm-email-change?userId={userId}&newEmail={Uri.EscapeDataString(updatedEmail)}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendConfirmationLinkAsync(updatedEmail, confirmationUrl);

        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ConfirmEmailChangeAsync(int userId, string updatedEmail, string token)
    {
        var userApp = await _unitOfWork.Users.GetByIdAsync(userId);
        var res = await CheckNullObjectAsync<bool, ApplicationUser>(userApp);
        if (!res.success) return res;

        var result = await _userManager.ChangeEmailAsync(userApp, updatedEmail, token);
        if (!result.Succeeded)
        {
            return await ServiceResult<bool>.Failure(result.Errors.Select(e => e.Description).ToList());
        }

        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<PagedResponseDto<FollowUserDto>>> GetFollowersAsync(int userId, int page, int pageSize)
    {
        var targetUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (targetUser == null)
        {
            return await ServiceResult<PagedResponseDto<FollowUserDto>>.Failure(new List<string> { "User not found" });
        }

        int skipCount = (page - 1) * pageSize;

        var follows = await _unitOfWork.Follows.GetPagedAsync(
            skipCount,
            pageSize + 1,
            f => f.CreatedAt,
            false,
            f => f.FollowedId == userId && !f.IsDeleted && f.IsActive,
            new[] { "Follower" }
        );

        bool hasMore = follows.Count > pageSize;
        var trimmed = follows.Take(pageSize).ToList();

        var items = trimmed.Select(f => new FollowUserDto
        {
            Id = f.Follower.Id,
            Name = f.Follower.Name,
            Username = f.Follower.UserName,
            AvatarPath = f.Follower.AvatarPath,
            Bio = f.Follower.Bio,
            FollowedSince = f.CreatedAt
        }).ToList();

        return await ServiceResult<PagedResponseDto<FollowUserDto>>.Success(new PagedResponseDto<FollowUserDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            HasMore = hasMore
        });
    }

    public async Task<ServiceResult<PagedResponseDto<FollowUserDto>>> GetFollowingAsync(int userId, int page, int pageSize)
    {
        var sourceUser = await _unitOfWork.Users.GetByIdAsync(userId);
        if (sourceUser == null)
        {
            return await ServiceResult<PagedResponseDto<FollowUserDto>>.Failure(new List<string> { "User not found" });
        }

        int skipCount = (page - 1) * pageSize;

        var follows = await _unitOfWork.Follows.GetPagedAsync(
            skipCount,
            pageSize + 1,
            f => f.CreatedAt,
            false,
            f => f.FollowerId == userId && !f.IsDeleted && f.IsActive,
            new[] { "Followed" }
        );

        bool hasMore = follows.Count > pageSize;
        var trimmed = follows.Take(pageSize).ToList();

        var items = trimmed.Select(f => new FollowUserDto
        {
            Id = f.Followed.Id,
            Name = f.Followed.Name,
            Username = f.Followed.UserName,
            AvatarPath = f.Followed.AvatarPath,
            Bio = f.Followed.Bio,
            FollowedSince = f.CreatedAt
        }).ToList();

        return await ServiceResult<PagedResponseDto<FollowUserDto>>.Success(new PagedResponseDto<FollowUserDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            HasMore = hasMore
        });
    }

    public async Task<ServiceResult<bool>> IsFollowingAsync(int followerId, int targetUserId)
    {
        var isFollowing = await _unitOfWork.Follows.GetAll()
            .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == targetUserId && !f.IsDeleted && f.IsActive);

        return await ServiceResult<bool>.Success(isFollowing);
    }

    public async Task<ServiceResult<string>> UpdateAvatarAsync(int userId, string avatarPath)
    {
        var appUser = await _unitOfWork.Users.GetByIdAsync(userId);
        var res = await CheckNullObjectAsync<string, ApplicationUser>(appUser);
        if (!res.success) return res;

        appUser.AvatarPath = avatarPath;
        await _unitOfWork.Users.UpdateAsync(appUser);
        await _unitOfWork.SaveAsync();

        return await ServiceResult<string>.Success(avatarPath);
    }

    public async Task<ServiceResult<PagedResponseDto<UserSearchResultDto>>> SearchUsersAsync(string query, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await ServiceResult<PagedResponseDto<UserSearchResultDto>>.Success(new PagedResponseDto<UserSearchResultDto>
            {
                Items = new List<UserSearchResultDto>(),
                PageNumber = page,
                PageSize = pageSize,
                HasMore = false
            });
        }

        var normalizedQuery = query.Trim().ToLower();
        int skipCount = (page - 1) * pageSize;

        var users = _unitOfWork.Users.GetAll()
            .Where(u => u.UserName.ToLower().Contains(normalizedQuery) || u.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(u => u.UserName)
            .Skip(skipCount)
            .Take(pageSize + 1);

        var list = await users.ToListAsync();
        bool hasMore = list.Count > pageSize;
        var trimmed = list.Take(pageSize).ToList();

        var items = trimmed.Select(u => new UserSearchResultDto
        {
            Id = u.Id,
            Name = u.Name,
            Username = u.UserName,
            AvatarPath = u.AvatarPath,
            Bio = u.Bio,
            FollowersCount = u.FollowersCount
        }).ToList();

        return await ServiceResult<PagedResponseDto<UserSearchResultDto>>.Success(new PagedResponseDto<UserSearchResultDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            HasMore = hasMore
        });
    }


    // Helper check null object
    private async Task<ServiceResult<T>> CheckNullObjectAsync<T,Y>(Y obj, string errorMessage = "Not Found")
    {
        if (obj == null)
        {
            return await ServiceResult<T>.Failure(new List<string>() { errorMessage });
        }

        return await ServiceResult<T>.Success();
    }
}
