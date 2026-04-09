using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeknoSOS.WebApp.Services;
using TeknoSOS.WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace TeknoSOS.WebApp.Controllers.Admin
{
    [Route("admin/test-email")]
    [Authorize(Roles = "Admin")]
    public class AdminTestEmailController : Controller
    {
        private readonly INotificationTemplateService _templateService;
        private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;

        public AdminTestEmailController(INotificationTemplateService templateService, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender, ApplicationDbContext db)
        {
            _templateService = templateService;
            _emailSender = emailSender;
            _db = db;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> SendTest(string email, string templateKey = "citizen_welcome")
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            var placeholders = new Dictionary<string, string>
            {
                { "FirstName", user?.FirstName ?? "Test" },
                { "LastName", user?.LastName ?? "User" },
                { "Email", email }
            };
            var (subject, body) = await _templateService.RenderEmailTemplateAsync(templateKey, placeholders);
            if (string.IsNullOrWhiteSpace(body))
                return Content($"Template '{templateKey}' nuk u gjet ose është bosh.");
            await _emailSender.SendEmailAsync(email, subject, body);
            return Content($"Email test u dërgua te {email} me template '{templateKey}'.");
        }
    }
}
