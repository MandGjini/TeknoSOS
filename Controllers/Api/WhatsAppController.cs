using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using Microsoft.AspNetCore.Identity;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;
using System.Text.Json;
using System.Text;

namespace TeknoSOS.WebApp.Controllers.Api
{
    /// <summary>
    /// WhatsApp Integration API for creating cases via WhatsApp chatbot.
    /// Endpoint: /api/whatsapp
    /// WhatsApp Number: +355682030419
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<WhatsAppController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProfessionalOpportunityNotifier _opportunityNotifier;
        private readonly IWhatsAppCloudSender _whatsAppCloudSender;

        // API Key for authentication - should be set in appsettings.json
        private string ApiKey => _configuration["WhatsApp:ApiKey"] ?? "teknosos-whatsapp-2026";

        public WhatsAppController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<WhatsAppController> logger,
            IConfiguration configuration,
            IProfessionalOpportunityNotifier opportunityNotifier,
            IWhatsAppCloudSender whatsAppCloudSender)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _opportunityNotifier = opportunityNotifier;
            _whatsAppCloudSender = whatsAppCloudSender;
        }

        /// <summary>
        /// Create a new service request (case) from WhatsApp chatbot.
        /// POST /api/whatsapp/create-case
        /// </summary>
        /// <param name="request">Case details from WhatsApp</param>
        /// <returns>Created case details with tracking code</returns>
        [HttpPost("create-case")]
        public async Task<IActionResult> CreateCase([FromBody] WhatsAppCaseRequest request)
        {
            // Validate API Key
            if (!ValidateApiKey(Request))
            {
                _logger.LogWarning("WhatsApp API: Unauthorized access attempt from {PhoneNumber}", request?.PhoneNumber);
                return Unauthorized(new { success = false, message = "Invalid API key" });
            }

            if (request == null)
            {
                return BadRequest(new { success = false, message = "Request body is required" });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new { success = false, message = "Phone number is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { success = false, message = "Description is required" });
            }

            try
            {
                var response = await CreateCaseInternalAsync(request, "WhatsAppApi");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp API: Error creating case from phone {PhoneNumber}", request.PhoneNumber);
                return StatusCode(500, new { success = false, message = "Gabim gjate krijimit te raportit. Ju lutem provoni perseri." });
            }
        }

        /// <summary>
        /// Get case status by tracking code.
        /// GET /api/whatsapp/status/{trackingCode}
        /// </summary>
        [HttpGet("status/{trackingCode}")]
        public async Task<IActionResult> GetCaseStatus(string trackingCode)
        {
            if (!ValidateApiKey(Request))
            {
                return Unauthorized(new { success = false, message = "Invalid API key" });
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Citizen)
                .Include(s => s.Professional)
                .FirstOrDefaultAsync(s => s.UniqueCode == trackingCode);

            if (serviceRequest == null)
            {
                return NotFound(new { success = false, message = $"Nuk u gjet raporti me kod {trackingCode}" });
            }

            return Ok(new
            {
                success = true,
                trackingCode = serviceRequest.UniqueCode,
                title = serviceRequest.Title,
                description = serviceRequest.Description,
                category = serviceRequest.Category.ToString(),
                status = GetStatusText(serviceRequest.Status),
                statusCode = serviceRequest.Status.ToString(),
                priority = serviceRequest.Priority.ToString(),
                createdAt = serviceRequest.CreatedDate,
                assignedTechnician = serviceRequest.Professional != null 
                    ? $"{serviceRequest.Professional.FirstName} {serviceRequest.Professional.LastName}".Trim() 
                    : null,
                scheduledDate = serviceRequest.ScheduledDate,
                completedDate = serviceRequest.CompletedDate,
                estimatedCost = serviceRequest.EstimatedCost,
                finalCost = serviceRequest.FinalCost
            });
        }

        /// <summary>
        /// Get all cases for a phone number.
        /// GET /api/whatsapp/my-cases/{phoneNumber}
        /// </summary>
        [HttpGet("my-cases/{phoneNumber}")]
        public async Task<IActionResult> GetMyCases(string phoneNumber)
        {
            if (!ValidateApiKey(Request))
            {
                return Unauthorized(new { success = false, message = "Invalid API key" });
            }

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone || 
                    u.PhoneNumber == phoneNumber ||
                    u.PhoneNumber == "+" + phoneNumber);

            if (user == null)
            {
                return NotFound(new { success = false, message = "Nuk u gjet perdorues me kete numer telefoni" });
            }

            var cases = await _context.ServiceRequests
                .Where(s => s.CitizenId == user.Id)
                .OrderByDescending(s => s.CreatedDate)
                .Take(10)
                .Select(s => new
                {
                    trackingCode = s.UniqueCode,
                    title = s.Title,
                    status = GetStatusText(s.Status),
                    createdAt = s.CreatedDate,
                    category = s.Category.ToString()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                phoneNumber = phoneNumber,
                totalCases = cases.Count,
                cases = cases
            });
        }

        /// <summary>
        /// Webhook endpoint for receiving WhatsApp messages (for future integration with WhatsApp Business API).
        /// POST /api/whatsapp/webhook
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement payload)
        {
            // Verify webhook (for WhatsApp Business API verification)
            if (Request.Query.ContainsKey("hub.verify_token"))
            {
                var verifyToken = Request.Query["hub.verify_token"].ToString();
                var challenge = Request.Query["hub.challenge"].ToString();
                
                if (verifyToken == _configuration["WhatsApp:VerifyToken"])
                {
                    return Ok(challenge);
                }
                return Unauthorized();
            }

            try
            {
                _logger.LogInformation("WhatsApp Webhook received: {Payload}", payload.ToString());

                var incomingMessages = ExtractIncomingMessages(payload);
                if (incomingMessages.Count == 0)
                {
                    return Ok(new { status = "ignored", reason = "no_text_messages" });
                }

                var created = new List<object>();

                foreach (var incoming in incomingMessages)
                {
                    if (string.IsNullOrWhiteSpace(incoming.PhoneNumber) || string.IsNullOrWhiteSpace(incoming.Body))
                    {
                        continue;
                    }

                    var categoryGuess = InferCategoryFromText(incoming.Body);
                    var priorityGuess = InferPriorityFromText(incoming.Body);
                    var title = BuildAutoTitle(incoming.Body, categoryGuess);

                    var request = new WhatsAppCaseRequest
                    {
                        PhoneNumber = incoming.PhoneNumber,
                        Name = incoming.Name,
                        Title = title,
                        Description = incoming.Body,
                        Category = categoryGuess,
                        Priority = priorityGuess
                    };

                    var caseResponse = await CreateCaseInternalAsync(request, "WhatsAppWebhook");
                    await SendTrackingReplyAsync(incoming.PhoneNumber, caseResponse);

                    created.Add(new
                    {
                        phone = incoming.PhoneNumber,
                        trackingCode = caseResponse.TrackingCode,
                        caseId = caseResponse.CaseId
                    });
                }

                return Ok(new
                {
                    status = "processed",
                    createdCount = created.Count,
                    created
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp Webhook error");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Health check endpoint.
        /// GET /api/whatsapp/health
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new 
            { 
                status = "healthy", 
                service = "TeknoSOS WhatsApp API",
                whatsappNumber = "+355682030419",
                version = "1.0",
                timestamp = DateTime.UtcNow
            });
        }

        #region Helper Methods

        private bool ValidateApiKey(HttpRequest request)
        {
            // Check header first
            if (request.Headers.TryGetValue("X-API-Key", out var headerKey) && headerKey == ApiKey)
            {
                return true;
            }

            // Check query parameter
            if (request.Query.TryGetValue("api_key", out var queryKey) && queryKey == ApiKey)
            {
                return true;
            }

            // For development - allow without key if configured
            var allowAnonymous = _configuration.GetValue<bool>("WhatsApp:AllowAnonymous");
            return allowAnonymous;
        }

        private async Task<ApplicationUser> FindOrCreateUserByPhone(string phoneNumber, string? name)
        {
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            // Try to find existing user via UserManager (handles normalized fields)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone ||
                                          u.PhoneNumber == phoneNumber ||
                                          u.PhoneNumber == "+" + phoneNumber);

            if (user != null)
            {
                return user;
            }

            // Create new user for WhatsApp contact using UserManager so normalization runs
            var username = $"wa_{normalizedPhone.Replace("+", "").Replace(" ", "")}";
            var email = $"{username}@whatsapp.teknosos.app";

            var newUser = new ApplicationUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = normalizedPhone,
                FirstName = name ?? "WhatsApp",
                LastName = $"User {normalizedPhone}",
                Role = UserRole.Citizen,
                IsActive = true,
                EmailConfirmed = false,
                PhoneNumberConfirmed = true,
                RegistrationDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser);
            if (result.Succeeded)
            {
                try
                {
                    await _userManager.AddToRoleAsync(newUser, "Citizen");
                }
                catch { }

                _logger.LogInformation("WhatsApp API: Created new user for phone {PhoneNumber}", phoneNumber);
                return newUser;
            }

            // Fallback: if create failed due to concurrency, try to find again
            var concurrent = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone ||
                                          u.PhoneNumber == phoneNumber ||
                                          u.PhoneNumber == "+" + phoneNumber);
            if (concurrent != null) return concurrent;

            // Last resort: do not bypass Identity by writing directly to the Users table.
            // Return the concurrent user if found, otherwise stop with a clear error.
            _logger.LogWarning("WhatsApp API: Failed to create user via UserManager for phone {PhoneNumber}. Errors: {Errors}",
                phoneNumber,
                string.Join(';', result.Errors.Select(e => e.Description)));

            if (concurrent != null)
            {
                return concurrent;
            }

            throw new InvalidOperationException("Nuk u arrit krijimi i përdoruesit WhatsApp.");
        }

        private async Task<WhatsAppCaseResponse> CreateCaseInternalAsync(WhatsAppCaseRequest request, string source)
        {
            var user = await FindOrCreateUserByPhone(request.PhoneNumber, request.Name);

            var lastCode = await _context.ServiceRequests
                .OrderByDescending(s => s.Id)
                .Select(s => s.UniqueCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.StartsWith("DEF-") && int.TryParse(lastCode[4..], out int lastNum))
            {
                nextNumber = lastNum + 1;
            }

            var uniqueCode = $"DEF-{nextNumber:D6}";
            var category = ParseCategory(request.Category);
            var priority = ParsePriority(request.Priority);

            var serviceRequest = new ServiceRequest
            {
                UniqueCode = uniqueCode,
                CitizenId = user.Id,
                Title = request.Title ?? $"Raporti nga WhatsApp - {DateTime.Now:dd/MM/yyyy HH:mm}",
                Description = request.Description,
                Category = category,
                Status = ServiceRequestStatus.Created,
                Priority = priority,
                CasePriority = priority == ServiceRequestPriority.Emergency ? CasePriority.Emergency : CasePriority.Medium,
                CreatedDate = DateTime.UtcNow,
                Location = string.IsNullOrWhiteSpace(request.Address)
                    ? request.City
                    : string.IsNullOrWhiteSpace(request.City)
                        ? request.Address
                        : $"{request.Address}, {request.City}",
                ClientContactNumber = NormalizePhoneNumber(request.PhoneNumber),
                ClientLatitude = request.Latitude,
                ClientLongitude = request.Longitude
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            var notifiedCount = await _opportunityNotifier.NotifyNewOpportunityAsync(serviceRequest, source);

            _logger.LogInformation(
                "WhatsApp {Source}: Created case {UniqueCode} from {PhoneNumber}. Notified professionals: {Count}",
                source,
                uniqueCode,
                request.PhoneNumber,
                notifiedCount);

            _context.Notifications.Add(new Notification
            {
                RecipientId = user.Id,
                Title = "Raport i Ri nga WhatsApp",
                Message = $"Raporti juaj me kod {uniqueCode} u krijua me sukses nga WhatsApp.",
                Type = NotificationType.CaseCreated,
                ServiceRequestId = serviceRequest.Id,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return new WhatsAppCaseResponse
            {
                Success = true,
                Message = $"Raporti u krijua me sukses! Kodi juaj: {uniqueCode}",
                CaseId = serviceRequest.Id,
                TrackingCode = uniqueCode,
                Status = "Pending",
                CreatedAt = serviceRequest.CreatedDate,
                ViewUrl = $"/DefectDetails?id={serviceRequest.Id}"
            };
        }

        private async Task SendTrackingReplyAsync(string phoneNumber, WhatsAppCaseResponse caseResponse)
        {
            var reply =
                $"Raporti juaj u krijua me sukses. Kodi: {caseResponse.TrackingCode}. " +
                $"Status: /api/whatsapp/status/{caseResponse.TrackingCode}";

            await _whatsAppCloudSender.SendTextMessageAsync(phoneNumber, reply);
        }

        private static string BuildAutoTitle(string text, string? categoryGuess)
        {
            var categoryLabel = string.IsNullOrWhiteSpace(categoryGuess) ? "general" : categoryGuess;
            var clean = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
            if (clean.Length > 56)
            {
                clean = clean[..56] + "...";
            }

            return $"[{categoryLabel}] {clean}";
        }

        private static string? InferCategoryFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var lower = text.ToLowerInvariant();

            if (lower.Contains("elektr") || lower.Contains("rryme") || lower.Contains("prize")) return "elektrike";
            if (lower.Contains("hidraul") || lower.Contains("uji") || lower.Contains("rubinet") || lower.Contains("tub")) return "hidraulike";
            if (lower.Contains("ngrohje") || lower.Contains("ftohje") || lower.Contains("klim") || lower.Contains("hvac")) return "ngrohje-ftohje";
            if (lower.Contains("zdrukth") || lower.Contains("mobil") || lower.Contains("dru")) return "zdrukthtari";
            if (lower.Contains("pajisj") || lower.Contains("frigorifer") || lower.Contains("lavatrice")) return "pajisje";
            if (lower.Contains("gips")) return "gips";
            if (lower.Contains("pllaka") || lower.Contains("tile")) return "pllaka";
            if (lower.Contains("parket") || lower.Contains("parquet")) return "parket";
            if (lower.Contains("izolim") || lower.Contains("tarac") || lower.Contains("terrace")) return "izolim-tarace";
            if (lower.Contains("arkitekt")) return "arkitekt";
            if (lower.Contains("inxhin") || lower.Contains("inginier") || lower.Contains("engineer")) return "inxhinier";
            if (lower.Contains("mekanik")) return "mekanike";
            if (lower.Contains("it") || lower.Contains("kompjuter") || lower.Contains("rrjet")) return "teknologji";

            return "general";
        }

        private static string? InferPriorityFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var lower = text.ToLowerInvariant();

            if (lower.Contains("emergj") || lower.Contains("urgent") || lower.Contains("urgjent") || lower.Contains("rrezik"))
                return "emergency";

            if (lower.Contains("larte") || lower.Contains("high"))
                return "high";

            return "normal";
        }

        private static List<IncomingWhatsAppMessage> ExtractIncomingMessages(JsonElement payload)
        {
            var results = new List<IncomingWhatsAppMessage>();

            if (!payload.TryGetProperty("entry", out var entries) || entries.ValueKind != JsonValueKind.Array)
            {
                return results;
            }

            foreach (var entry in entries.EnumerateArray())
            {
                if (!entry.TryGetProperty("changes", out var changes) || changes.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var change in changes.EnumerateArray())
                {
                    if (!change.TryGetProperty("value", out var value))
                    {
                        continue;
                    }

                    var contactNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (value.TryGetProperty("contacts", out var contacts) && contacts.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var contact in contacts.EnumerateArray())
                        {
                            var waId = contact.TryGetProperty("wa_id", out var waIdEl) ? waIdEl.GetString() : null;
                            var name = contact.TryGetProperty("profile", out var profileEl) && profileEl.TryGetProperty("name", out var nameEl)
                                ? nameEl.GetString()
                                : null;

                            if (!string.IsNullOrWhiteSpace(waId) && !string.IsNullOrWhiteSpace(name))
                            {
                                contactNames[waId] = name;
                            }
                        }
                    }

                    if (!value.TryGetProperty("messages", out var messages) || messages.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var message in messages.EnumerateArray())
                    {
                        var from = message.TryGetProperty("from", out var fromEl) ? fromEl.GetString() : null;
                        var type = message.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null;

                        if (!string.Equals(type, "text", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var body = message.TryGetProperty("text", out var textEl) && textEl.TryGetProperty("body", out var bodyEl)
                            ? bodyEl.GetString()
                            : null;

                        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(body))
                        {
                            continue;
                        }

                        contactNames.TryGetValue(from, out var name);
                        results.Add(new IncomingWhatsAppMessage
                        {
                            PhoneNumber = from,
                            Name = name,
                            Body = body.Trim()
                        });
                    }
                }
            }

            return results;
        }

        private sealed class IncomingWhatsAppMessage
        {
            public string PhoneNumber { get; set; } = string.Empty;
            public string? Name { get; set; }
            public string Body { get; set; } = string.Empty;
        }

        private static string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            // Remove spaces and dashes
            phone = phone.Replace(" ", "").Replace("-", "");

            // Add country code if missing
            if (phone.StartsWith("06") || phone.StartsWith("6"))
            {
                phone = "+355" + (phone.StartsWith("0") ? phone.Substring(1) : phone);
            }
            else if (!phone.StartsWith("+"))
            {
                phone = "+" + phone;
            }

            return phone;
        }

        private static ServiceCategory ParseCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return ServiceCategory.General;

            return category.ToLower() switch
            {
                "elektrike" or "electrical" or "elektrik" => ServiceCategory.Electrical,
                "hidraulike" or "plumbing" or "hidraulik" or "uji" => ServiceCategory.Plumbing,
                "mekanike" or "mechanical" or "mekanik" => ServiceCategory.Mechanical,
                "ngrohje" or "ftohje" or "hvac" or "klimatizim" => ServiceCategory.HVAC,
                "teknologji" or "it" or "kompjuter" or "technology" => ServiceCategory.ITTechnology,
                "zdrukthtari" or "carpentry" or "mobilje" => ServiceCategory.Carpentry,
                "pajisje" or "appliance" or "elektroshtepiake" => ServiceCategory.Appliance,
                "gips" or "gypsum" => ServiceCategory.Gypsum,
                "pllaka" or "tiles" or "tile" => ServiceCategory.Tiles,
                "parket" or "parquet" => ServiceCategory.Parquet,
                "izolim tarace" or "izolim-tarace" or "terrace insulation" => ServiceCategory.TerraceInsulation,
                "arkitekt" or "architect" => ServiceCategory.Architect,
                "inxhinier" or "inginier" or "engineer" => ServiceCategory.Engineer,
                _ => ServiceCategory.General
            };
        }

        private static ServiceRequestPriority ParsePriority(string? priority)
        {
            if (string.IsNullOrWhiteSpace(priority))
                return ServiceRequestPriority.Normal;

            return priority.ToLower() switch
            {
                "urgent" or "urgjent" or "emergency" or "emergjent" or "emergjence" => ServiceRequestPriority.Emergency,
                "high" or "larte" or "i larte" => ServiceRequestPriority.High,
                "low" or "ulet" or "i ulet" => ServiceRequestPriority.Normal, // No Low priority exists, use Normal
                _ => ServiceRequestPriority.Normal
            };
        }

        private static string GetStatusText(ServiceRequestStatus status)
        {
            return status switch
            {
                ServiceRequestStatus.Created => "Ne Pritje",
                ServiceRequestStatus.Matched => "I Caktuar",
                ServiceRequestStatus.InProgress => "Ne Progres",
                ServiceRequestStatus.Completed => "I Perfunduar",
                ServiceRequestStatus.Cancelled => "I Anuluar",
                _ => status.ToString()
            };
        }

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// Request model for creating a case from WhatsApp.
    /// </summary>
    public class WhatsAppCaseRequest
    {
        /// <summary>
        /// WhatsApp phone number (required). Example: +355682030419
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Contact name (optional).
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Case title (optional). Auto-generated if not provided.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Description of the issue (required).
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category: elektrike, hidraulike, mekanike, ngrohje, teknologji, zdrukthtari, pajisje, general
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Priority: normal, high/larte, urgent/emergjent
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Address (optional).
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// City (optional).
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// GPS Latitude (optional).
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// GPS Longitude (optional).
        /// </summary>
        public decimal? Longitude { get; set; }
    }

    /// <summary>
    /// Response model for case creation.
    /// </summary>
    public class WhatsAppCaseResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CaseId { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ViewUrl { get; set; } = string.Empty;
    }

    #endregion
}
