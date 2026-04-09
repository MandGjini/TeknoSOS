using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages
{
    [Authorize]
    public class StartChatModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StartChatModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnPostAsync(string targetId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (string.IsNullOrEmpty(targetId)) return BadRequest();

            var tech = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetId || u.DisplayUsername == targetId);
            if (tech == null) return NotFound();

            if (tech.Id == user.Id) return RedirectToPage("/TechnicianProfile", new { id = tech.DisplayUsername ?? tech.Id });

            var existing = await _context.ServiceRequests
                .Where(sr => (sr.CitizenId == user.Id && sr.ProfessionalId == tech.Id) || (sr.CitizenId == tech.Id && sr.ProfessionalId == user.Id))
                .OrderByDescending(sr => sr.CreatedDate)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return RedirectToPage("/Chat", new { defectId = existing.Id });
            }

            var sr = new ServiceRequest
            {
                CitizenId = user.Id,
                ProfessionalId = tech.Id,
                UniqueCode = await GenerateUniqueCodeAsync(),
                Title = $"Bisedë me {tech.GetFullName()}",
                Description = "Bisedë iniciuar nga profili",
                Category = ServiceCategory.General,
                Status = ServiceRequestStatus.InProgress,
                Priority = ServiceRequestPriority.Normal,
                CasePriority = CasePriority.Minimal,
                CreatedDate = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(sr);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Chat", new { defectId = sr.Id });
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            var lastRequest = await _context.ServiceRequests
                .OrderByDescending(sr => sr.Id)
                .Select(sr => sr.UniqueCode)
                .FirstOrDefaultAsync();

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