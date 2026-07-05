using AskFm.BLL.DTO;
using AskFm.DAL.Enums;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Thread = AskFm.DAL.Models.Thread;

namespace AskFm.BLL.Services;

public class ThreadService : IThreadService
{
    private IUnitOfWork _unitOfWork;
    private readonly ILogger<ThreadService> _logger;
    private readonly IMapper _mapper;

    public ThreadService(IUnitOfWork unitOfWork, ILogger<ThreadService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ServiceResult<ThreadResponseDto>> AddThread(int askerId, CreateThreadDto createThreadDto)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // check if the Asked user exite or not
            var askedId = createThreadDto.AskedId;

            var askedUser = await _unitOfWork.Users.GetByIdAsync(askedId);
            if (askedUser == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(
                    new List<string>() { "Could not find asked user" });
            }

            // getting the Asker user object
            var askerUser = await _unitOfWork.Users.GetByIdAsync(askerId);
            if (askerUser == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(
                    new List<string>() { "Could not find asker user" });
            }


            // check if the Asked User's id == Asker User's Id
            if (askedId == askerId)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(
                    new List<string>() { "User can not ask him self" });
            }

            // Check if there is a block in either direction
            var block = await _unitOfWork.UserBlocks.FindAsync(
                b => (b.BlockerId == askerId && b.BlockedId == askedId) || 
                     (b.BlockerId == askedId && b.BlockedId == askerId)
            );

            if (block != null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(
                    new List<string>() { "You cannot ask this user questions" });
            }

            // check if the QuestionContent is empty or not
            if (string.IsNullOrEmpty(createThreadDto.QuestionContent))
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(new List<string>()
                    { "Question Can't be null or empty" });
            }

            // creating a new thread
            Thread thread = new Thread
            {
                AskedId = askedId,

                AskerId = askerId,

                QuestionContent = createThreadDto.QuestionContent,

                AnswerContent = "",
                Status = createThreadDto.Status,
                isAnonymous = createThreadDto.IsAnonymous,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.Threads.AddAsync(thread);
            askerUser.AskedThreads?.Add(thread);
            askedUser.ReceivedThreads?.Add(thread);
            await _unitOfWork.Users.UpdateAsync(askedUser);
            await _unitOfWork.Users.UpdateAsync(askerUser);
            await _unitOfWork.SaveAsync();

            await transaction.CommitAsync();

            var responseDto = new ThreadResponseDto
            {
                Id = thread.Id,
                QuestionContent = thread.QuestionContent,
                Status = thread.Status,
                IsAnonymous = thread.isAnonymous,
                CreatedAt = thread.CreatedAt,
                AskerId = thread.AskerId.Value,
                AskerName = thread.isAnonymous ? "Anonymous" : thread.Asker?.Name,
                AskedId = thread.AskedId,
                AskedName = askedUser.Name
            };

            return await ServiceResult<ThreadResponseDto>.Success(responseDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding thread");
            return await ServiceResult<ThreadResponseDto>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<ThreadResponseDto>> GetThreadById(int id)
    {
        try
        {
            var thread = await _unitOfWork.Threads.FindAsync(
                predicate: t => t.Id == id && !t.IsDeleted,
                includes: new[] { "Asker", "Asked", "Comments", "ThreadLikes" }
            );

            if (thread == null)
            {
                return await ServiceResult<ThreadResponseDto>.Failure(new List<string>() { "Thread not found" });
            }

            var threadDto = new ThreadResponseDto
            {
                Id = thread.Id,
                QuestionContent = thread.QuestionContent,
                AnswerContent = thread.AnswerContent,
                Status = thread.Status,
                IsAnonymous = thread.isAnonymous,
                CreatedAt = thread.CreatedAt,
                AskerId = thread.AskerId ?? 0,
                AskerName = thread.isAnonymous ? "Anonymous" : thread.Asker?.Name,
                AskedId = thread.AskedId,
                AskedName = thread.Asked?.Name,
                LikesCount = thread.ThreadLikes?.Count ?? 0,
                CommentsCount = thread.Comments?.Count ?? 0
            };

            return await ServiceResult<ThreadResponseDto>.Success(threadDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving thread");
            return await ServiceResult<ThreadResponseDto>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetAllThreads(int askedId, int page, int pageSize)
    {
        try
        {
            int skipCount = (page - 1) * pageSize;

            // Get all threads for user, paginated
            var threads = await _unitOfWork.Threads.GetPagedAsync(
                skipCount,
                pageSize + 1,
                t => t.CreatedAt,
                false,
                t => t.AskedId == askedId && t.Status != ThreadStatus.Hidden,
                new[] { "Asker", "Asked", "Comments", "ThreadLikes" }
            );

            bool hasMore = threads.Count > pageSize;
            var trimmed = threads.Take(pageSize).ToList();

            var threadDtos = trimmed.Select(thread => new ThreadResponseDto
            {
                Id = thread.Id,
                QuestionContent = thread.QuestionContent,
                AnswerContent = thread.AnswerContent,
                Status = thread.Status,
                IsAnonymous = thread.isAnonymous,
                CreatedAt = thread.CreatedAt,
                AskerId = thread.AskerId ?? 0,
                AskerName = thread.isAnonymous ? "Anonymous" : thread.Asker?.Name,
                AskedId = thread.AskedId,
                AskedName = thread.Asked?.Name,
                LikesCount = thread.ThreadLikes?.Count ?? 0,
                CommentsCount = thread.Comments?.Count ?? 0
            }).ToList();

            var response = new PagedResponseDto<ThreadResponseDto>
            {
                Items = threadDtos,
                PageNumber = page,
                PageSize = pageSize,
                HasMore = hasMore
            };

            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Success(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving threads");
            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<ThreadResponseDto>> AnswerThread(int threadId, int userId,
        AnswerThreadDto answerDto)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            if (string.IsNullOrWhiteSpace(answerDto.AnswerContent))
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(
                    new List<string> { "Answer cannot be empty or contain only whitespace" });
            }

            var trimmedAnswer = answerDto.AnswerContent.Trim();
            if (trimmedAnswer.Length > 5000)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(
                    new List<string> { "Answer cannot exceed 5000 characters" });
            }

            var thread = await _unitOfWork.Threads.FindAsync(
                predicate: t => t.Id == threadId,
                includes: new[] { "Asker", "Asked" }
            );

            if (thread == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(new List<string>() { "Thread not found" });
            }

            if (thread.AskedId != userId)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<ThreadResponseDto>.Failure(new List<string>()
                    { "User not authorized to answer this thread" });
            }

            thread.AnswerContent = answerDto.AnswerContent;
            thread.Status = ThreadStatus.Answered;

            await _unitOfWork.Threads.UpdateAsync(thread);
            await _unitOfWork.SaveAsync();

            await transaction.CommitAsync();

            var threadDto = new ThreadResponseDto
            {
                Id = thread.Id,
                QuestionContent = thread.QuestionContent,
                AnswerContent = thread.AnswerContent,
                Status = thread.Status,
                IsAnonymous = thread.isAnonymous,
                CreatedAt = thread.CreatedAt,
                AskerId = thread.AskerId ?? 0,
                AskerName = thread.isAnonymous ? "Anonymous" : thread.Asker?.Name,
                AskedId = thread.AskedId,
                LikesCount = thread.ThreadLikes?.Count ?? 0,
                CommentsCount = thread.Comments?.Count ?? 0,
                AskedName = thread.Asked?.Name
            };

            return await ServiceResult<ThreadResponseDto>.Success(threadDto);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error answering thread");
            return await ServiceResult<ThreadResponseDto>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetThreads(int page, int pageSize)
    {
        try
        {
            int skipCount = (page - 1) * pageSize;

            // var totalCount = await _unitOfWork.Threads.CountAsync();

            var threads = await _unitOfWork.Threads.GetPagedAsync(
                skipCount,
                pageSize + 1,
                t => t.CreatedAt,
                false,
                t => true,
                new[] { "Asker", "Asked", "Comments", "ThreadLikes" }
            );

            bool hasMore = threads.Count > pageSize;

            var trimmed = threads.Take(pageSize).ToList();

            var threadDtos = trimmed.Select(thread => new ThreadResponseDto
            {
                Id = thread.Id,
                QuestionContent = thread.QuestionContent,
                AnswerContent = thread.AnswerContent,
                Status = thread.Status,
                IsAnonymous = thread.isAnonymous,
                CreatedAt = thread.CreatedAt,
                AskerId = thread.AskerId ?? 0,
                AskerName = thread.isAnonymous ? "Anonymous" : thread.Asker?.Name,
                AskedId = thread.AskedId,
                AskedName = thread.Asked?.Name,
                LikesCount = thread.ThreadLikes?.Count ?? 0,
                CommentsCount = thread.Comments?.Count ?? 0
            }).ToList();

            var response = new PagedResponseDto<ThreadResponseDto>
            {
                Items = threadDtos,
                PageNumber = page,
                PageSize = pageSize,
                HasMore = hasMore

            };

            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Success(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving threads");
            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeleteThread(int threadId, int userId)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get the thread
            var thread = await _unitOfWork.Threads.FindAsync(t => t.Id == threadId);

            if (thread == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<bool>.Failure(new List<string>() { "Thread not found" });
            }

            // Check if user is authorized to delete this thread (either asker or asked)
            if (thread.AskerId != userId && thread.AskedId != userId)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<bool>.Failure(new List<string>()
                    { "User not authorized to delete this thread" });
            }

            // Delete the thread
            await _unitOfWork.Threads.RemoveAsync(thread);
            await _unitOfWork.SaveAsync();

            await transaction.CommitAsync();

            return await ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error deleting thread");
            return await ServiceResult<bool>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetFeed(int userId, int page, int pageSize)
    {
        try
        {
            int skipCount = (page - 1) * pageSize;

            var followedUsers = await _unitOfWork.Follows.FindAllAsync(f => f.FollowerId == userId);
            var followedUserIds = followedUsers.Select(f => f.FollowedId).ToList();

            followedUserIds.Add(userId);

            var totalCount = await _unitOfWork.Threads.CountAsync(t =>
                followedUserIds.Contains(t.AskedId) && t.Status == ThreadStatus.Answered);

            var threads = await _unitOfWork.Threads.GetPagedAsync(
                skipCount,
                pageSize + 1,
                t => t.CreatedAt,
                false,
                t => followedUserIds.Contains(t.AskedId) && t.Status == ThreadStatus.Answered,
                new[] { "Asker", "Asked", "Comments", "ThreadLikes" }
            );

            // asking if still  there is some remaining pages withou using COUNT()
            bool hasMore = threads.Count > pageSize;

            /* after getting the threads with size of pageSize + 1
             to find if there are remaining threads (nextPage) without usint TotalCound and totalPages
             now we need to return back only the PageSize of threads
             that what i'm doing here in the trimmed list,
             */
            var trimmed = threads.Take(pageSize).ToList();

            var threadDtos = trimmed.Select(thread => new ThreadResponseDto
            {
                Id = thread.Id,
                QuestionContent = thread.QuestionContent,
                AnswerContent = thread.AnswerContent,
                Status = thread.Status,
                IsAnonymous = thread.isAnonymous,
                CreatedAt = thread.CreatedAt,
                AskerId = thread.AskerId ?? 0,
                AskerName = thread.isAnonymous ? "Anonymous" : thread.Asker?.Name,
                AskedId = thread.AskedId,
                AskedName = thread.Asked?.Name,
                LikesCount = thread.ThreadLikes?.Count ?? 0,
                CommentsCount = thread.Comments?.Count ?? 0
            }).ToList();

            var response = new PagedResponseDto<ThreadResponseDto>
            {
                Items = threadDtos,
                PageNumber = page,
                PageSize = pageSize,
                HasMore = hasMore
            };

            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Success(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving feed");
            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<bool>> SaveThread(int threadId, int userId)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Check if thread exists
            var thread = await _unitOfWork.Threads.FindAsync(t => t.Id == threadId);
            if (thread == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<bool>.Failure(new List<string>() { "Thread not found" });
            }

            // Check if thread is already saved
            var existingSave = await _unitOfWork.SavedThreads.FindAsync(
                st => st.SavedThreadId == threadId && st.UserId == userId
            );

            if (existingSave != null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<bool>.Failure(new List<string>() { "Thread already saved" });
            }

            // Create saved thread
            var savedThread = new SavedThreads
            {
                SavedThreadId = threadId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Add saved thread
            await _unitOfWork.SavedThreads.AddAsync(savedThread);
            await _unitOfWork.SaveAsync();

            await transaction.CommitAsync();

            return await ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error saving thread");
            return await ServiceResult<bool>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<bool>> UnsaveThread(int threadId, int userId)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Find saved thread
            var savedThread = await _unitOfWork.SavedThreads.FindAsync(
                st => st.SavedThreadId == threadId && st.UserId == userId
            );

            if (savedThread == null)
            {
                await transaction.RollbackAsync();
                return await ServiceResult<bool>.Failure(new List<string>() { "Thread not saved" });
            }

            // Remove saved thread
            await _unitOfWork.SavedThreads.RemoveAsync(savedThread);
            await _unitOfWork.SaveAsync();

            await transaction.CommitAsync();

            return await ServiceResult<bool>.Success(true);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error unsaving thread");
            return await ServiceResult<bool>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<PagedResponseDto<ThreadResponseDto>>> GetSavedThreads(int userId, int page,
        int pageSize)
    {
        try
        {
            int skipCount = (page - 1) * pageSize;

            // var totalCount = await _unitOfWork.SavedThreads.CountAsync(st => st.UserId == userId);

            var savedThreads = await _unitOfWork.SavedThreads.GetPagedAsync(
                skipCount,
                pageSize + 1,
                st => st.CreatedAt,
                false,
                st => st.UserId == userId,
                new[] { "Thread", "Thread.Asker", "Thread.Asked", "Thread.Comments", "Thread.ThreadLikes" }
            );


            bool hasMore = savedThreads.Count > pageSize;

            /* after getting the threads with size of pageSize + 1
             to find if there are remaining threads (nextPage) without usint TotalCound and totalPages
             now we need to return back only the PageSize of threads
             that what i'm doing here in the trimmed list,
            */
            var trimmed = savedThreads.Take(pageSize).ToList();

            var threadDtos = trimmed.Select(st => new ThreadResponseDto
            {
                Id = st.Thread.Id,
                QuestionContent = st.Thread.QuestionContent,
                AnswerContent = st.Thread.AnswerContent,
                Status = st.Thread.Status,
                IsAnonymous = st.Thread.isAnonymous,
                CreatedAt = st.Thread.CreatedAt,
                AskerId = st.Thread.AskerId ?? 0,
                AskerName = st.Thread.isAnonymous ? "Anonymous" : st.Thread.Asker?.Name,
                AskedId = st.Thread.AskedId,
                AskedName = st.Thread.Asked?.Name,
                LikesCount = st.Thread.ThreadLikes?.Count ?? 0,
                CommentsCount = st.Thread.Comments?.Count ?? 0,
                SavedAt = st.CreatedAt
            }).ToList();

            var response = new PagedResponseDto<ThreadResponseDto>
            {
                Items = threadDtos,
                PageNumber = page,
                PageSize = pageSize,
                HasMore = hasMore
            };

            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Success(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving saved threads");
            return await ServiceResult<PagedResponseDto<ThreadResponseDto>>.Failure(new List<string>() { e.Message });
        }
    }

    public async Task<ServiceResult<ThreadResponseDto>> ToggleThreadVisibilityAsync(int threadId, int userId)
    {
        try
        {
            var thread = await _unitOfWork.Threads.GetByIdAsync(threadId);

            if (thread == null)
                return await ServiceResult<ThreadResponseDto>.Failure(new List<string> { "Thread not found" });

            if (thread.AskedId != userId)
                return await ServiceResult<ThreadResponseDto>.Failure(new List<string> { "Only the profile owner can toggle thread visibility" });

            if (thread.Status == ThreadStatus.Answered)
            {
                thread.Status = ThreadStatus.Hidden;
            }
            else if (thread.Status == ThreadStatus.Hidden)
            {
                thread.Status = ThreadStatus.Answered;
            }
            else
            {
                return await ServiceResult<ThreadResponseDto>.Failure(new List<string> { "Thread cannot be hidden in its current state" });
            }

            _unitOfWork.Threads.Update(thread);
            await _unitOfWork.SaveAsync();

            var responseDto = new ThreadResponseDto
            {
                Id = thread.Id,
                QuestionContent = thread.QuestionContent,
                AnswerContent = thread.AnswerContent,
                Status = thread.Status,
                IsAnonymous = thread.isAnonymous,
                CreatedAt = thread.CreatedAt,
                AskerId = thread.AskerId ?? 0,
                AskerName = thread.isAnonymous ? "Anonymous" : thread.Asker?.Name,
                AskedId = thread.AskedId,
                AskedName = thread.Asked?.Name,
                LikesCount = thread.ThreadLikes?.Count ?? 0,
                CommentsCount = thread.Comments?.Count ?? 0
            };

            return await ServiceResult<ThreadResponseDto>.Success(responseDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error toggling thread visibility");
            return await ServiceResult<ThreadResponseDto>.Failure(new List<string> { e.Message });
        }
    }
}
