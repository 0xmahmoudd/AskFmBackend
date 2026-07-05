using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using AskFm.BLL.Services;

namespace AskFm.BLL.Hub
{
    [Authorize]
    public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            // Auto-join user to their group based on their ID from JWT token
            var userId = Context.UserIdentifier;
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
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}