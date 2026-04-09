using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages.Messages
{
    [Authorize]
    public class OpenMessageModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpenMessageModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public string RecipientRole { get; set; } = "Admin";

        [BindProperty]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        public new string Content { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Content) || string.IsNullOrWhiteSpace(Subject))
            {
                ModelState.AddModelError(string.Empty, "Subjekti dhe mesazhi duhet të plotësohen.");
                return Page();
            }

            var senderId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(senderId))
            {
                return Challenge();
            }

            var recipients = await _userManager.GetUsersInRoleAsync(RecipientRole);
            if (recipients == null || recipients.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Nuk u gjet asnjë përdorues për rolin e zgjedhur.");
                return Page();
            }

            foreach (var r in recipients)
            {
                var msg = new Message
                {
                    SenderId = senderId,
                    ReceiverId = r.Id,
                    Content = Subject + "\n\n" + Content,
                    MessageType = "text",
                    ServiceRequestId = null,
                    Status = MessageStatus.Sent,
                    CreatedDate = DateTime.UtcNow,
                    IsArchived = false
                };
                _db.Messages.Add(msg);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Mesazhi u dërgua te administrata.";
            return RedirectToPage("/Index");
        }
    }
}
