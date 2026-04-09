using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AuditLogModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public AuditLogModel(ApplicationDbContext db) => _db = db;

        public List<AuditLog> Logs { get; set; } = new();

        public async Task OnGetAsync()
        {
            Logs = await _db.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedDate)
                .Take(200)
                .ToListAsync();
        }
    }
}
