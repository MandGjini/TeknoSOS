using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Controllers.Api.v1;

[ApiController]
[Route("api/v1/messages")]
[Authorize]
[Produces("application/json")]
public class MobileMessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MobileMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("conversation/{userId}/{serviceRequestId:int}")]
    public async Task<IActionResult> GetConversation(string userId, int serviceRequestId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Unauthorized" });

        var messages = await _context.Messages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ServiceRequestId == serviceRequestId &&
                        ((m.SenderId == currentUserId && m.ReceiverId == userId) ||
                         (m.SenderId == userId && m.ReceiverId == currentUserId)))
            .OrderBy(m => m.CreatedDate)
            .Select(m => new MobileMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderId = m.SenderId,
                SenderName = m.Sender.DisplayUsername ?? m.Sender.FirstName ?? m.Sender.Email ?? "User",
                ReceiverId = m.ReceiverId,
                CreatedAt = m.CreatedDate,
                IsRead = m.Status == MessageStatus.Read,
                AttachmentUrl = m.AttachmentUrl
            })
            .ToListAsync();

        // Mark inbound unread messages as read during conversation fetch.
        var unreadMessages = await _context.Messages
            .Where(m => m.ServiceRequestId == serviceRequestId &&
                        m.SenderId == userId &&
                        m.ReceiverId == currentUserId &&
                        m.Status != MessageStatus.Read)
            .ToListAsync();

        if (unreadMessages.Count > 0)
        {
            foreach (var msg in unreadMessages)
            {
                msg.Status = MessageStatus.Read;
                msg.ReadDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        return Ok(new ApiResponse<List<MobileMessageDto>>
        {
            Success = true,
            Data = messages
        });
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MobileSendMessageRequest request)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Unauthorized" });

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Message content is required" });

        var serviceRequest = await _context.ServiceRequests.FindAsync(request.ServiceRequestId);
        if (serviceRequest == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Service request not found" });

        var receiver = await _userManager.FindByIdAsync(request.ReceiverId);
        if (receiver == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Recipient not found" });

        var sender = await _userManager.FindByIdAsync(currentUserId);

        var message = new Message
        {
            SenderId = currentUserId,
            ReceiverId = request.ReceiverId,
            ServiceRequestId = request.ServiceRequestId,
            Content = request.Content.Trim(),
            AttachmentUrl = request.AttachmentUrl,
            MessageType = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? "text" : "image",
            Status = MessageStatus.Sent,
            CreatedDate = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        _context.Notifications.Add(new Notification
        {
            RecipientId = request.ReceiverId,
            ServiceRequestId = request.ServiceRequestId,
            Title = "Mesazh i ri",
            Message = $"{sender?.GetFullName() ?? "Dikush"} ju dërgoi një mesazh.",
            Type = NotificationType.CaseInProgress,
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<MobileMessageDto>
        {
            Success = true,
            Message = "Message sent",
            Data = new MobileMessageDto
            {
                Id = message.Id,
                Content = message.Content,
                SenderId = message.SenderId,
                SenderName = sender?.DisplayUsername ?? sender?.FirstName ?? sender?.Email ?? "User",
                ReceiverId = message.ReceiverId,
                CreatedAt = message.CreatedDate,
                IsRead = false,
                AttachmentUrl = message.AttachmentUrl
            }
        });
    }
}

public class MobileSendMessageRequest
{
    public string ReceiverId { get; set; } = string.Empty;
    public int ServiceRequestId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class MobileMessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? AttachmentUrl { get; set; }
}
