using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get conversation messages between current user and another user for a service request.
        /// </summary>
        [HttpGet("conversation/{userId}/{serviceRequestId}")]
        public async Task<IActionResult> GetConversation(string userId, int serviceRequestId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var messages = await _context.Messages
                .Where(m => m.ServiceRequestId == serviceRequestId &&
                    ((m.SenderId == currentUserId && m.ReceiverId == userId) ||
                     (m.SenderId == userId && m.ReceiverId == currentUserId)))
                .OrderBy(m => m.CreatedDate)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Content,
                    m.Status,
                    m.CreatedDate,
                    m.ReadDate,
                    IsOwn = m.SenderId == currentUserId
                })
                .ToListAsync();

            // Mark received messages as read
            var unreadMessages = await _context.Messages
                .Where(m => m.ServiceRequestId == serviceRequestId &&
                    m.SenderId == userId && m.ReceiverId == currentUserId &&
                    m.Status != MessageStatus.Read)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.Status = MessageStatus.Read;
                msg.ReadDate = DateTime.UtcNow;
            }

            if (unreadMessages.Any())
                await _context.SaveChangesAsync();

            return Ok(messages);
        }

        /// <summary>
        /// Send a new message.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { error = "Message content is required." });

            // Verify the service request exists
            var serviceRequest = await _context.ServiceRequests.FindAsync(request.ServiceRequestId);
            if (serviceRequest == null)
                return NotFound(new { error = "Service request not found." });

            // Verify the receiver exists
            var receiver = await _userManager.FindByIdAsync(request.ReceiverId);
            if (receiver == null)
                return NotFound(new { error = "Recipient not found." });

            var message = new Message
            {
                SenderId = currentUserId,
                ReceiverId = request.ReceiverId,
                ServiceRequestId = request.ServiceRequestId,
                Content = request.Content.Trim(),
                Status = MessageStatus.Sent,
                CreatedDate = DateTime.UtcNow
            };

            _context.Messages.Add(message);

            // Create notification for receiver
            var senderUser = await _userManager.FindByIdAsync(currentUserId);
            _context.Notifications.Add(new Notification
            {
                RecipientId = request.ReceiverId,
                ServiceRequestId = request.ServiceRequestId,
                Title = "Mesazh i ri",
                Message = $"{senderUser?.GetFullName() ?? "Dikush"} ju dërgoi një mesazh.",
                Type = NotificationType.CaseInProgress,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message.Id,
                message.SenderId,
                message.ReceiverId,
                message.Content,
                message.Status,
                message.CreatedDate
            });
        }
    }

    public class SendMessageRequest
    {
        public string ReceiverId { get; set; } = string.Empty;
        public int ServiceRequestId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
