using AskFm.DAL;
using AskFm.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AskFm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeedController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("stress-test")]
    public async Task<IActionResult> SeedStressTestData()
    {
        // 1. Create a base user to ask/receive the thread
        var baseUser = new ApplicationUser 
        { 
            UserName = "testuser_" + Guid.NewGuid().ToString().Substring(0,8),
            Email = Guid.NewGuid().ToString() + "@test.com",
            Name = "Test User",
            AvatarPath = "default.jpg",
            Bio = "I am a test user",
            PasswordHash = "AQAAAAEAACcQAAAAE..." // Mock hash
        };
        _context.Users.Add(baseUser);
        await _context.SaveChangesAsync();

        // 2. Create 10,000 dummy users (to satisfy FK constraints for likes/comments)
        var dummyUsers = new List<ApplicationUser>();
        var guidBase = Guid.NewGuid().ToString().Substring(0, 5);
        for (int i = 0; i < 10000; i++)
        {
            dummyUsers.Add(new ApplicationUser
            {
                UserName = $"dummy_{guidBase}_{i}",
                Email = $"dummy_{guidBase}_{i}@test.com",
                Name = $"Dummy {i}",
                AvatarPath = "default.jpg",
                Bio = "I am a dummy user",
                PasswordHash = "AQAAAAEAACcQAAAAE..."
            });
        }
        _context.Users.AddRange(dummyUsers);
        await _context.SaveChangesAsync(); // save to generate IDs

        // 3. Create a thread
        var thread = new AskFm.DAL.Models.Thread
        {
            AskerId = baseUser.Id,
            AskedId = baseUser.Id,
            QuestionContent = "This is a viral stress-test question?",
            AnswerContent = "This is the viral answer!",
            Status = AskFm.DAL.Enums.ThreadStatus.Answered,
            isAnonymous = false
        };
        _context.Threads.Add(thread);
        await _context.SaveChangesAsync();

        // 4. Follow base user by base user (so it appears in feed)
        _context.Follows.Add(new Follow { FollowerId = baseUser.Id, FollowedId = baseUser.Id });
        await _context.SaveChangesAsync();

        // 5. Create 10,000 likes
        var likes = new List<ThreadLike>();
        for (int i = 0; i < 10000; i++)
        {
            likes.Add(new ThreadLike { ThreadId = thread.Id, UserId = dummyUsers[i].Id });
        }
        _context.ThreadLikes.AddRange(likes);

        // 6. Create 5,000 comments
        var comments = new List<Comment>();
        for (int i = 0; i < 5000; i++)
        {
            comments.Add(new Comment { ThreadId = thread.Id, UserId = dummyUsers[i].Id, Content = $"Viral comment {i}" });
        }
        _context.Comments.AddRange(comments);

        // 7. Create 50 notifications for the base user to test N+1
        var notifications = new List<Notification>();
        for (int i = 0; i < 50; i++)
        {
            notifications.Add(new Notification
            {
                UserId = baseUser.Id,
                Type = AskFm.DAL.Enums.NotificationStatus.FOLLOW,
                ResourceId = dummyUsers[i].Id, // Assume they followed
                Message = $"Dummy {i} followed you",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.Notifications.AddRange(notifications);

        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "Successfully seeded stress test data!",
            threadId = thread.Id,
            userId = baseUser.Id
        });
    }

    [HttpGet("test-feed")]
    public async Task<IActionResult> TestFeedPerf(int userId)
    {
        var sw = Stopwatch.StartNew();
        
        // This simulates exactly what GetFeed does, but bypassing auth to easily measure
        int pageSize = 10;
        var followedUserIds = await _context.Follows.Where(f => f.FollowerId == userId).Select(f => f.FollowedId).ToListAsync();
        followedUserIds.Add(userId);

        var threads = await _context.Threads
            .Include(t => t.Asker)
            .Include(t => t.Asked)
            .Include(t => t.Comments)
            .Include(t => t.ThreadLikes)
            .Where(t => followedUserIds.Contains(t.AskedId) && t.Status == AskFm.DAL.Enums.ThreadStatus.Answered)
            .OrderByDescending(t => t.CreatedAt)
            .Take(pageSize + 1)
            .ToListAsync();

        var trimmed = threads.Take(pageSize).ToList();
        var threadDtos = trimmed.Select(t => new 
        {
            t.Id,
            LikesCount = t.ThreadLikes?.Count ?? 0,
            CommentsCount = t.Comments?.Count ?? 0
        }).ToList();

        sw.Stop();
        
        return Ok(new { 
            timeTakenMs = sw.ElapsedMilliseconds,
            threadsReturned = threadDtos.Count,
            data = threadDtos
        });
    }
    [HttpGet("test-thread")]
    public async Task<IActionResult> TestThreadPerf(int threadId)
    {
        var sw = Stopwatch.StartNew();
        
        var thread = await _context.Threads
            .Include(t => t.Asker)
            .Include(t => t.Asked)
            .Include(t => t.Comments)
            .Include(t => t.ThreadLikes)
            .FirstOrDefaultAsync(t => t.Id == threadId && !t.IsDeleted);

        var dto = new
        {
            thread?.Id,
            LikesCount = thread?.ThreadLikes?.Count ?? 0,
            CommentsCount = thread?.Comments?.Count ?? 0
        };

        sw.Stop();
        
        return Ok(new { 
            timeTakenMs = sw.ElapsedMilliseconds,
            data = dto
        });
    }

    [HttpGet("test-notifications")]
    public async Task<IActionResult> TestNotificationsPerf(int userId)
    {
        var sw = Stopwatch.StartNew();
        
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .ToListAsync();

        var notificationDtos = new List<object>();

        foreach (var notification in notifications)
        {
            // Simulate what GetActorUserByResourceId does (N+1)
            ApplicationUser actorUser = null;
            if (notification.Type == AskFm.DAL.Enums.NotificationStatus.FOLLOW)
            {
                var follow = await _context.Follows.Include(f => f.Follower).FirstOrDefaultAsync(f => f.FollowedId == notification.ResourceId);
                actorUser = follow?.Follower;
            }
            // other types omitted for brevity

            notificationDtos.Add(new 
            {
                notification.Id,
                notification.Message,
                ActorName = actorUser?.UserName ?? "Unknown"
            });
        }

        sw.Stop();
        
        return Ok(new { 
            timeTakenMs = sw.ElapsedMilliseconds,
            count = notificationDtos.Count,
            data = notificationDtos
        });
    }
}
