using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    public interface INotificationTemplateService
    {
        Task<NotificationTemplate?> GetTemplateAsync(string templateKey);
        Task<List<NotificationTemplate>> GetAllTemplatesAsync();
        Task<List<NotificationTemplate>> GetTemplatesByCategoryAsync(string category);
        Task<List<NotificationTemplate>> GetTemplatesByTypeAsync(string templateType);
        Task SaveTemplateAsync(NotificationTemplate template);
        Task<string> RenderTemplateAsync(string templateKey, Dictionary<string, string> placeholders);
        Task<(string Subject, string Body)> RenderEmailTemplateAsync(string templateKey, Dictionary<string, string> placeholders);
        Task SeedDefaultTemplatesAsync();
    }

    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly ApplicationDbContext _db;

        public NotificationTemplateService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<NotificationTemplate?> GetTemplateAsync(string templateKey)
        {
            return await _db.NotificationTemplates
                .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.IsEnabled);
        }

        public async Task<List<NotificationTemplate>> GetAllTemplatesAsync()
        {
            return await _db.NotificationTemplates
                .OrderBy(t => t.Category)
                .ThenBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<List<NotificationTemplate>> GetTemplatesByCategoryAsync(string category)
        {
            return await _db.NotificationTemplates
                .Where(t => t.Category == category)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<List<NotificationTemplate>> GetTemplatesByTypeAsync(string templateType)
        {
            return await _db.NotificationTemplates
                .Where(t => t.TemplateType == templateType)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task SaveTemplateAsync(NotificationTemplate template)
        {
            var existing = await _db.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == template.Id || t.TemplateKey == template.TemplateKey);

            if (existing != null)
            {
                existing.DisplayName = template.DisplayName;
                existing.Description = template.Description;
                existing.Subject = template.Subject;
                existing.Content = template.Content;
                existing.IsEnabled = template.IsEnabled;
                existing.Category = template.Category;
                existing.SortOrder = template.SortOrder;
                existing.LastModifiedDate = DateTime.UtcNow;
            }
            else
            {
                template.CreatedDate = DateTime.UtcNow;
                _db.NotificationTemplates.Add(template);
            }

            await _db.SaveChangesAsync();
        }

        public async Task<string> RenderTemplateAsync(string templateKey, Dictionary<string, string> placeholders)
        {
            var template = await GetTemplateAsync(templateKey);
            if (template == null) return string.Empty;

            return RenderContent(template.Content, placeholders);
        }

        public async Task<(string Subject, string Body)> RenderEmailTemplateAsync(string templateKey, Dictionary<string, string> placeholders)
        {
            var template = await GetTemplateAsync(templateKey);
            if (template == null) return ("", "");

            var subject = RenderContent(template.Subject ?? "", placeholders);
            var body = RenderContent(template.Content, placeholders);

            return (subject, body);
        }

        private static string RenderContent(string content, Dictionary<string, string> placeholders)
        {
            if (string.IsNullOrEmpty(content)) return content;

            foreach (var kvp in placeholders)
            {
                content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            }

            return content;
        }

        public async Task SeedDefaultTemplatesAsync()
        {
            if (await _db.NotificationTemplates.AnyAsync()) return;

            var defaults = new List<NotificationTemplate>
            {
                // ========== REGISTRATION EMAILS ==========
                new()
                {
                    TemplateKey = "technician_welcome",
                    DisplayName = "Email Mirëseardhje Teknik",
                    Description = "Dërgohet tek teknikët e rinj pas regjistrimit. Shpjegon procesin e verifikimit.",
                    TemplateType = "Email",
                    Category = "Regjistrimi",
                    Subject = "Mirë se vini në TeknoSOS - Regjistrimi si Teknik",
                    Content = @"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #2563eb;'>Mirë se vini në TeknoSOS, {{FirstName}}!</h2>
    <p>Faleminderit që u regjistruat si teknik në platformën tonë.</p>
    
    <div style='background-color: #fef3c7; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <strong>⏳ Llogaria juaj është në pritje për verifikim</strong>
        <p style='margin: 10px 0 0 0;'>Stafi ynë do të shqyrtojë profilin tuaj dhe do t'ju njoftojë me email kur të aktivizohet llogaria.</p>
    </div>
    
    <p><strong>Çfarë pritet:</strong></p>
    <ul>
        <li>Verifikimi zakonisht zgjat 24-48 orë</li>
        <li>Do të merrni email kur llogaria të aktivizohet</li>
        <li>Pas aktivizimit, mund të filloni të pranoni punë</li>
    </ul>
    
    <p style='color: #6b7280; margin-top: 20px;'>Nëse keni pyetje, na kontaktoni në support@teknosos.app</p>
    <p style='color: #6b7280;'>— Ekipi TeknoSOS</p>
</body>
</html>",
                    IsEnabled = true,
                    SortOrder = 1
                },

                new()
                {
                    TemplateKey = "citizen_welcome",
                    DisplayName = "Email Mirëseardhje Qytetar",
                    Description = "Dërgohet tek qytetarët e rinj pas regjistrimit.",
                    TemplateType = "Email",
                    Category = "Regjistrimi",
                    Subject = "Mirë se vini në TeknoSOS!",
                    Content = @"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #2563eb;'>Mirë se vini në TeknoSOS, {{FirstName}}!</h2>
    <p>Llogaria juaj u krijua me sukses.</p>
    
    <div style='background-color: #dbeafe; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <strong>✅ Llogaria juaj është aktive!</strong>
        <p style='margin: 10px 0 0 0;'>Mund të filloni të raportoni defekte dhe të lidheni me teknikë profesionistë.</p>
    </div>
    
    <p><strong>Çfarë mund të bëni:</strong></p>
    <ul>
        <li>📸 Raportoni defekte me foto dhe vendndodhje</li>
        <li>🔍 Gjeni teknikë të specializuar pranë jush</li>
        <li>💬 Komunikoni direkt përmes chat-it</li>
        <li>⭐ Vlerësoni teknikët pas punës</li>
    </ul>
    
    <p><a href='https://teknosos.app' style='display: inline-block; background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px;'>Hap Aplikacionin</a></p>
    
    <p style='color: #6b7280; margin-top: 20px;'>— Ekipi TeknoSOS</p>
</body>
</html>",
                    IsEnabled = true,
                    SortOrder = 2
                },

                // ========== VERIFICATION EMAILS ==========
                new()
                {
                    TemplateKey = "technician_verified",
                    DisplayName = "Email Verifikimi Profili",
                    Description = "Dërgohet kur admini verifikon profilin e teknikut.",
                    TemplateType = "Email",
                    Category = "Verifikimi",
                    Subject = "Profili juaj u Verifikua - TeknoSOS",
                    Content = @"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #059669;'>✅ Profili juaj u Verifikua!</h2>
    <p>Përshëndetje {{FirstName}},</p>
    
    <div style='background-color: #d1fae5; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <strong>Lajm i mirë!</strong>
        <p style='margin: 10px 0 0 0;'>Profili juaj si teknik në TeknoSOS u verifikua nga stafi ynë.</p>
    </div>
    
    <p>Tani mund të:</p>
    <ul>
        <li>Shikoni dhe ofertoni për defekte të reja</li>
        <li>Pranoni punë nga klientët</li>
        <li>Ndërtoni reputacionin tuaj në platformë</li>
    </ul>
    
    <p><a href='https://teknosos.app/Dashboard' style='display: inline-block; background-color: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px;'>Shko në Dashboard</a></p>
    
    <p style='color: #6b7280; margin-top: 20px;'>Faleminderit që jeni pjesë e TeknoSOS!</p>
    <p style='color: #6b7280;'>— Ekipi TeknoSOS</p>
</body>
</html>",
                    IsEnabled = true,
                    SortOrder = 1
                },

                new()
                {
                    TemplateKey = "technician_activated",
                    DisplayName = "Email Aktivizimi Llogarie",
                    Description = "Dërgohet kur admini aktivizon llogarinë e teknikut.",
                    TemplateType = "Email",
                    Category = "Verifikimi",
                    Subject = "Llogaria juaj u Aktivizua - TeknoSOS",
                    Content = @"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #059669;'>🎉 Llogaria juaj u Aktivizua!</h2>
    <p>Përshëndetje {{FirstName}},</p>
    
    <div style='background-color: #d1fae5; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <strong>Llogaria juaj është tani aktive!</strong>
        <p style='margin: 10px 0 0 0;'>Mund të filloni të pranoni punë dhe të shërbeni klientët në TeknoSOS.</p>
    </div>
    
    <p>Çfarë mund të bëni tani:</p>
    <ul>
        <li>✅ Shikoni defekte të reja në zonën tuaj</li>
        <li>✅ Ofertoni dhe pranoni punë</li>
        <li>✅ Komunikoni me klientët përmes chat-it</li>
        <li>✅ Merrni vlerësime dhe ndërtoni reputacionin</li>
    </ul>
    
    <p><a href='https://teknosos.app/Dashboard' style='display: inline-block; background-color: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px;'>Filloni Tani</a></p>
    
    <p style='color: #6b7280; margin-top: 20px;'>Suksese!</p>
    <p style='color: #6b7280;'>— Ekipi TeknoSOS</p>
</body>
</html>",
                    IsEnabled = true,
                    SortOrder = 2
                },

                new()
                {
                    TemplateKey = "technician_deactivated",
                    DisplayName = "Email Çaktivizimi Llogarie",
                    Description = "Dërgohet kur admini çaktivizon llogarinë e teknikut.",
                    TemplateType = "Email",
                    Category = "Verifikimi",
                    Subject = "Llogaria juaj u Çaktivizua - TeknoSOS",
                    Content = @"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #dc2626;'>⚠️ Llogaria juaj u Çaktivizua</h2>
    <p>Përshëndetje {{FirstName}},</p>
    
    <div style='background-color: #fee2e2; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <strong>Llogaria juaj u çaktivizua përkohësisht.</strong>
        <p style='margin: 10px 0 0 0;'>Nuk do të mund të pranoni punë të reja derisa llogaria të ri-aktivizohet.</p>
    </div>
    
    <p>Nëse mendoni se kjo është gabim ose keni pyetje, ju lutem na kontaktoni:</p>
    <ul>
        <li>Email: support@teknosos.app</li>
    </ul>
    
    <p style='color: #6b7280; margin-top: 20px;'>— Ekipi TeknoSOS</p>
</body>
</html>",
                    IsEnabled = true,
                    SortOrder = 3
                },

                // ========== SMS TEMPLATES ==========
                new()
                {
                    TemplateKey = "sms_technician_welcome",
                    DisplayName = "SMS Mirëseardhje Teknik",
                    Description = "SMS i shkurtër për teknikët e rinj.",
                    TemplateType = "SMS",
                    Category = "Regjistrimi",
                    Subject = null,
                    Content = "Mire se vini ne TeknoSOS, {{FirstName}}! Llogaria juaj eshte ne pritje per verifikim. Do t'ju njoftojme kur te aktivizohet.",
                    IsEnabled = true,
                    SortOrder = 1
                },

                new()
                {
                    TemplateKey = "sms_citizen_welcome",
                    DisplayName = "SMS Mirëseardhje Qytetar",
                    Description = "SMS i shkurtër për qytetarët e rinj.",
                    TemplateType = "SMS",
                    Category = "Regjistrimi",
                    Subject = null,
                    Content = "Mire se vini ne TeknoSOS, {{FirstName}}! Llogaria juaj eshte aktive. Raportoni defekte dhe gjeni teknike ne teknosos.app",
                    IsEnabled = true,
                    SortOrder = 2
                },

                new()
                {
                    TemplateKey = "sms_technician_activated",
                    DisplayName = "SMS Aktivizimi Teknik",
                    Description = "SMS kur llogaria e teknikut aktivizohet.",
                    TemplateType = "SMS",
                    Category = "Verifikimi",
                    Subject = null,
                    Content = "TeknoSOS: Llogaria juaj u aktivizua! Tani mund te pranoni pune. Hyni ne teknosos.app/Dashboard",
                    IsEnabled = true,
                    SortOrder = 1
                },

                new()
                {
                    TemplateKey = "sms_new_defect",
                    DisplayName = "SMS Defekt i Ri",
                    Description = "SMS për teknikët kur ka defekt të ri në zonën e tyre.",
                    TemplateType = "SMS",
                    Category = "Njoftime",
                    Subject = null,
                    Content = "TeknoSOS: Defekt i ri ne zonen tuaj! {{DefectTitle}} - {{City}}. Shikoni ne teknosos.app",
                    IsEnabled = true,
                    SortOrder = 1
                },

                new()
                {
                    TemplateKey = "sms_defect_assigned",
                    DisplayName = "SMS Puna u Caktua",
                    Description = "SMS për klientin kur caktohet tekniku.",
                    TemplateType = "SMS",
                    Category = "Njoftime",
                    Subject = null,
                    Content = "TeknoSOS: Tekniku {{TechnicianName}} u caktua per defektin tuaj. Kontaktoni permes chat-it ne teknosos.app",
                    IsEnabled = true,
                    SortOrder = 2
                },

                // ========== REGISTRATION EMAILS WITH CONFIRMATION LINK ==========
                new()
                {
                    TemplateKey = "technician_welcome_registration",
                    DisplayName = "Email Regjistrimi Teknik (me konfirmim)",
                    Description = "Dërgohet gjatë regjistrimit me link konfirmimi. Përdorni {{ConfirmationUrl}} për linkun.",
                    TemplateType = "Email",
                    Category = "Regjistrimi",
                    Subject = "Mirë se vini në TeknoSOS - Konfirmo Regjistrimin",
                    Content = @"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #2563eb;'>Mirë se vini në TeknoSOS, {{FirstName}}!</h2>
    <p>Faleminderit që u regjistruat si teknik në platformën tonë.</p>
    
    <div style='background-color: #fef3c7; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <strong>⏳ Llogaria juaj është në pritje për verifikim</strong>
        <p style='margin: 10px 0 0 0;'>Stafi ynë do të shqyrtojë profilin tuaj dhe do t'ju njoftojë me email kur të aktivizohet llogaria.</p>
    </div>
    
    <p><strong>Hapat e ardhshëm:</strong></p>
    <ol>
        <li>Konfirmoni email-in duke klikuar linkun më poshtë</li>
        <li>Prisni verifikimin nga stafi (zakonisht 24-48 orë)</li>
        <li>Do të merrni email kur llogaria të aktivizohet</li>
    </ol>
    
    <p><a href='{{ConfirmationUrl}}' style='display: inline-block; background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px;'>Konfirmo Email-in</a></p>
    
    <p style='color: #6b7280; margin-top: 20px;'>Nëse keni pyetje, na kontaktoni në support@teknosos.app</p>
    <p style='color: #6b7280;'>— Ekipi TeknoSOS</p>
</body>
</html>",
                    IsEnabled = true,
                    SortOrder = 3
                },

                new()
                {
                    TemplateKey = "citizen_welcome_registration",
                    DisplayName = "Email Regjistrimi Qytetar (me konfirmim)",
                    Description = "Dërgohet gjatë regjistrimit me link konfirmimi. Përdorni {{ConfirmationUrl}} për linkun.",
                    TemplateType = "Email",
                    Category = "Regjistrimi",
                    Subject = "Konfirmo Email-in tënd - TeknoSOS",
                    Content = @"<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #2563eb;'>Mirë se vini në TeknoSOS, {{FirstName}}!</h2>
    <p>Faleminderit që u regjistruat në platformën tonë.</p>
    
    <div style='background-color: #dbeafe; padding: 15px; border-radius: 8px; margin: 20px 0;'>
        <strong>📧 Konfirmoni email-in tuaj</strong>
        <p style='margin: 10px 0 0 0;'>Klikoni butonin më poshtë për të aktivizuar llogarinë tuaj.</p>
    </div>
    
    <p><a href='{{ConfirmationUrl}}' style='display: inline-block; background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px;'>Konfirmo Email-in</a></p>
    
    <p><strong>Pas konfirmimit mund të:</strong></p>
    <ul>
        <li>📸 Raportoni defekte me foto dhe vendndodhje</li>
        <li>🔍 Gjeni teknikë të specializuar pranë jush</li>
        <li>💬 Komunikoni direkt përmes chat-it</li>
        <li>⭐ Vlerësoni teknikët pas punës</li>
    </ul>
    
    <p style='color: #6b7280; margin-top: 20px;'>— Ekipi TeknoSOS</p>
</body>
</html>",
                    IsEnabled = true,
                    SortOrder = 4
                }
            };

            _db.NotificationTemplates.AddRange(defaults);
            await _db.SaveChangesAsync();
        }
    }
}
