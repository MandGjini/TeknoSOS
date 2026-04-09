using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ChatMonitorModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ChatMonitorModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ChatConversation> Conversations { get; set; } = new();
        public int TotalMessages { get; set; }
        public int TodayMessages { get; set; }
        public int ActiveChats { get; set; }

        public class ChatConversation
        {
            public int DefectId { get; set; }
            public string DefectCode { get; set; } = string.Empty;
            public string DefectTitle { get; set; } = string.Empty;
            public string CitizenName { get; set; } = string.Empty;
            public string? TechnicianName { get; set; }
            public int MessageCount { get; set; }
            public int UnreadCount { get; set; }
            public string? LastMessage { get; set; }
            public DateTime? LastMessageDate { get; set; }
            public bool HasAttachments { get; set; }
        }

        public async Task OnGetAsync()
        {
            var today = DateTime.UtcNow.Date;

            TotalMessages = await _context.Messages.CountAsync();
            TodayMessages = await _context.Messages.CountAsync(m => m.CreatedDate >= today);

            Conversations = await _context.ServiceRequests
                .Where(sr => sr.Messages.Any())
                .Select(sr => new ChatConversation
                {
                    DefectId = sr.Id,
                    DefectCode = sr.UniqueCode ?? $"#{sr.Id}",
                    DefectTitle = sr.Title,
                    CitizenName = sr.Citizen.FirstName + " " + sr.Citizen.LastName,
                    TechnicianName = sr.Professional != null ? sr.Professional.FirstName + " " + sr.Professional.LastName : null,
                    MessageCount = sr.Messages.Count,
                    UnreadCount = sr.Messages.Count(m => m.ReadDate == null),
                    LastMessage = sr.Messages.OrderByDescending(m => m.CreatedDate).Select(m => m.Content).FirstOrDefault(),
                    LastMessageDate = sr.Messages.Max(m => m.CreatedDate),
                    HasAttachments = sr.Messages.Any(m => m.AttachmentUrl != null)
                })
                .OrderByDescending(c => c.LastMessageDate)
                .Take(50)
                .ToListAsync();

            ActiveChats = Conversations.Count(c => c.LastMessageDate >= today.AddDays(-1));
        }
    }
}
