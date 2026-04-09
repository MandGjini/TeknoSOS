using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILocalizationService _localizer;

        public ChatModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILocalizationService localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }

        public ServiceRequest? Defect { get; set; }
        public List<Message> Messages { get; set; } = new();
        public ApplicationUser? OtherUser { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? defectId)
        {
            // If no defectId provided, redirect to Messages page
            if (!defectId.HasValue)
            {
                return RedirectToPage("/Messages");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            CurrentUserId = user.Id;

            Defect = await _context.ServiceRequests
                .Include(sr => sr.Citizen)
                .Include(sr => sr.Professional)
                .FirstOrDefaultAsync(sr => sr.Id == defectId.Value);

            if (Defect == null) return NotFound();

            // Determine the other user in the conversation
            if (Defect.CitizenId == user.Id)
            {
                OtherUser = Defect.Professional;
            }
            else if (Defect.ProfessionalId == user.Id)
            {
                OtherUser = Defect.Citizen;
            }
            else
            {
                // Admin can view any chat
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin) return Forbid();
                OtherUser = Defect.Citizen;
            }

            Messages = await _context.Messages
                .Where(m => m.ServiceRequestId == defectId.Value)
                .OrderBy(m => m.CreatedDate)
                .Include(m => m.Sender)
                .ToListAsync();

            // Mark unread messages as read
            var unreadMessages = Messages.Where(m => m.ReceiverId == user.Id && m.ReadDate == null).ToList();
            foreach (var msg in unreadMessages)
            {
                msg.ReadDate = DateTime.UtcNow;
                msg.Status = MessageStatus.Read;
            }
            if (unreadMessages.Any())
                await _context.SaveChangesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(int defectId)
        {
            // Verify user is participant in this conversation
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return new JsonResult(new { success = false, error = "Unauthorized" }) { StatusCode = 401 };

            var defect = await _context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Id == defectId);

            if (defect == null)
                return new JsonResult(new { success = false, error = "Not found" }) { StatusCode = 404 };

            // Check if user is participant or admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (defect.CitizenId != user.Id && defect.ProfessionalId != user.Id && !isAdmin)
                return new JsonResult(new { success = false, error = "Forbidden" }) { StatusCode = 403 };

            if (Request.Form.Files.Count == 0)
                return new JsonResult(new { success = false, error = "No file" });

            var file = Request.Form.Files[0];
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedTypes.Contains(ext))
                return new JsonResult(new { success = false, error = "File type not allowed" });

            if (file.Length > 10 * 1024 * 1024) // 10MB max
                return new JsonResult(new { success = false, error = "File too large (max 10MB)" });

            // Validate file content matches extension (magic bytes check)
            var isValidContent = await ValidateFileContentAsync(file, ext);
            if (!isValidContent)
                return new JsonResult(new { success = false, error = "Invalid file content" });

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
            Directory.CreateDirectory(uploadsDir);

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/chat/{uniqueName}";
            var messageType = ext is ".jpg" or ".jpeg" or ".png" or ".gif" ? "image" : "document";

            return new JsonResult(new
            {
                success = true,
                url = fileUrl,
                fileName = file.FileName,
                messageType = messageType
            });
        }

        private static async Task<bool> ValidateFileContentAsync(IFormFile file, string extension)
        {
            // Magic bytes validation for common file types
            var signatures = new Dictionary<string, byte[][]>
            {
                { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
                { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
                { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
                { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
                { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
                { ".doc", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } } },
                { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }
            };

            if (!signatures.TryGetValue(extension, out var validSignatures))
                return true; // Unknown extension, skip validation

            using var stream = file.OpenReadStream();
            var headerBytes = new byte[8];
            await stream.ReadAsync(headerBytes.AsMemory(0, Math.Min((int)file.Length, 8)));

            return validSignatures.Any(sig => 
                headerBytes.Take(sig.Length).SequenceEqual(sig));
        }
    }
}
