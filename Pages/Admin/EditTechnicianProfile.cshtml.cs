using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.IO;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditTechnicianProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EditTechnicianProfileModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public TechnicianInputModel Input { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public class TechnicianInputModel
        {
            public string Id { get; set; } = string.Empty;
            [Required]
            public string FirstName { get; set; } = string.Empty;
            [Required]
            public string LastName { get; set; } = string.Empty;
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string DisplayUsername { get; set; } = string.Empty;
            public string Bio { get; set; } = string.Empty;
            public int? YearsOfExperience { get; set; }
            public string Specialties { get; set; } = string.Empty; // comma separated
            public bool IsActive { get; set; } = true;
            public IFormFile? ProfileImage { get; set; }
            public string? ProfileImageUrl { get; set; }
            public string Certificates { get; set; } = string.Empty; // semicolon separated
            public string Portfolio { get; set; } = string.Empty; // semicolon separated
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _context.Users.Include(u => u.Certificates).Include(u => u.PortfolioItems).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null || user.Role != Domain.Enums.UserRole.Professional) return NotFound();

            Input = new TechnicianInputModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email as string ?? string.Empty,
                City = user.City as string ?? string.Empty,
                CompanyName = user.CompanyName as string ?? string.Empty,
                DisplayUsername = user.DisplayUsername as string ?? string.Empty,
                Bio = user.Bio as string ?? string.Empty,
                YearsOfExperience = user.YearsOfExperience,
                Specialties = string.Join(", ", user.Specialties.Select(s => s.Category.ToString())),
                IsActive = user.IsActive,
                ProfileImageUrl = user.ProfileImageUrl,
                Certificates = user.Certificates != null ? string.Join("; ", user.Certificates.Select(c => c.Title)) : string.Empty,
                Portfolio = user.PortfolioItems != null ? string.Join("; ", user.PortfolioItems.Select(p => p.Title)) : string.Empty
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _context.Users.Include(u => u.Certificates).Include(u => u.PortfolioItems).FirstOrDefaultAsync(u => u.Id == Input.Id);
            if (user == null || user.Role != Domain.Enums.UserRole.Professional)
            {
                ErrorMessage = "Tekniku nuk u gjet.";
                return Page();
            }
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Email = Input.Email;
            user.City = Input.City;
            user.CompanyName = Input.CompanyName;
            user.DisplayUsername = Input.DisplayUsername;
            user.Bio = Input.Bio;
            user.YearsOfExperience = Input.YearsOfExperience;
            // Update specialties (clear and add new)
            user.Specialties.Clear();
            if (!string.IsNullOrWhiteSpace(Input.Specialties))
            {
                var specialties = Input.Specialties.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                foreach (var spec in specialties)
                {
                    if (Enum.TryParse<Domain.Enums.ServiceCategory>(spec, true, out var cat))
                    {
                        user.Specialties.Add(new ProfessionalSpecialty { Category = cat, ProfessionalId = user.Id });
                    }
                }
            }
            user.IsActive = Input.IsActive;

            // Profile image upload
            if (Input.ProfileImage != null && Input.ProfileImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images", "profiles");
                Directory.CreateDirectory(uploads);
                var fileName = $"{user.Id}_{DateTime.UtcNow.Ticks}{Path.GetExtension(Input.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfileImage.CopyToAsync(stream);
                }
                user.ProfileImageUrl = $"/images/profiles/{fileName}";
            }

            // Certificates
            user.Certificates.Clear();
            if (!string.IsNullOrWhiteSpace(Input.Certificates))
            {
                var certs = Input.Certificates.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();
                foreach (var cert in certs)
                {
                    user.Certificates.Add(new TechnicianCertificate { Title = cert, TechnicianId = user.Id });
                }
            }

            // Portfolio
            user.PortfolioItems.Clear();
            if (!string.IsNullOrWhiteSpace(Input.Portfolio))
            {
                var items = Input.Portfolio.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
                foreach (var item in items)
                {
                    user.PortfolioItems.Add(new TechnicianPortfolio { Title = item, TechnicianId = user.Id });
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            SuccessMessage = "Profili u përditësua me sukses.";
            return RedirectToPage("/Admin/Technicians");
        }
    }
}
