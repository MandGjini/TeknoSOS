using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Hubs
{
    /// <summary>
    /// SignalR hub for real-time notifications
    /// Each user joins their own group (user-{userId}) on connect.
    /// Server pushes notifications to specific users via their group.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Keep-alive ping from client to maintain connection
        /// </summary>
        public Task Ping()
        {
            return Task.CompletedTask;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                // Each user joins their own notification group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{user.Id}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{user.Id}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
