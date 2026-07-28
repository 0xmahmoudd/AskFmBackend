using AskFm.BLL.DTO;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AskFm.BLL.Services;

public class ThreadLikeService : IThreadLikeService
{
    private IUnitOfWork _unitOfWork;
    private readonly ILogger<CommentLikeService> _logger;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ThreadLikeService(IUnitOfWork unitOfWork, ILogger<CommentLikeService> logger, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    // add a like on the Thread that has id = id
    public async Task<ServiceResult<ThreadLikeResponseDto>> AddLike(int id, int userId)
    {
        try
        {
            var transaction = await _unitOfWork.BeginTransactionAsync();
            // get the thread, Including the  ThreadLike Collection
            var thread = await _unitOfWork.Threads.FindAsync(
                predicate: thread => thread.Id == id,
                includes: new[] { "ThreadLikes" }
            );

            // check if thread exists
            if (thread == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadLikeResponseDto>.Failure(new List<string>() { "Thread not found" });
            }

            // check if user has already liked this thread
            var existingLike = thread.ThreadLikes?.FirstOrDefault(like => like.UserId == userId);
            if (existingLike != null)
            {
                // if the The use already liked this thread
                if (!existingLike.IsDeleted)
                {
                    var errors = new List<String>()
                    {
                        "User has already liked this thread"
                    };
                    await transaction.RollbackAsync();
                    return await ServiceResult<ThreadLikeResponseDto>.Failure(errors);
                }

                // otherwise , the user liked the comment , then unliked it , and then wants to like it again
                existingLike.IsDeleted = false;
                existingLike.CreatedAt = DateTime.UtcNow;
                thread.ThreadLikes.Add(existingLike);
                _unitOfWork.Threads.Update(thread);
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Like added successfully for thread id: {threadId}", thread.Id);


                // updating the createdAt column to Now , ignoring the first time the user liked the comment
                return await ServiceResult<ThreadLikeResponseDto>.Success(new ThreadLikeResponseDto()
                {
                    ThreadId = existingLike.ThreadId,
                    UserId = existingLike.UserId,
                    CreatedAt = existingLike.CreatedAt
                });
            }


            try
            {
                // create a new ThreadLike
                var threadLike = new ThreadLike
                {
                    ThreadId = id,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // add the ThreadLike to the Thread
                thread.ThreadLikes?.Add(threadLike);

                // update the thread
                await _unitOfWork.Threads.UpdateAsync(thread);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                try
                {
                    if (thread.AskedId != userId)
                    {
                        var user = await _unitOfWork.Users.GetByIdAsync(userId);
                        await _notificationService.CreateNotification(
                            thread.AskedId,
                            DAL.Enums.NotificationStatus.QUESTION_LIKE,
                            thread.Id,
                            $"{user?.Name ?? "Someone"} liked your post."
                        );
                    }
                }
                catch (Exception)
                {
                    // Ignore notification errors
                }

                // create and return the response DTO
                var response = new ThreadLikeResponseDto
                {
                    ThreadId = threadLike.ThreadId,
                    UserId = threadLike.UserId,
                    CreatedAt = threadLike.CreatedAt
                };

                return await ServiceResult<ThreadLikeResponseDto>.Success(response);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            return await ServiceResult<ThreadLikeResponseDto>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<List<ThreadLikeResponseDto>>> GetLikes(int id)
    {
        try
        {
            // get the thread, Including the ThreadLikes collection
            var thread = await _unitOfWork.Threads.FindAsync(
                predicate: thread => thread.Id == id,
                includes: new[] { "ThreadLikes.User" }
            );

            // check if thread exists
            if (thread == null)
            {
                return await ServiceResult<List<ThreadLikeResponseDto>>.Failure(new List<string>()
                    { "Thread not found" });
            }

            // return the list of likes
            var likes = thread.ThreadLikes.Select(like => new ThreadLikeResponseDto
            {
                ThreadId = like.ThreadId,
                UserId = like.UserId,
                UserName = like.User?.Name ?? "Unknown",
                CreatedAt = like.CreatedAt
            }).ToList();

            return await ServiceResult<List<ThreadLikeResponseDto>>.Success(likes);
        }
        catch (Exception e)
        {
            return await ServiceResult<List<ThreadLikeResponseDto>>.Failure(new List<string>() { e.Message });
        }
    }

    // Remove a like from the thread with id = threadId
    public async Task<ServiceResult<bool>> RemoveLike(int threadId, int userId)
    {
        try
        {
            // get the thread with its likes
            var thread = await _unitOfWork.Threads.FindAsync(
                predicate: thread => thread.Id == threadId,
                includes: new[] { "ThreadLikes" }
            );

            // check if thread exists
            if (thread == null)
            {
                return await ServiceResult<bool>.Failure(new List<string>() { "Thread not found" });
            }

            // find the like to remove
            var likeToRemove = thread.ThreadLikes?.FirstOrDefault(like => like.UserId == userId);
            if (likeToRemove == null)
            {
                return await ServiceResult<bool>.Failure(new List<string>() { "Like not found for this user" });
            }

            var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // remove the like from the thread
                thread.ThreadLikes?.Remove(likeToRemove);

                // update the thread
                await _unitOfWork.Threads.UpdateAsync(thread);
                await _unitOfWork.SaveAsync();

                transaction.Commit();

                return await ServiceResult<bool>.Success(true);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            return await ServiceResult<bool>.Failure(new List<string>() { e.Message });
        }
    }
}