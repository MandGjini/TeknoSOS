using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TeknoSOS.WebApp.Services;

public class WhatsAppCloudSender : IWhatsAppCloudSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppCloudSender> _logger;

    public WhatsAppCloudSender(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppCloudSender> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendTextMessageAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var phoneNumberId = _configuration["WhatsApp:CloudApi:PhoneNumberId"];
        var accessToken = _configuration["WhatsApp:CloudApi:AccessToken"];
        var apiVersion = _configuration["WhatsApp:CloudApi:ApiVersion"] ?? "v21.0";

        if (string.IsNullOrWhiteSpace(phoneNumberId) || string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogInformation("WhatsApp Cloud API is not configured yet. Skipping outbound message to {Phone}", toPhoneNumber);
            return false;
        }

        var endpoint = $"https://graph.facebook.com/{apiVersion}/{phoneNumberId}/messages";
        var payload = new
        {
            messaging_product = "whatsapp",
            to = NormalizePhone(toPhoneNumber),
            type = "text",
            text = new { body = message }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var res = await _httpClient.SendAsync(req, cancellationToken);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("WhatsApp Cloud send failed: {Status} {Body}", res.StatusCode, body);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WhatsApp Cloud send exception for {Phone}", toPhoneNumber);
            return false;
        }
    }

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;

        var normalized = phone.Replace(" ", string.Empty).Replace("-", string.Empty);
        if (normalized.StartsWith("+")) normalized = normalized[1..];

        return normalized;
    }
}
