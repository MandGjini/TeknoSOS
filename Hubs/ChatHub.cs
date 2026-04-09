using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Hubs
{
    /// <summary>
    /// SignalR hub for real-time chat between clients and technicians per defect.
    /// All messages are filtered to prevent sharing of contact information.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IContactMaskingService _contactMaskingService;

        // Track which users are typing in which defect groups
        private static readonly Dictionary<string, HashSet<string>> _typingUsers = new();

        public ChatHub(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            IHubContext<NotificationHub> notificationHubContext,
            IContactMaskingService contactMaskingService)
        {
            _context = context;
            _userManager = userManager;
            _notificationHubContext = notificationHubContext;
            _contactMaskingService = contactMaskingService;
        }

        /// <summary>
        /// Join a defect-specific chat group - with authorization check
        /// </summary>
        public async Task JoinDefectChat(int serviceRequestId)
        {
            var groupName = $"defect-{serviceRequestId}";
            
            // SECURITY: Verify user has access to this service request
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null)
            {
                throw new HubException("User not found.");
            }

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.Id == serviceRequestId);
            
            if (serviceRequest == null)
            {
                throw new HubException("Service request not found.");
            }

            // Check authorization: owner, assigned technician, or admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            bool isAuthorized = serviceRequest.CitizenId == user.Id ||
                               serviceRequest.ProfessionalId == user.Id ||
                               isAdmin;

            if (!isAuthorized)
            {
                throw new HubException("You don't have access to this chat.");
            }
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserJoined", new
            {
                userId = user.Id,
                userName = System.Net.WebUtility.HtmlEncode(user.GetFullName()),
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Leave a defect chat group - with authorization check
        /// </summary>
        public async Task LeaveDefectChat(int serviceRequestId)
        {
            var groupName = $"defect-{serviceRequestId}";
            
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;
            
            // Note: No strict authorization needed for leaving - 
            // they can only be in a group if they joined legitimately
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserLeft", new
            {
                userId = user.Id,
                userName = System.Net.WebUtility.HtmlEncode(user.GetFullName()),
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Send a text message in a defect chat.
        /// Messages are automatically filtered to prevent sharing of contact information.
        /// </summary>
        public async Task SendMessage(int serviceRequestId, string receiverId, string content, string messageType = "text", string? attachmentUrl = null, string? attachmentFileName = null)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            // Check if message contains contact information and warn the user
            bool containsContactInfo = _contactMaskingService.ContainsContactInfo(content);
            
            // Mask any contact information in the message
            string maskedContent = _contactMaskingService.MaskContactInfo(content);

            var message = new Message
            {
                SenderId = user.Id,
                ReceiverId = receiverId,
                ServiceRequestId = serviceRequestId,
                Content = maskedContent, // Store the masked version
                MessageType = messageType,
                AttachmentUrl = attachmentUrl,
                AttachmentFileName = attachmentFileName,
                Status = MessageStatus.Sent,
                CreatedDate = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var groupName = $"defect-{serviceRequestId}";
            
            // Send the masked message to the group
            await Clients.Group(groupName).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                senderId = user.Id,
                senderName = user.GetFullName(),
                senderImage = user.ProfileImageUrl ?? "/images/default-avatar.png",
                content = maskedContent, // Send masked content
                messageType = messageType,
                attachmentUrl = attachmentUrl,
                attachmentFileName = attachmentFileName,
                timestamp = message.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                serviceRequestId = serviceRequestId,
                containedContactInfo = containsContactInfo // Flag for UI warning
            });

            // Send a warning to the sender if contact info was detected
            if (containsContactInfo)
            {
                await Clients.Caller.SendAsync("ContactInfoWarning", new
                {
                    message = _contactMaskingService.GetContactWarningMessage("sq"),
                    originalContent = content,
                    maskedContent = maskedContent
                });
            }

            // Create notification for the receiver (with masked content)
            if (!string.IsNullOrEmpty(receiverId))
            {
                var notificationPreview = maskedContent.Length > 50 ? maskedContent.Substring(0, 50) + "..." : maskedContent;
                var notification = new Notification
                {
                    RecipientId = receiverId,
                    ServiceRequestId = serviceRequestId,
                    Type = NotificationType.NewMessage,
                    Title = "Mesazh i ri",
                    Message = $"{user.GetFullName()}: {notificationPreview}",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    IsPushSent = false,
                    IsEmailSent = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send real-time notification to receiver via NotificationHub
                await _notificationHubContext.Clients.Group($"user-{receiverId}").SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = (int)notification.Type,
                    serviceRequestId = serviceRequestId,
                    createdDate = notification.CreatedDate
                });
            }

            // Also notify admin monitoring group (with masked content)
            await Clients.Group("admin-chat-monitor").SendAsync("NewChatMessage", new
            {
                messageId = message.Id,
                serviceRequestId = serviceRequestId,
                senderName = user.GetFullName(),
                content = maskedContent.Length > 50 ? maskedContent.Substring(0, 50) + "..." : maskedContent,
                timestamp = message.CreatedDate,
                containedContactInfo = containsContactInfo
            });
        }

        /// <summary>
        /// Signal that a user is typing
        /// </summary>
        public async Task StartTyping(int serviceRequestId)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            var groupName = $"defect-{serviceRequestId}";
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new
            {
                userId = user.Id,
                userName = user.GetFullName(),
                isTyping = true
            });
        }

        /// <summary>
        /// Signal that a user stopped typing
        /// </summary>
        public async Task StopTyping(int serviceRequestId)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            var groupName = $"defect-{serviceRequestId}";
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new
            {
                userId = user.Id,
                userName = user.GetFullName(),
                isTyping = false
            });
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        public async Task MarkAsRead(int serviceRequestId)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            var unreadMessages = await _context.Messages
                .Where(m => m.ServiceRequestId == serviceRequestId 
                         && m.ReceiverId == user.Id 
                         && m.ReadDate == null)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.ReadDate = DateTime.UtcNow;
                msg.Status = MessageStatus.Read;
            }
            await _context.SaveChangesAsync();

            var groupName = $"defect-{serviceRequestId}";
            await Clients.Group(groupName).SendAsync("MessagesRead", new
            {
                userId = user.Id,
                serviceRequestId = serviceRequestId,
                count = unreadMessages.Count
            });
        }

        /// <summary>
        /// Admin joins monitoring group
        /// </summary>
        public async Task JoinAdminMonitoring()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin-chat-monitor");
            }
        }

        /// <summary>
        /// Technician joins their category-specific group for receiving targeted notifications
        /// </summary>
        public async Task JoinTechnicianGroup(string category)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null && await _userManager.IsInRoleAsync(user, "Professional"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"technicians-{category.ToLower()}");
                await Groups.AddToGroupAsync(Context.ConnectionId, "all-technicians");
            }
        }

        /// <summary>
        /// Update technician's GPS location (called periodically from technician's browser)
        /// </summary>
        public async Task UpdateTechnicianLocation(decimal latitude, decimal longitude)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user == null) return;

            user.Latitude = latitude;
            user.Longitude = longitude;
            await _userManager.UpdateAsync(user);
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                // Auto-join technicians to "all-technicians" group
                if (await _userManager.IsInRoleAsync(user, "Professional"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "all-technicians");

                    // Join category-specific groups based on specialties
                    var specialties = await _context.Set<ProfessionalSpecialty>()
                        .Where(s => s.ProfessionalId == user.Id)
                        .Select(s => s.Category)
                        .ToListAsync();

                    foreach (var cat in specialties)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"technicians-{cat.ToString().ToLower()}");
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
