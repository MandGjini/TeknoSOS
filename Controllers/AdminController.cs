using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TeknoSOS.WebApp.Models;

namespace TeknoSOS.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult SendMail(string email)
        {
            var model = new SendMailViewModel { To = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMail(SendMailViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var msg = new MailMessage("noreply@teknosos.app", model.To, model.Subject, model.Body)
            {
                IsBodyHtml = true
            };
            using (var client = new SmtpClient("business177.web-hosting.com", 587)
            {
                Credentials = new NetworkCredential("noreply@teknosos.app", "Celine@021021!"),
                EnableSsl = true
            })
            {
                await client.SendMailAsync(msg);
            }
            ViewBag.Success = "Email-i u dërgua me sukses.";
            return View(new SendMailViewModel { To = model.To });
        }
    }
}
