using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services;

public interface IPushNotificationSender
{
    Task<bool> SendToUserAsync(
        ApplicationUser user,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
