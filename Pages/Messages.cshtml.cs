using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages
{
    [Authorize]
    public class MessagesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ConversationViewModel> Conversations { get; set; } = new();

        public class ConversationViewModel
        {
            public int DefectId { get; set; }
            public string DefectTitle { get; set; } = string.Empty;
            public string DefectCode { get; set; } = string.Empty;
            public string OtherUserName { get; set; } = string.Empty;
            public string OtherUserAvatar { get; set; } = string.Empty;
            public string OtherUserProfileUrl { get; set; } = string.Empty;
            public string LastMessage { get; set; } = string.Empty;
            public DateTime LastMessageDate { get; set; }
            public int UnreadCount { get; set; }
            public bool IsOnline { get; set; }
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var userId = user.Id;

            // Get all service requests where user is involved (as citizen or professional)
            var serviceRequests = await _context.ServiceRequests
                .Include(sr => sr.Citizen)
                .Include(sr => sr.Professional)
                .Where(sr => sr.CitizenId == userId || sr.ProfessionalId == userId)
                .ToListAsync();

            foreach (var sr in serviceRequests)
            {
                // Determine the other user
                ApplicationUser? otherUser = null;
                if (sr.CitizenId == userId)
                {
                    otherUser = sr.Professional;
                }
                else
                {
                    otherUser = sr.Citizen;
                }

                // Get last message and unread count
                var lastMessage = await _context.Messages
                    .Where(m => m.ServiceRequestId == sr.Id)
                    .OrderByDescending(m => m.CreatedDate)
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.Messages
                    .CountAsync(m => m.ServiceRequestId == sr.Id && m.ReceiverId == userId && m.ReadDate == null);

                // Only show conversations that have messages or have a professional assigned
                if (lastMessage != null || otherUser != null)
                {
                    var conversation = new ConversationViewModel
                    {
                        DefectId = sr.Id,
                        DefectTitle = sr.Title ?? $"Defekt #{sr.Id}",
                        DefectCode = sr.UniqueCode ?? $"#{sr.Id}",
                        OtherUserName = otherUser != null 
                            ? $"{otherUser.FirstName} {otherUser.LastName}".Trim() 
                            : "Pa caktuar",
                        OtherUserAvatar = otherUser?.ProfileImageUrl ?? "",
                        OtherUserProfileUrl = otherUser == null
                            ? string.Empty
                            : otherUser.Role == UserRole.Professional
                                ? $"/TechnicianProfile/{otherUser.DisplayUsername ?? otherUser.Id}"
                                : $"/UserProfile/{otherUser.DisplayUsername ?? otherUser.Id}",
                        LastMessage = lastMessage?.Content ?? "Asnjë mesazh ende",
                        LastMessageDate = lastMessage?.CreatedDate ?? sr.CreatedDate,
                        UnreadCount = unreadCount,
                        IsOnline = false // Could implement online status via SignalR
                    };
                    Conversations.Add(conversation);
                }
            }

            // Sort by last message date (newest first)
            Conversations = Conversations.OrderByDescending(c => c.LastMessageDate).ToList();
        }
    }
}
