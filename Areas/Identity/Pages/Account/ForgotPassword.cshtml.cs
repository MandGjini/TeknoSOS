using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            ILogger<ForgotPasswordModel> logger,
            Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool EmailSent { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email është i detyrueshëm")]
            [EmailAddress(ErrorMessage = "Ju lutem vendosni një email të vlefshëm")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            EmailSent = false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    // For security reasons, show the same message regardless
                    EmailSent = true;
                    return Page();
                }

                // Generate the reset token
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // TODO: Send email with the reset link
                // Send the password reset email
                var safeUrl = callbackUrl ?? string.Empty;
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Rivendosni Fjalëkalimin - TeknoSOS",
                    $"Rivendosni fjalëkalimin duke klikuar <a href='{HtmlEncoder.Default.Encode(safeUrl)}'>këtu</a>.");

                EmailSent = true;
                return Page();
            }

            return Page();
        }
    }
}
