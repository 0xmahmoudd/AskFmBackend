using AskFm.BLL.DTO;
using AskFm.DAL.Enums;

namespace AskFm.BLL.Services;

public interface INotificationService
{
    Task<ServiceResult<List<NotificationDto>>> GetUserNotifications(int userId, int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult<List<NotificationDto>>> GetNotificationsByType(int userId, string category, int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult<string>> MarkNotificationAsRead(int notificationId, int userId);
    Task<ServiceResult<string>> MarkAllNotificationsAsRead(int userId);
    Task<ServiceResult<NotificationDto>> CreateNotification(int userId, NotificationStatus type, int resourceId, string message);
    Task<ServiceResult<int>> GetUnreadCount(int userId);
}
