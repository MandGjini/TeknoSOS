
using System.Collections.Generic;
using System.Linq;
using TeknoSOS.WebApp.Hubs;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    public class NotificationService
    {
        private readonly List<Notification> _db;
        public NotificationService(List<Notification> db) { _db = db; }

        public Notification CreateNotification(string recipientId, string title, string message, TeknoSOS.WebApp.Domain.Enums.NotificationType type)
        {
            var n = new Notification
            {
                RecipientId = recipientId,
                Title = title,
                Message = message,
                Type = type,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };
            _db.Add(n);
            return n;
        }

        // SignalR push methods should be implemented via injected IHubContext<NotificationHub> in controllers/services, not static methods
    }
}
