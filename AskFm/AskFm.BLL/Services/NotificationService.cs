using AskFm.BLL.DTO;
using AskFm.BLL.Hub;
using AskFm.DAL.Enums;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AskFm.BLL.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    public async Task<ServiceResult<List<NotificationDto>>> GetUserNotifications(int userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var (notifications, totalCount) = await _notificationRepository.GetAllNotifications(userId, pageNumber, pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var actorDictionary = await _notificationRepository.GetActorUsersForNotifications(notifications);

            var notificationDtos = new List<NotificationDto>();

            foreach (var notification in notifications)
            {
                actorDictionary.TryGetValue(notification.ResourceId, out var actorUser);
                notificationDtos.Add(new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Type = notification.Type.ToString(),
                    ResourceId = notification.ResourceId,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    Actor = actorUser == null ? null : new ActorDto
                    {
                        Id = actorUser.Id,
                        Username = actorUser?.UserName ?? "Unknown",
                        AvatarPath = actorUser?.AvatarPath ?? String.Empty
                    },
                    Pagination = new PaginationDto
                    {
                        CurrentPage = pageNumber,
                        TotalPages = totalPages,
                        TotalCount = totalCount,
                        HasNext = pageNumber < totalPages,
                        HasPrevious = pageNumber > 1
                    }
                });
            }
            return await ServiceResult<List<NotificationDto>>.Success(notificationDtos);
        }
        catch (Exception ex)
        {
            return await ServiceResult<List<NotificationDto>>.Failure(new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<List<NotificationDto>>> GetNotificationsByType(int userId, string category, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var upperCategory = category.ToUpper();
            List<NotificationStatus> statuses = new List<NotificationStatus>();

            if (upperCategory == "COMMENT" || upperCategory == "REPLAY")
            {
                statuses.Add(NotificationStatus.REPLAY);
            }
            else if (upperCategory == "LIKE")
            {
                statuses.Add(NotificationStatus.QUESTION_LIKE);
                statuses.Add(NotificationStatus.COMMENT_LIKE);
            }
            else if (Enum.TryParse<NotificationStatus>(upperCategory, out var parsedType))
            {
                statuses.Add(parsedType);
            }
            else
            {
                return await ServiceResult<List<NotificationDto>>.Failure(new List<string> { $"Invalid notification category: {category}" });
            }

            var (notifications, totalCount) = await _notificationRepository.GetNotificationsByTypes(userId, statuses, pageNumber, pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var actorDictionary = await _notificationRepository.GetActorUsersForNotifications(notifications);

            var notificationDtos = new List<NotificationDto>();

            foreach (var notification in notifications)
            {
                actorDictionary.TryGetValue(notification.ResourceId, out var actorUser);
                notificationDtos.Add(new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Type = notification.Type.ToString(),
                    ResourceId = notification.ResourceId,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    Actor = actorUser == null ? null : new ActorDto
                    {
                        Id = actorUser.Id,
                        Username = actorUser?.UserName ?? "Unknown",
                        AvatarPath = actorUser?.AvatarPath ?? String.Empty
                    },
                    Pagination = new PaginationDto
                    {
                        CurrentPage = pageNumber,
                        TotalPages = totalPages,
                        TotalCount = totalCount,
                        HasNext = pageNumber < totalPages,
                        HasPrevious = pageNumber > 1
                    }
                });
            }

            return await ServiceResult<List<NotificationDto>>.Success(notificationDtos);
        }
        catch (Exception ex)
        {
            return await ServiceResult<List<NotificationDto>>.Failure(new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<string>> MarkNotificationAsRead(int notificationId, int userId)
    {
        try
        {
            var notification = await _notificationRepository.GetUserNotificationById(notificationId, userId);
            if (notification == null)
                return await ServiceResult<string>.Failure(new List<string> { "Notification not found or access denied." });

            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveAsync();

            var count = await _unitOfWork.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdated", count);

            return await ServiceResult<string>.Success("notification has been read");
        }
        catch (Exception ex)
        {
            return await ServiceResult<string>.Failure(new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<string>> MarkAllNotificationsAsRead(int userId)
    {
        try
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);

            var count = await _unitOfWork.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdated", count);

            return await ServiceResult<string>.Success("All notifications marked as read");
        }
        catch (Exception ex)
        {
            return await ServiceResult<string>.Failure(new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<NotificationDto>> CreateNotification(int userId, NotificationStatus type, int resourceId, string message)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                ResourceId = resourceId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();

            // Get actor information for the notification
            var actorUser = await _notificationRepository.GetActorUserByResourceId(resourceId, type);

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type.ToString(),
                ResourceId = notification.ResourceId,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                Actor = actorUser == null ? null : new ActorDto
                {
                    Id = actorUser.Id,
                    Username = actorUser.UserName ?? "Unknown",
                    AvatarPath = actorUser.AvatarPath ?? string.Empty
                }
            };

            // Send real-time notification to the specific user
            await _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", notificationDto);

            var count = await _unitOfWork.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdated", count);

            return await ServiceResult<NotificationDto>.Success(notificationDto);
        }
        catch (Exception ex)
        {
            return await ServiceResult<NotificationDto>.Failure(new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<int>> GetUnreadCount(int userId)
    {
        try
        {
            var count = await _unitOfWork.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            return await ServiceResult<int>.Success(count);
        }
        catch (Exception ex)
        {
            return await ServiceResult<int>.Failure(new List<string> { ex.Message });
        }
    }
}