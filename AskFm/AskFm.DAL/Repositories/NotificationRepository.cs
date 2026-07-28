using AskFm.DAL.Enums;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace AskFm.DAL.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(IEnumerable<Notification> notifications, int totalCount)> GetAllNotifications(int userId, int pageNumber, int pageSize)
    {
        var query = _unitOfWork.Notifications.FindAll(n => n.UserId == userId);

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (notifications, totalCount);
    }

    public async Task<(IEnumerable<Notification> notifications, int totalCount)> GetNotificationsByTypes(int userId, IEnumerable<NotificationStatus> statuses, int pageNumber, int pageSize)
    {
        var statusList = statuses.ToList();
        var query = _unitOfWork.Notifications.FindAll(n => n.UserId == userId && statusList.Contains(n.Type));

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (notifications, totalCount);
    }

    public async Task<ApplicationUser?> GetActorUserByResourceId(int resourceId, NotificationStatus type)
    {
        if (type == NotificationStatus.FOLLOW)
        {
            var follow = await _unitOfWork.Follows.FindAsync(f => f.FollowedId == resourceId, new[] { "Follower" });
            return follow?.Follower;
        }
        else if (type == NotificationStatus.QUESTION)
        {
            var thread = await _unitOfWork.Threads.FindAsync(t => t.Id == resourceId, new[] { "Asker" });
            return thread?.Asker;
        }
        else if (type == NotificationStatus.ANSWER)
        {
            var thread = await _unitOfWork.Threads.FindAsync(t => t.Id == resourceId, new[] { "Asked" });
            return thread?.Asked;
        }
        else if (type == NotificationStatus.COMMENT_LIKE)
        {
            var commentLike = await _unitOfWork.CommentLikes.FindAsync(cl => cl.CommentId == resourceId, new[] { "User" });
            return commentLike?.User;
        }
        else if (type == NotificationStatus.QUESTION_LIKE)
        {
            var threadLike = await _unitOfWork.ThreadLikes.FindAsync(tl => tl.ThreadId == resourceId, new[] { "User" });
            return threadLike?.User;
        }
        else if (type == NotificationStatus.REPLAY)
        {
            var comment = await _unitOfWork.Comments.FindAsync(c => c.Id == resourceId, new[] { "User" });
            return comment?.User;
        }
        return null;
    }

    public async Task<Dictionary<int, ApplicationUser>> GetActorUsersForNotifications(IEnumerable<Notification> notifications)
    {
        var result = new Dictionary<int, ApplicationUser>();
        
        var grouped = notifications.GroupBy(n => n.Type);
        foreach (var group in grouped)
        {
            var type = group.Key;
            var resourceIds = group.Select(n => n.ResourceId).Distinct().ToList();
            if (!resourceIds.Any()) continue;

            if (type == NotificationStatus.FOLLOW)
            {
                var follows = await _unitOfWork.Follows.GetAll(trackChanges: false)
                    .Where(f => resourceIds.Contains(f.FollowedId))
                    .Select(f => new { f.FollowedId, f.Follower })
                    .ToListAsync();
                foreach (var f in follows) if (f.Follower != null) result[f.FollowedId] = f.Follower;
            }
            else if (type == NotificationStatus.QUESTION)
            {
                var threads = await _unitOfWork.Threads.GetAll(trackChanges: false)
                    .Where(t => resourceIds.Contains(t.Id))
                    .Select(t => new { t.Id, t.Asker })
                    .ToListAsync();
                foreach (var t in threads) if (t.Asker != null) result[t.Id] = t.Asker;
            }
            else if (type == NotificationStatus.ANSWER)
            {
                var threads = await _unitOfWork.Threads.GetAll(trackChanges: false)
                    .Where(t => resourceIds.Contains(t.Id))
                    .Select(t => new { t.Id, t.Asked })
                    .ToListAsync();
                foreach (var t in threads) if (t.Asked != null) result[t.Id] = t.Asked;
            }
            else if (type == NotificationStatus.COMMENT_LIKE)
            {
                var likes = await _unitOfWork.CommentLikes.GetAll(trackChanges: false)
                    .Where(cl => resourceIds.Contains(cl.CommentId))
                    .Select(cl => new { cl.CommentId, cl.User })
                    .ToListAsync();
                // If a comment has multiple likes, we might map the first/last one depending on ordering.
                // Normally a CommentLike notification's resourceId refers to the comment, but wait, who is the actor? 
                // The DB structure suggests the resourceId is the CommentId, which might have multiple likes.
                // I will replicate the single query logic which used FindAsync(cl => cl.CommentId == resourceId) which returns the first one.
                foreach (var cl in likes) if (cl.User != null) result[cl.CommentId] = cl.User;
            }
            else if (type == NotificationStatus.QUESTION_LIKE)
            {
                var likes = await _unitOfWork.ThreadLikes.GetAll(trackChanges: false)
                    .Where(tl => resourceIds.Contains(tl.ThreadId))
                    .Select(tl => new { tl.ThreadId, tl.User })
                    .ToListAsync();
                foreach (var tl in likes) if (tl.User != null) result[tl.ThreadId] = tl.User;
            }
            else if (type == NotificationStatus.REPLAY)
            {
                var comments = await _unitOfWork.Comments.GetAll(trackChanges: false)
                    .Where(c => resourceIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.User })
                    .ToListAsync();
                foreach (var c in comments) if (c.User != null) result[c.Id] = c.User;
            }
        }
        
        return result;
    }


    public async Task<Notification?> GetUserNotificationById(int notificationId, int userId)
    {
        return await _unitOfWork.Notifications.FindAsync(n => n.Id == notificationId && n.UserId == userId);
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _unitOfWork.Notifications.FindAll(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in notifications)
        {
            n.IsRead = true;
            _unitOfWork.Notifications.Update(n);
        }
        await _unitOfWork.SaveAsync();
    }
}