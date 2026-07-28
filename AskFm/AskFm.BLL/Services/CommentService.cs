using AskFm.BLL.DTO;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AskFm.BLL.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommentService> _logger;
    private readonly INotificationService _notificationService;

    public CommentService(IUnitOfWork unitOfWork, ILogger<CommentService> logger, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<ServiceResult<CommentResponseDto>> GetCommentAsync(int id)
    {
        try
        {
            var comment = await _unitOfWork.Comments.FindAsync(
                c => c.Id == id,
                new[] { "User", "CommentLikes" }
            );

            if (comment == null)
            {
                return await ServiceResult<CommentResponseDto>.Failure(new List<string>() { "Comment not found" });
            }

            var commentDto = new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.UserId,
                UserName = comment.User?.Name ?? "Unknown",
                UserAvatarPath = comment.User?.AvatarPath,
                ThreadId = comment.ThreadId,
                CreatedAt = comment.CreatedAt,
                LikesCount = comment.CommentLikes?.Count ?? 0,
            };

            return await ServiceResult<CommentResponseDto>.Success(commentDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving comment with ID {Id}", id);
            return await ServiceResult<CommentResponseDto>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<CommentResponseDto>> AddComment(int threadId, int userId, CreateCommentDto commentDto)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Check if thread exists
            var thread = await _unitOfWork.Threads.FindAsync(t => t.Id == threadId);
            if (thread == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<CommentResponseDto>.Failure(new List<string>() { "Thread not found" });
            }

            // Check if user exists
            var user = await _unitOfWork.Users.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<CommentResponseDto>.Failure(new List<string>() { "User not found" });
            }

            // Validate comment content
            if (string.IsNullOrWhiteSpace(commentDto.Content))
            {
                await transaction.RollbackAsync();
                return await ServiceResult<CommentResponseDto>.Failure(new List<string>() { "Comment content cannot be empty" });
            }

            // Create comment
            var comment = new Comment
            {
                Content = commentDto.Content,
                UserId = userId,
                ThreadId = threadId,
                CreatedAt = DateTime.UtcNow,
                CommentLikes = new List<CommentLike>()
            };

            // Add comment to database
            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveAsync();

            await transaction.CommitAsync();

            try
            {
                if (thread.AskedId != userId)
                {
                    await _notificationService.CreateNotification(
                        thread.AskedId,
                        DAL.Enums.NotificationStatus.REPLAY,
                        comment.Id,
                        $"{user.Name} commented on your question."
                    );
                }
            }
            catch (Exception)
            {
                // Ignore notification failure
            }

            // Return response dto
            var commentResponseDto = new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                UserId = comment.UserId,
                UserName = user.Name,
                UserAvatarPath = user.AvatarPath,
                ThreadId = comment.ThreadId,
                CreatedAt = comment.CreatedAt,
                LikesCount = 0,
            };

            return await ServiceResult<CommentResponseDto>.Success(commentResponseDto);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error adding comment to thread {ThreadId}", threadId);
            return await ServiceResult<CommentResponseDto>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<PagedResponseDto<CommentResponseDto>>> GetCommentsByThreadId(int threadId, int page, int pageSize)
    {
        try
        {
            // Check if thread exists
            var thread = await _unitOfWork.Threads.FindAsync(t => t.Id == threadId);
            if (thread == null)
            {
                return await ServiceResult<PagedResponseDto<CommentResponseDto>>.Failure(new List<string>() { "Thread not found" });
            }

            
            int skip =  (page - 1) * pageSize;
            // Get paginated comments
            var comments = await _unitOfWork.Comments.GetPagedAsync(
                skip: skip,
                take: pageSize + 1,
                orderBy: c => c.CreatedAt,
                ascending: false,  // Newest first
                predicate: c => c.ThreadId == threadId,
                includes: new[] { "User", "CommentLikes" }
            );
            
            bool hasMore = comments.Count > pageSize;

            var trimmed = comments.Take(pageSize).ToList();

            var commentDtos = trimmed.Select(c => new CommentResponseDto
            {
                Id = c.Id,
                Content = c.Content,
                UserId = c.UserId,
                UserName = c.User?.Name ?? "Unknown",
                UserAvatarPath = c.User?.AvatarPath,
                ThreadId = c.ThreadId,
                CreatedAt = c.CreatedAt,
                LikesCount = c.CommentLikes?.Count ?? 0,
            }).ToList();

            // Create paged response
            var response = new PagedResponseDto<CommentResponseDto>
            {
                Items = commentDtos,
                PageNumber = page,
                PageSize = pageSize,
                HasMore = hasMore
            };

            return await ServiceResult<PagedResponseDto<CommentResponseDto>>.Success(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving comments for thread {ThreadId}", threadId);
            return await ServiceResult<PagedResponseDto<CommentResponseDto>>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeleteComment(int threadId, int commentId, int userId)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Check if comment exists
            var comment = await _unitOfWork.Comments.FindAsync(
                c => c.Id == commentId && c.ThreadId == threadId,
                new[] { "CommentLikes" }
            );

            if (comment == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<bool>.Failure(new List<string>() { "Comment not found" });
            }

            // Check if user is authorized to delete (either comment owner or thread owner)
            var thread = await _unitOfWork.Threads.FindAsync(t => t.Id == threadId);
            
            if (comment.UserId != userId && thread.AskedId != userId && thread.AskerId != userId)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<bool>.Failure(new List<string>() { "User not authorized to delete this comment" });
            }

            // Remove comment likes first
            if (comment.CommentLikes != null && comment.CommentLikes.Any())
            {
                foreach (var like in comment.CommentLikes.ToList())
                {
                    await _unitOfWork.CommentLikes.RemoveAsync(like);
                }
            }

            // Remove comment
            await _unitOfWork.Comments.RemoveAsync(comment);
            await _unitOfWork.SaveAsync();

            await transaction.CommitAsync();

            return await ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error deleting comment {CommentId} from thread {ThreadId}", commentId, threadId);
            return await ServiceResult<bool>.Failure(new List<string>() { e.Message });
        }
    }
}