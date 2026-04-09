using Microsoft.AspNetCore.Mvc;

namespace TeknoSOS.WebApp.Controllers
{
    [Route("api/[controller]")]
    public class LanguageController : Controller
    {
        [HttpGet("set/{lang}")]
        public IActionResult SetLanguage(string lang)
        {
            var validLangs = new[] { "sq", "en", "it", "de", "fr" };
            if (!validLangs.Contains(lang)) lang = "sq";
            
            Response.Cookies.Append("teknosos_lang", lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
            
            var returnUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "/";
            
            return Redirect(returnUrl);
        }
    }
}
