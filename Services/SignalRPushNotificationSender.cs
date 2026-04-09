using Microsoft.AspNetCore.SignalR;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Hubs;

namespace TeknoSOS.WebApp.Services;

public class SignalRPushNotificationSender : IPushNotificationSender
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<SignalRPushNotificationSender> _logger;

    public SignalRPushNotificationSender(
        IHubContext<NotificationHub> notificationHub,
        ILogger<SignalRPushNotificationSender> logger)
    {
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task<bool> SendToUserAsync(
        ApplicationUser user,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _notificationHub.Clients.Group($"user-{user.Id}").SendAsync("ReceiveNotification", new
            {
                title,
                message,
                data,
                timestamp = DateTime.UtcNow
            }, cancellationToken);

            if (!string.IsNullOrWhiteSpace(user.DevicePushToken))
            {
                _logger.LogInformation(
                    "Push provider integration prepared for user {UserId} on {Platform}. Token captured.",
                    user.Id,
                    user.DevicePlatform ?? "Unknown");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SignalR push notification to user {UserId}", user.Id);
            return false;
        }
    }
}
