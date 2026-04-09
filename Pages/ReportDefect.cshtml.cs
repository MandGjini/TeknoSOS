using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Hubs;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    [Authorize]
    public class ReportDefectModel : PageModel
    {
        private readonly ILogger<ReportDefectModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILocalizationService _localizer;
        private readonly IProfessionalOpportunityNotifier _opportunityNotifier;

        public ReportDefectModel(
            ILogger<ReportDefectModel> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext,
            ILocalizationService localizer,
            IProfessionalOpportunityNotifier opportunityNotifier)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _localizer = localizer;
            _opportunityNotifier = opportunityNotifier;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// List of nearby technicians that match the selected category (populated on emergency)
        /// </summary>
        public List<NearbyTechnicianInfo> NearbyTechnicians { get; set; } = new();

        public class InputModel
        {
            [Display(Name = "Kategoria")]
            public string? Category { get; set; }

            [Display(Name = "Prioriteti")]
            public string Priority { get; set; } = "Normal";

            [StringLength(100, ErrorMessage = "Titulli nuk mund te kaloje 100 karaktere")]
            [Display(Name = "Titulli")]
            public string Title { get; set; } = string.Empty;

            [StringLength(2000, ErrorMessage = "Pershkrimi nuk mund te kaloje 2000 karaktere")]
            [Display(Name = "Pershkrimi")]
            public string Description { get; set; } = string.Empty;

            [Display(Name = "Adresa")]
            public string? Address { get; set; }

            [Display(Name = "Qyteti")]
            public string? City { get; set; }

            [Display(Name = "Detaje Lokacioni")]
            [StringLength(500)]
            public string? LocationDetails { get; set; }

            [Display(Name = "GPS Latitude")]
            public decimal? Latitude { get; set; }

            [Display(Name = "GPS Longitude")]
            public decimal? Longitude { get; set; }

            [Display(Name = "Numri i telefonit")]
            public string? ContactPhone { get; set; }

            [Display(Name = "Njoftime me email")]
            public bool EmailNotifications { get; set; } = true;

            [Display(Name = "Njoftime me SMS")]
            public bool SmsNotifications { get; set; } = false;

            public List<IFormFile> Photos { get; set; } = new();
        }

        public class NearbyTechnicianInfo
        {
            public string Id { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public double DistanceKm { get; set; }
            public decimal? Rating { get; set; }
            public string? City { get; set; }
        }

        /// <summary>
        /// All Albanian cities for the dropdown
        /// </summary>
        public List<string> Cities { get; set; } = new()
        {
            "Tirane", "Durres", "Vlore", "Elbasan", "Shkoder", "Korce",
            "Fier", "Berat", "Lushnje", "Pogradec", "Kavaje", "Gjirokaster",
            "Sarande", "Peshkopi", "Kukes", "Lezhe", "Permet", "Tepelene",
            "Gramsh", "Librazhd", "Bulqize", "Kruje", "Lac", "Burrel",
            "Puke", "Bajze", "Rreshen", "Corovode"
        };

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Report defect page accessed by user: {User}", User.Identity?.Name);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // GPS is optional - helps but not required
            // if (!Input.Latitude.HasValue || !Input.Longitude.HasValue)
            // {
            //     ErrorMessage = "GPS eshte i detyrueshem per raportimin e defektit. Ju lutem aktivizoni GPS.";
            //     return Page();
            // }

            try
            {
                var serviceRequest = await SubmitDefectReport();

                // Handle photo uploads
                if (Input.Photos?.Count > 0)
                {
                    var photoUrl = await SavePhotos(Input.Photos);
                    if (!string.IsNullOrEmpty(photoUrl))
                    {
                        serviceRequest.PhotoUrl = photoUrl;
                        await _context.SaveChangesAsync();
                    }
                }

                // Category-based routing: notify matching technicians
                var notifiedCount = await _opportunityNotifier.NotifyNewOpportunityAsync(serviceRequest, "Web");

                // Emergency: send immediate SignalR push
                if (serviceRequest.Priority == ServiceRequestPriority.Emergency)
                {
                    await SendEmergencyNotifications(serviceRequest, notifiedCount);
                }

                SuccessMessage = $"Defekti u raportua me sukses! Kodi i gjurmimit: {serviceRequest.UniqueCode}. " +
                    $"{notifiedCount} teknike te kategorise perkatese u njoftuan." +
                    (serviceRequest.Priority == ServiceRequestPriority.Emergency ? " EMERGJENCE: Njoftim i menjehershem u dergua!" : "");

                _logger.LogInformation("Defect {Code} created by {User} - Category: {Category}, Priority: {Priority}, Notified: {Count} technicians",
                    serviceRequest.UniqueCode, User.Identity?.Name, serviceRequest.Category, serviceRequest.Priority, notifiedCount);

                return RedirectToPage("/DefectList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting defect report for user: {User}", User.Identity?.Name);
                ErrorMessage = "Gabim gjate raportimit. Ju lutem provoni perseri.";
                return Page();
            }
        }

        private async Task<ServiceRequest> SubmitDefectReport()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("User not found.");

            var category = MapCategory(Input.Category ?? "general");
            var priority = MapPriority(Input.Priority ?? "normal");

            // Use defaults if title/description are empty
            var title = string.IsNullOrWhiteSpace(Input.Title) ? "Defekt pa titull" : Input.Title.Trim();
            var description = string.IsNullOrWhiteSpace(Input.Description) ? "Pa pershkrim" : Input.Description.Trim();

            var serviceRequest = new ServiceRequest
            {
                CitizenId = userId,
                UniqueCode = await GenerateUniqueCode(),
                Title = title,
                Description = description,
                Category = category,
                Status = ServiceRequestStatus.Created,
                Priority = priority,
                CasePriority = priority == ServiceRequestPriority.Emergency ? CasePriority.Emergency : 
                               priority == ServiceRequestPriority.High ? CasePriority.Medium : CasePriority.Minimal,
                Location = $"{Input.Address}, {Input.City}" +
                    (string.IsNullOrEmpty(Input.LocationDetails) ? "" : $" ({Input.LocationDetails})"),
                ClientLatitude = Input.Latitude,
                ClientLongitude = Input.Longitude,
                ClientContactNumber = Input.ContactPhone,
                CreatedDate = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(serviceRequest);

            // Create audit log
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "ServiceRequest.Created",
                Entity = "ServiceRequest",
                EntityId = serviceRequest.Id,
                NewValue = $"[{serviceRequest.UniqueCode}] {title} [{category}] Priority: {priority}",
                CreatedDate = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
            });

            await _context.SaveChangesAsync();
            return serviceRequest;
        }

        /// <summary>
        /// Notify technicians whose specialties match the defect category.
        /// Calculates distance from defect to each technician and stores nearest distance.
        /// </summary>
        private async Task<int> NotifyMatchingTechnicians(ServiceRequest serviceRequest)
        {
            // Find all active Professional users with matching specialty
            var matchingTechnicians = await _context.Users
                .Where(u => u.IsActive && u.IsAvailableForWork)
                .Where(u => u.Specialties.Any(s => s.Category == serviceRequest.Category))
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Latitude,
                    u.Longitude,
                    u.ServiceRadiusKm
                })
                .ToListAsync();

            if (!matchingTechnicians.Any())
            {
                // Fallback: notify all professionals if no specialty match
                var allPros = await _userManager.GetUsersInRoleAsync("Professional");
                matchingTechnicians = allPros
                    .Where(u => u.IsActive)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Latitude,
                        u.Longitude,
                        u.ServiceRadiusKm
                    })
                    .ToList();
            }

            double nearestDistance = double.MaxValue;
            int notified = 0;

            foreach (var tech in matchingTechnicians)
            {
                double distance = 0;
                if (serviceRequest.ClientLatitude.HasValue && serviceRequest.ClientLongitude.HasValue &&
                    tech.Latitude.HasValue && tech.Longitude.HasValue)
                {
                    distance = CalculateDistanceKm(
                        (double)serviceRequest.ClientLatitude.Value, (double)serviceRequest.ClientLongitude.Value,
                        (double)tech.Latitude.Value, (double)tech.Longitude.Value);

                    // Skip if technician is outside their service radius (if set)
                    if (tech.ServiceRadiusKm.HasValue && distance > tech.ServiceRadiusKm.Value)
                        continue;

                    if (distance < nearestDistance)
                        nearestDistance = distance;
                }

                // Create notification
                var notification = new Notification
                {
                    RecipientId = tech.Id,
                    ServiceRequestId = serviceRequest.Id,
                    Type = serviceRequest.Priority == ServiceRequestPriority.Emergency
                        ? NotificationType.EmergencyDefect
                        : NotificationType.CaseCreated,
                    Title = serviceRequest.Priority == ServiceRequestPriority.Emergency
                        ? $"EMERGJENCE: {serviceRequest.Title}"
                        : $"Defekt i ri: {serviceRequest.Title}",
                    Message = $"Kodi: {serviceRequest.UniqueCode} | Kategoria: {serviceRequest.Category} | " +
                        $"Prioriteti: {serviceRequest.Priority}" +
                        (distance > 0 ? $" | Distanca: {distance:F1} km" : ""),
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                notified++;
            }

            // Store nearest technician distance
            if (nearestDistance < double.MaxValue)
            {
                serviceRequest.NearestTechnicianDistance = (decimal)nearestDistance;
            }

            await _context.SaveChangesAsync();
            return notified;
        }

        /// <summary>
        /// Send real-time emergency notifications via SignalR to all connected users
        /// </summary>
        private async Task SendEmergencyNotifications(ServiceRequest serviceRequest, int notifiedCount)
        {
            await _hubContext.Clients.All.SendAsync("EmergencyDefect", new
            {
                defectId = serviceRequest.Id,
                uniqueCode = serviceRequest.UniqueCode,
                title = serviceRequest.Title,
                category = serviceRequest.Category.ToString(),
                priority = "Emergency",
                latitude = serviceRequest.ClientLatitude,
                longitude = serviceRequest.ClientLongitude,
                location = serviceRequest.Location,
                nearestDistance = serviceRequest.NearestTechnicianDistance,
                notifiedTechnicians = notifiedCount,
                timestamp = serviceRequest.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
            });

            // Also notify admin group
            await _hubContext.Clients.Group("admin-chat-monitor").SendAsync("EmergencyAlert", new
            {
                defectId = serviceRequest.Id,
                uniqueCode = serviceRequest.UniqueCode,
                title = serviceRequest.Title,
                category = serviceRequest.Category.ToString(),
                location = serviceRequest.Location,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Save uploaded photos and return the URL of the first photo
        /// </summary>
        private async Task<string?> SavePhotos(List<IFormFile> photos)
        {
            if (photos == null || photos.Count == 0) return null;

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "defects");
            Directory.CreateDirectory(uploadsDir);

            string? firstPhotoUrl = null;
            int count = 0;

            foreach (var photo in photos.Take(5)) // Max 5 photos
            {
                if (photo.Length > 10 * 1024 * 1024) continue; // Max 10MB per file

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(photo.FileName).ToLower();
                if (!allowedExtensions.Contains(ext)) continue;

                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                if (count == 0) firstPhotoUrl = $"/uploads/defects/{uniqueName}";
                count++;
            }

            return firstPhotoUrl;
        }

        /// <summary>
        /// Calculate distance between two GPS coordinates using the Haversine formula
        /// </summary>
        public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;

        /// <summary>
        /// Map category string from form to ServiceCategory enum
        /// </summary>
        private ServiceCategory MapCategory(string category) => category?.ToLower() switch
        {
            "electrical" or "elektrike" => ServiceCategory.Electrical,
            "plumbing" or "hidraulike" => ServiceCategory.Plumbing,
            "hvac" or "ngrohje-ftohje" => ServiceCategory.HVAC,
            "mechanical" or "mekanike" => ServiceCategory.Mechanical,
            "it" or "teknologji" => ServiceCategory.ITTechnology,
            "carpentry" or "zdrukthtari" => ServiceCategory.Carpentry,
            "appliance" or "pajisje" => ServiceCategory.Appliance,
            "gips" or "gypsum" => ServiceCategory.Gypsum,
            "pllaka" or "tiles" or "tile" => ServiceCategory.Tiles,
            "parket" or "parquet" => ServiceCategory.Parquet,
            "izolim-tarace" or "izolimtarace" or "terrace-insulation" => ServiceCategory.TerraceInsulation,
            "arkitekt" or "architect" or "arkitekture" => ServiceCategory.Architect,
            "inxhinier" or "inginier" or "engineer" => ServiceCategory.Engineer,
            _ => ServiceCategory.General
        };

        /// <summary>
        /// Map priority string from form to ServiceRequestPriority enum
        /// </summary>
        private ServiceRequestPriority MapPriority(string priority) => priority?.ToLower() switch
        {
            "high" or "i-larte" => ServiceRequestPriority.High,
            "emergency" or "emergjent" => ServiceRequestPriority.Emergency,
            _ => ServiceRequestPriority.Normal
        };

        /// <summary>
        /// Generates a unique tracking code in format DEF-XXXXXX
        /// </summary>
        private async Task<string> GenerateUniqueCode()
        {
            var lastRequest = _context.ServiceRequests
                .OrderByDescending(sr => sr.Id)
                .Select(sr => sr.UniqueCode)
                .FirstOrDefault();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastRequest) && lastRequest.StartsWith("DEF-"))
            {
                if (int.TryParse(lastRequest.Substring(4), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"DEF-{nextNumber:D6}";
        }
    }
}