using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages
{
    public class BusinessesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BusinessesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Business> Businesses { get; set; } = new List<Business>();
        public IEnumerable<string> AvailableCities { get; set; } = new List<string>();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedTypes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SelectedCategory { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedCity { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinRating { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; } = "rating";

        public async Task OnGetAsync(string[]? types)
        {
            // Populate types from query string
            if (types != null && types.Length > 0)
            {
                SelectedTypes = types.ToList();
            }

            // Get available cities
            AvailableCities = await _context.Businesses
                .Where(b => b.IsActive && !string.IsNullOrEmpty(b.City))
                .Select(b => b.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Build query
            var query = _context.Businesses
                .Where(b => b.IsActive && b.VerificationStatus == BusinessVerificationStatus.Verified)
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchLower = SearchQuery.ToLower();
                query = query.Where(b => 
                    b.Name.ToLower().Contains(searchLower) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchLower)));
            }

            // Apply type filter
            if (SelectedTypes.Any())
            {
                var typeEnums = SelectedTypes
                    .Select(t => Enum.TryParse<BusinessType>(t, out var result) ? result : (BusinessType?)null)
                    .Where(t => t.HasValue)
                    .Select(t => t!.Value)
                    .ToList();

                if (typeEnums.Any())
                {
                    query = query.Where(b => typeEnums.Contains(b.Type));
                }
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(SelectedCategory) && 
                Enum.TryParse<BusinessCategory>(SelectedCategory, out var category))
            {
                query = query.Where(b => b.PrimaryCategory == category);
            }

            // Apply city filter
            if (!string.IsNullOrWhiteSpace(SelectedCity))
            {
                query = query.Where(b => b.City == SelectedCity);
            }

            // Apply rating filter
            if (MinRating.HasValue)
            {
                query = query.Where(b => b.AverageRating >= MinRating.Value);
            }

            // Apply sorting
            query = SortBy switch
            {
                "name" => query.OrderBy(b => b.Name),
                "recent" => query.OrderByDescending(b => b.CreatedDate),
                _ => query.OrderByDescending(b => b.AverageRating).ThenByDescending(b => b.TotalReviews)
            };

            Businesses = await query.ToListAsync();
        }
    }
}
