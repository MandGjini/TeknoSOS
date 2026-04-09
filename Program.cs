using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TeknoSOS.WebApp.Hubs;
using TeknoSOS.WebApp.Services;
using TeknoSOS.WebApp.Models;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

// ... pjesa ekzistuese e konfigurimit të databazës, autentikimit, etj ...

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1. DATABASE CONFIGURATION
// ============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ============================================================================
// 2. IDENTITY CONFIGURATION
// ============================================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements - simplified for testing
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // SignIn settings - disabled for testing
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// ============================================================================
// 3. MVC & RAZOR PAGES
// ============================================================================
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminControl", policy => policy.RequireRole("Admin"));
});
builder.Services.AddRazorPages(options =>
{
    // Force the entire Admin panel to be accessible only by Admin accounts.
    options.Conventions.AuthorizeFolder("/Admin", "AdminControl");
});
builder.Services.AddSignalR();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, SmtpEmailSender>();

// Response Compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/javascript", "text/css", "image/svg+xml" });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
});

// Localization & HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<ISiteContentService, SiteContentService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ISitemapService, SitemapService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddSingleton<IContactMaskingService, ContactMaskingService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISmsSender, LoggingSmsSender>();
builder.Services.AddScoped<IPushNotificationSender, SignalRPushNotificationSender>();
builder.Services.AddScoped<IProfessionalOpportunityNotifier, ProfessionalOpportunityNotifier>();
builder.Services.AddHttpClient<IWhatsAppCloudSender, WhatsAppCloudSender>();
builder.Services.AddHostedService<SubscriptionExpiryReminderHostedService>();
builder.Services.AddHostedService<MonthlyBillingHostedService>();

// ============================================================================
// JWT Authentication for Mobile API
// ============================================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey required");

builder.Services.AddAuthentication(options =>
{
    // Default to cookies for web, but allow JWT for API
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && 
                (path.StartsWithSegments("/chathub") || path.StartsWithSegments("/notificationhub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
// Google Authentication
var googleAuthSection = builder.Configuration.GetSection("GoogleAuth");
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = googleAuthSection["ClientId"] ?? string.Empty;
        options.ClientId = googleAuthSection["ClientId"] ?? string.Empty;
        options.ClientSecret = googleAuthSection["ClientSecret"] ?? string.Empty;
        options.CallbackPath = "/signin-google";
    });

// ============================================================================
// CORS Configuration for Mobile Apps
// ============================================================================
var corsOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobilePolicy", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
    options.AddPolicy("AnyOrigin", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================================
// Rate Limiting Configuration
// ============================================================================
builder.Services.AddRateLimiter(limiter =>
{
    // Fixed window limiter for login attempts (5 attempts per 15 minutes)
    limiter.AddFixedWindowLimiter(policyName: "login", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(15);
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });
    
    // Sliding window for general API usage (100 requests per minute)
    limiter.AddSlidingWindowLimiter(policyName: "api", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.SegmentsPerWindow = 6;
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 10;
    });
    
    // Strict limiter for password reset (3 attempts per hour)
    limiter.AddFixedWindowLimiter(policyName: "passwordReset", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromHours(1);
        options.QueueLimit = 0;
    });
    
    // Default rejection response
    limiter.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.Append("Retry-After", "60");
        await context.HttpContext.Response.WriteAsJsonAsync(new 
        { 
            error = "Too many requests. Please try again later.",
            retryAfter = 60 
        }, cancellationToken: token);
    };
});

// ============================================================================
// Swagger/OpenAPI for Mobile API Documentation
// ============================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "TeknoSOS Mobile API", 
        Version = "v1",
        Description = "REST API for TeknoSOS iOS and Android applications",
        Contact = new OpenApiContact { Name = "TeknoSOS Support", Email = "support@teknosos.app" }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Configure cookie security settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    
    // Security settings
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// ============================================================================
// Build Application
// ============================================================================
// If certificate files for production HTTPS are missing, avoid letting Kestrel try to load them
var certPath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Path"];
var certKeyPath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:KeyPath"];
if (!string.IsNullOrEmpty(certPath) && !System.IO.File.Exists(certPath))
{
    Console.WriteLine($"Warning: HTTPS certificate not found at {certPath}. Overriding Kestrel to run HTTP only for local/testing.");
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        // Ensure we at least listen on HTTP port 5000 — this overrides the configuration-based HTTPS endpoint
        serverOptions.ListenAnyIP(5000);
    });
}

// Also clear certificate config values so Kestrel won't try to load them from configuration
if (!string.IsNullOrEmpty(certPath) && !System.IO.File.Exists(certPath))
{
    try
    {
        builder.Configuration["Kestrel:Endpoints:Https:Certificate:Path"] = string.Empty;
        builder.Configuration["Kestrel:Endpoints:Https:Certificate:KeyPath"] = string.Empty;
    }
    catch { }
}

var app = builder.Build();

// ============================================================================
// 4. DATABASE MIGRATION & SEEDING
// ============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        async Task<ApplicationUser?> FindUserByEmailIncludingInactiveAsync(string email)
        {
            var normalizedEmail = userManager.NormalizeEmail(email);
            return await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        }

        // Apply migrations
        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = { "Admin", "Professional", "Citizen" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        var adminEmail = "admin@teknosos.local";
        var adminUser = await FindUserByEmailIncludingInactiveAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                DisplayUsername = "admin",
                EmailConfirmed = true,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow
            };
            var adminPassword = builder.Configuration["SeedPasswords:Admin"] ?? "Admin#2024";
            try
            {
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    Console.WriteLine($"Warning: could not create admin user: {string.Join(';', result.Errors.Select(e=>e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: exception creating admin user: {ex.Message}");
            }
        }

        // Seed citizen user
        var citizenEmail = "citizen@teknosos.local";
        var citizenUser = await FindUserByEmailIncludingInactiveAsync(citizenEmail);
        if (citizenUser == null)
        {
            citizenUser = new ApplicationUser
            {
                UserName = citizenEmail,
                Email = citizenEmail,
                FirstName = "Demo",
                LastName = "Citizen",
                DisplayUsername = "citizen_demo",
                EmailConfirmed = true,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow
            };
            var citizenPassword = builder.Configuration["SeedPasswords:Citizen"] ?? "Citizen#2024";
            try
            {
                var result = await userManager.CreateAsync(citizenUser, citizenPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(citizenUser, "Citizen");
                }
                else
                {
                    Console.WriteLine($"Warning: could not create citizen user: {string.Join(';', result.Errors.Select(e=>e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: exception creating citizen user: {ex.Message}");
            }
        }

        // Seed professional user
        var proEmail = "pro@teknosos.local";
        var proUser = await FindUserByEmailIncludingInactiveAsync(proEmail);
        if (proUser == null)
        {
            proUser = new ApplicationUser
            {
                UserName = proEmail,
                Email = proEmail,
                FirstName = "Demo",
                LastName = "Professional",
                DisplayUsername = "pro_demo",
                EmailConfirmed = true,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow
            };
            var proPassword = builder.Configuration["SeedPasswords:Professional"] ?? "Pro#2024";
            try
            {
                var result = await userManager.CreateAsync(proUser, proPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(proUser, "Professional");
                }
                else
                {
                    Console.WriteLine($"Warning: could not create professional user: {string.Join(';', result.Errors.Select(e=>e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: exception creating professional user: {ex.Message}");
            }
        }

        Console.WriteLine("Database migrated and seeded successfully!");

        // Seed test data (30 users + 20 defects + bids)
        try
        {
            await TestDataSeeder.SeedTestDataAsync(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: test data seeding failed: {ex.Message}");
        }

        // Seed 1000 bulk users — skip if the DB already has many users to avoid duplicate insertion attempts
        var existingUserCount = await context.Users.IgnoreQueryFilters().CountAsync();
        if (existingUserCount < 500)
        {
            try
            {
                await TestDataSeeder.SeedBulkUsersAsync(context, userManager);
            }
            catch (Exception ex)
            {
                // Log and continue — bulk seeder can throw on duplicate inserts in some environments
                Console.WriteLine($"Warning: bulk user seeding failed: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"Skipping bulk user seeding; existing user count = {existingUserCount}.");
        }

        // Seed certificates for technicians that don't have any
        await TestDataSeeder.SeedCertificatesForExistingTechniciansAsync(context);

        // Seed site settings
        var settingsService = services.GetRequiredService<ISiteSettingsService>();
        await settingsService.SeedDefaultsAsync();

        // Seed notification templates
        var templateService = services.GetRequiredService<INotificationTemplateService>();
        await templateService.SeedDefaultTemplatesAsync();


        // Seed CMS content
        var contentService = services.GetRequiredService<ISiteContentService>();
        if (!await contentService.HasContentAsync("home", "sq"))
        {
            var defaultContent = ContentSeeder.GetDefaultContent();
            await contentService.SaveBulkContentAsync(defaultContent, "system");
            Console.WriteLine($"Seeded {defaultContent.Count} content blocks.");
        }

        // Blog Seeder (200+ posts)
        TeknoSOS.WebApp.Data.Seed.BlogSeeder.Seed(context);

        // Seed blog posts from static data as fallback only if DB is empty
        var blogService = services.GetRequiredService<IBlogService>();
        await blogService.SeedFromStaticDataAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database setup: {ex.Message}");
    }
}

// ============================================================================
// 5. MIDDLEWARE PIPELINE
// ============================================================================

// Response compression (must be early in pipeline)
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
    
    // Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeknoSOS Mobile API v1");
        c.RoutePrefix = "api-docs";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    
    // Also enable Swagger in production (optional - can be disabled for security)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeknoSOS Mobile API v1");
        c.RoutePrefix = "api-docs";
    });
}

// HTTPS Redirection - enabled conditionally
if (!app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Security:EnableHttpsRedirection", true))
{
    app.UseHttpsRedirection();
}

// Static files with caching headers for production
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static assets for 1 year in production
        if (!app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
    }
});

app.UseRouting();

// Rate limiting - must be after routing
app.UseRateLimiter();

// CORS must be after Routing but before Authorization
app.UseCors("MobilePolicy");

// CRITICAL: Order matters! Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

// ============================================================================
// 6. ENDPOINT ROUTING
// ============================================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapGet("/Admin/Dashboard", context =>
{
    context.Response.Redirect("/Admin");
    return Task.CompletedTask;
});

app.MapGet("/admin-command-center", context =>
{
    context.Response.Redirect("/Admin");
    return Task.CompletedTask;
});

app.MapGet("/sitemap.xml", async (HttpContext context, ISitemapService sitemapService, ISiteSettingsService settingsService) =>
{
    var seoSettings = await settingsService.GetGroupAsync("seo");
    var configuredBaseUrl = seoSettings.TryGetValue("SiteBaseUrl", out var configured) && !string.IsNullOrWhiteSpace(configured)
        ? configured
        : $"{context.Request.Scheme}://{context.Request.Host}";

    var xml = await sitemapService.BuildSitemapXmlAsync(configuredBaseUrl);
    return Results.Content(xml, "application/xml", Encoding.UTF8);
});

app.MapGet("/robots.txt", async (HttpContext context, ISiteSettingsService settingsService) =>
{
    var seoSettings = await settingsService.GetGroupAsync("seo");
    var configuredBaseUrl = seoSettings.TryGetValue("SiteBaseUrl", out var configured) && !string.IsNullOrWhiteSpace(configured)
        ? configured.TrimEnd('/')
        : $"{context.Request.Scheme}://{context.Request.Host}";

    var robots =
        "User-agent: *\n" +
        "Allow: /\n\n" +
        "Disallow: /Admin/\n" +
        "Disallow: /Account/Manage/\n" +
        "Disallow: /Identity/Account/Manage/\n" +
        "Disallow: /api/\n" +
        "Disallow: /Dashboard\n" +
        "Disallow: /Messages\n" +
        "Disallow: /Chat\n\n" +
        $"Sitemap: {configuredBaseUrl}/sitemap.xml\n";

    return Results.Text(robots, "text/plain", Encoding.UTF8);
});

// SignalR Hubs
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationHub>("/notificationhub");

// ============================================================================
// 7. RUN APPLICATION
// ============================================================================
Console.WriteLine("TeknoSOS Platform starting...");
Console.WriteLine("Access: https://localhost:5001 or http://localhost:5000");
Console.WriteLine("");
Console.WriteLine("Test Accounts:");
Console.WriteLine("   Admin: admin@teknosos.local / Admin#2024");
Console.WriteLine("   Citizen: citizen@teknosos.local / Citizen#2024");
Console.WriteLine("   Professional: pro@teknosos.local / Pro#2024");

app.Run();
