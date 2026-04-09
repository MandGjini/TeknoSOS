using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class NotificationTemplatesModel : PageModel
    {
        private readonly INotificationTemplateService _templateService;

        public NotificationTemplatesModel(INotificationTemplateService templateService)
        {
            _templateService = templateService;
        }

        public List<NotificationTemplate> EmailTemplates { get; set; } = new();
        public List<NotificationTemplate> SmsTemplates { get; set; } = new();
        public NotificationTemplate? EditingTemplate { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public int? EditId { get; set; }

        public async Task OnGetAsync(int? editId = null)
        {
            EmailTemplates = await _templateService.GetTemplatesByTypeAsync("Email");
            SmsTemplates = await _templateService.GetTemplatesByTypeAsync("SMS");

            if (editId.HasValue)
            {
                var all = await _templateService.GetAllTemplatesAsync();
                EditingTemplate = all.FirstOrDefault(t => t.Id == editId.Value);
            }
        }

        public async Task<IActionResult> OnPostSaveTemplateAsync(
            int id, 
            string templateKey,
            string displayName, 
            string? description,
            string templateType,
            string? subject,
            string content,
            string? category,
            bool isEnabled,
            int sortOrder)
        {
            var template = new NotificationTemplate
            {
                Id = id,
                TemplateKey = templateKey,
                DisplayName = displayName,
                Description = description,
                TemplateType = templateType,
                Subject = subject,
                Content = content,
                Category = category,
                IsEnabled = isEnabled,
                SortOrder = sortOrder
            };

            await _templateService.SaveTemplateAsync(template);
            SuccessMessage = $"Template '{displayName}' u ruajt me sukses!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleEnabledAsync(int id)
        {
            var templates = await _templateService.GetAllTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.Id == id);
            
            if (template != null)
            {
                template.IsEnabled = !template.IsEnabled;
                await _templateService.SaveTemplateAsync(template);
                SuccessMessage = $"Template '{template.DisplayName}' u {(template.IsEnabled ? "aktivizua" : "çaktivizua")}!";
            }
            
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateNewAsync(string templateType)
        {
            var template = new NotificationTemplate
            {
                TemplateKey = $"custom_{Guid.NewGuid().ToString("N")[..8]}",
                DisplayName = templateType == "SMS" ? "SMS i Ri" : "Email i Ri",
                Description = "Template i krijuar nga admini",
                TemplateType = templateType,
                Subject = templateType == "Email" ? "Subjekt i Ri" : null,
                Content = templateType == "Email" 
                    ? "<html><body><p>Përmbajtja e email-it këtu. Përdorni {{FirstName}}, {{LastName}}, {{Email}} si placeholders.</p></body></html>"
                    : "Mesazhi SMS ketu. Perdorni {{FirstName}}, {{LastName}} si placeholders.",
                Category = "Custom",
                IsEnabled = false,
                SortOrder = 99
            };

            await _templateService.SaveTemplateAsync(template);
            SuccessMessage = $"Template i ri u krijua! Editoni tani.";
            
            // Get the ID of the newly created template
            var all = await _templateService.GetAllTemplatesAsync();
            var newTemplate = all.FirstOrDefault(t => t.TemplateKey == template.TemplateKey);
            
            return RedirectToPage(new { editId = newTemplate?.Id });
        }
    }
}
