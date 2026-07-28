using AskFm.DAL.Enums;
using AskFm.DAL.Models;

namespace AskFm.DAL.Interfaces;

public interface INotificationRepository
{
    Task<(IEnumerable<Notification> notifications, int totalCount)> GetAllNotifications(int userId, int pageNumber, int pageSize);
    Task<(IEnumerable<Notification> notifications, int totalCount)> GetNotificationsByTypes(int userId, IEnumerable<NotificationStatus> statuses, int pageNumber, int pageSize);
    Task<ApplicationUser?> GetActorUserByResourceId(int resourceId, NotificationStatus type);
    Task<Dictionary<int, ApplicationUser>> GetActorUsersForNotifications(IEnumerable<Notification> notifications);
    Task<Notification?> GetUserNotificationById(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}