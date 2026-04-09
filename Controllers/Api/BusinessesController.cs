using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Controllers.Api
{
    [ApiController]
    [Route("api/businesses")]
    public class BusinessesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public BusinessesController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all verified business locations for the map
        /// </summary>
        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations([FromQuery] string? category = null, [FromQuery] string? city = null)
        {
            var query = _db.Businesses
                .Where(b => b.IsActive && b.VerificationStatus == BusinessVerificationStatus.Verified);

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(b => b.City == city);
            }

            var businesses = await query
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    city = b.City,
                    latitude = b.Latitude,
                    longitude = b.Longitude,
                    type = b.Type.ToString(),
                    category = b.PrimaryCategory.ToString(),
                    categoryText = GetCategoryText(b.PrimaryCategory),
                    isVerified = b.VerificationStatus == BusinessVerificationStatus.Verified,
                    averageRating = b.AverageRating,
                    totalReviews = b.TotalReviews
                })
                .ToListAsync();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                businesses = businesses
                    .Where(b => b.category == category)
                    .ToList();
            }

            return Ok(businesses);
        }

        /// <summary>
        /// Get business details (without sensitive contact info)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBusiness(int id)
        {
            var business = await _db.Businesses
                .Where(b => b.Id == id && b.IsActive)
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    description = b.Description,
                    slogan = b.Slogan,
                    type = b.Type.ToString(),
                    category = b.PrimaryCategory.ToString(),
                    city = b.City,
                    region = b.Region,
                    latitude = b.Latitude,
                    longitude = b.Longitude,
                    serviceRadiusKm = b.ServiceRadiusKm,
                    logoUrl = b.LogoUrl,
                    coverImageUrl = b.CoverImageUrl,
                    yearEstablished = b.YearEstablished,
                    isVerified = b.VerificationStatus == BusinessVerificationStatus.Verified,
                    averageRating = b.AverageRating,
                    totalReviews = b.TotalReviews,
                    totalOrders = b.TotalOrders,
                    completedOrders = b.CompletedOrders
                    // Note: Contact info (email, phone, address) is NOT exposed
                })
                .FirstOrDefaultAsync();

            if (business == null)
                return NotFound(new { error = "Business not found" });

            return Ok(business);
        }

        /// <summary>
        /// Search businesses
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? q = null,
            [FromQuery] string? type = null,
            [FromQuery] string? category = null,
            [FromQuery] string? city = null,
            [FromQuery] int? minRating = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.Businesses
                .Where(b => b.IsActive && b.VerificationStatus == BusinessVerificationStatus.Verified);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchLower = q.ToLower();
                query = query.Where(b => 
                    b.Name.ToLower().Contains(searchLower) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<BusinessType>(type, out var typeEnum))
            {
                query = query.Where(b => b.Type == typeEnum);
            }

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<BusinessCategory>(category, out var categoryEnum))
            {
                query = query.Where(b => b.PrimaryCategory == categoryEnum);
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(b => b.City == city);
            }

            if (minRating.HasValue)
            {
                query = query.Where(b => b.AverageRating >= minRating.Value);
            }

            var total = await query.CountAsync();

            var businesses = await query
                .OrderByDescending(b => b.AverageRating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    description = b.Description,
                    type = b.Type.ToString(),
                    category = b.PrimaryCategory.ToString(),
                    city = b.City,
                    logoUrl = b.LogoUrl,
                    isVerified = b.VerificationStatus == BusinessVerificationStatus.Verified,
                    averageRating = b.AverageRating,
                    totalReviews = b.TotalReviews
                })
                .ToListAsync();

            return Ok(new
            {
                data = businesses,
                pagination = new
                {
                    page,
                    pageSize,
                    total,
                    totalPages = (int)Math.Ceiling((double)total / pageSize)
                }
            });
        }

        /// <summary>
        /// Get products for a business
        /// </summary>
        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetProducts(int id)
        {
            var products = await _db.BusinessProducts
                .Where(p => p.BusinessId == id && p.IsAvailable)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    category = p.Category.ToString(),
                    price = p.Price,
                    unit = p.Unit,
                    imageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// Get reviews for a business
        /// </summary>
        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetReviews(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _db.BusinessReviews
                .Where(r => r.BusinessId == id)
                .OrderByDescending(r => r.CreatedDate);

            var total = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    id = r.Id,
                    rating = r.Rating,
                    comment = r.Comment,
                    createdDate = r.CreatedDate,
                    isVerifiedPurchase = r.IsVerifiedPurchase,
                    reviewerName = r.Reviewer != null ? r.Reviewer.FirstName + " " + (r.Reviewer.LastName != null ? r.Reviewer.LastName.Substring(0, 1) + "." : "") : "Anonim"
                })
                .ToListAsync();

            return Ok(new
            {
                data = reviews,
                pagination = new { page, pageSize, total }
            });
        }

        private static string GetCategoryText(BusinessCategory category) => category switch
        {
            BusinessCategory.ElectricalSupplies => "Materiale Elektrike",
            BusinessCategory.PlumbingSupplies => "Materiale Hidraulike",
            BusinessCategory.ConstructionMaterials => "Materiale Ndërtimi",
            BusinessCategory.HVACSupplies => "Ngrohje/Ftohje",
            BusinessCategory.PaintAndFinishes => "Bojë & Përfundime",
            BusinessCategory.ToolsAndEquipment => "Vegla & Pajisje",
            BusinessCategory.SafetyEquipment => "Pajisje Sigurie",
            BusinessCategory.LightingSupplies => "Ndriçim",
            BusinessCategory.ArchitecturalServices => "Arkitekturë",
            BusinessCategory.EngineeringServices => "Inxhinieri",
            BusinessCategory.InteriorDesign => "Dizajn Enterieri",
            BusinessCategory.PermitServices => "Shërbime Lejesh",
            BusinessCategory.InspectionServices => "Inspektime",
            BusinessCategory.EquipmentRental => "Qiradhënie Pajisjesh",
            BusinessCategory.ScaffoldingRental => "Qiradhënie Skelash",
            BusinessCategory.VehicleRental => "Qiradhënie Automjetesh",
            BusinessCategory.WasteDisposal => "Largim Mbeturinash",
            BusinessCategory.Logistics => "Logjistikë",
            BusinessCategory.Cleaning => "Pastrim",
            BusinessCategory.Security => "Siguri",
            BusinessCategory.GeneralContractor => "Kontraktor",
            BusinessCategory.Other => "Të Tjera",
            _ => "Biznes"
        };
    }
}
