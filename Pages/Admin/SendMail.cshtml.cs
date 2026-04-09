using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SendMailModel : PageModel
    {
        private readonly IEmailSender _emailSender;
        public SendMailModel(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [BindProperty]
        public string RecipientEmail { get; set; } = string.Empty;
        [BindProperty]
        public string Subject { get; set; } = string.Empty;
        [BindProperty]
        public string Body { get; set; } = string.Empty;

        public string SuccessMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        [BindProperty]
        public bool IsBodyHtml { get; set; } = false;

        public void OnGet(string email = "")
        {
            RecipientEmail = email;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(RecipientEmail) || string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(Body))
            {
                ErrorMessage = "Të gjitha fushat janë të detyrueshme.";
                return Page();
            }
            try
            {
                // Use HTML if checked, otherwise plain text
                if (_emailSender is TeknoSOS.WebApp.Services.SmtpEmailSender smtpSender)
                {
                    await smtpSender.SendEmailAsync(RecipientEmail, Subject, Body, IsBodyHtml);
                }
                else
                {
                    await _emailSender.SendEmailAsync(RecipientEmail, Subject, Body);
                }
                SuccessMessage = $"Email-i u dërgua me sukses te {RecipientEmail}.";
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Dërgimi i email-it dështoi: {ex.Message}";
            }
            return Page();
        }
    }
}
