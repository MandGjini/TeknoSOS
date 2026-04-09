namespace TeknoSOS.WebApp.Services;

public interface IWhatsAppCloudSender
{
    Task<bool> SendTextMessageAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}
