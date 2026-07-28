using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using AskFm.BLL.Services;

namespace AskFm.BLL.Hub
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("UserId")?.Value
                ?? Context.UserIdentifier;
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int uid))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var countResult = await notificationService.GetUnreadCount(uid);
                if (countResult.success)
                {
                    await Clients.Caller.SendAsync("UnreadCountUpdated", countResult.Data);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}