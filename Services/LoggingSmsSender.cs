namespace TeknoSOS.WebApp.Services;

public class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // Prepared channel: wire Twilio/Vonage/Infobip provider here when credentials are configured.
        _logger.LogInformation("SMS channel prepared. Message not sent (provider not configured). To: {PhoneNumber}, Message: {Message}", phoneNumber, message);
        return Task.FromResult(false);
    }
}
