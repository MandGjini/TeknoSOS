# TeknoSOS — Dokumentacion Teknik i Plotë

**Data e Përditësimit:** Mars 2026  
**Versioni:** 2.0  
**Platforma:** ASP.NET Core 8.0

---

## Përmbajtja

1. [Arkitektura e Sistemit](#1-arkitektura-e-sistemit)
2. [Modelet e Databazës](#2-modelet-e-databazës)
3. [Shërbimet (Services)](#3-shërbimet-services)
4. [SignalR Hubs](#4-signalr-hubs)
5. [API Controllers](#5-api-controllers)
6. [Faqet (Pages)](#6-faqet-pages)
7. [Admin Panel](#7-admin-panel)
8. [Sistemi CMS](#8-sistemi-cms)
9. [Lokalizimi](#9-lokalizimi)
10. [Siguria](#10-siguria)
11. [Konfigurimet](#11-konfigurimet)
12. [Deployment](#12-deployment)

---

## 1. Arkitektura e Sistemit

### 1.1 Diagrami i Arkitekturës

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                                 │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│   │   Browser    │  │   Mobile     │  │   WhatsApp   │              │
│   │  (Desktop)   │  │   Browser    │  │   Chatbot    │              │
│   └──────────────┘  └──────────────┘  └──────────────┘              │
└─────────────────────────────────────────────────────────────────────┘
                              │ HTTPS
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       PRESENTATION LAYER                             │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│   │ Razor Pages  │  │  REST API    │  │  SignalR     │              │
│   │   (Views)    │  │  Controllers │  │   Hubs       │              │
│   └──────────────┘  └──────────────┘  └──────────────┘              │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       APPLICATION LAYER                              │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│   │ Localization │  │ SiteContent  │  │ SiteSettings │              │
│   │   Service    │  │   Service    │  │   Service    │              │
│   └──────────────┘  └──────────────┘  └──────────────┘              │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         DATA LAYER                                   │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│   │   EF Core    │  │ DbContext    │  │   Identity   │              │
│   │     ORM      │  │   (14+ Sets) │  │   Stores     │              │
│   └──────────────┘  └──────────────┘  └──────────────┘              │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       DATABASE LAYER                                 │
│                    ┌──────────────────┐                              │
│                    │   SQL Server     │                              │
│                    │  (LocalDB/Express)│                              │
│                    └──────────────────┘                              │
└─────────────────────────────────────────────────────────────────────┘
```

### 1.2 Stack Teknologjik

| Komponent | Teknologjia | Versioni | Qëllimi |
|-----------|-------------|----------|---------|
| Framework | ASP.NET Core | 8.0 LTS | Backend, Razor Pages |
| ORM | Entity Framework Core | 8.0 | Aksesi në databazë |
| Database | SQL Server | 2022 | Ruajtja e të dhënave |
| Auth | ASP.NET Identity | 8.0 | Autentifikim, autorizim |
| Real-time | SignalR | 8.0 | Chat, notifikime |
| Frontend | Bootstrap | 5.3.2 | UI Framework |
| Icons | Bootstrap Icons | 1.11.2 | Ikonografi |
| Charts | Chart.js | 4.4.7 | Vizualizime |
| Validation | jQuery Validation | 1.x | Validim formash |
| Compression | Brotli + Gzip | - | Performancë |

---

## 2. Modelet e Databazës

### 2.1 Entity Relationship Diagram

```
ApplicationUser (1) ──────< (N) ServiceRequest
       │                          │
       │                          │
       └──< (N) TechnicianInterest >───┘
       │
       └──< (N) Message >──< (N) ServiceRequest
       │
       └──< (N) Review
       │
       └──< (N) Notification
       │
       └──< (N) SubscriptionPayment

SiteContent (CMS) ──── PageKey, SectionKey, Language
SiteSetting ──── GroupKey, SettingKey
MenuItem ──── Parent-Child hierarchy
MediaFile ──── Uploaded files
AuditLog ──── All changes tracking
Banner ──── Promotional banners
```

### 2.2 Entitetet Kryesore

#### ApplicationUser (Përdoruesi)
```csharp
public class ApplicationUser : IdentityUser
{
    // Të dhëna bazë
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayUsername { get; set; }
    public UserRole Role { get; set; }  // Citizen, Professional, Admin
    
    // Profili
    public string ProfileImageUrl { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    
    // GPS
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    // Vlerësimi
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    // Për Teknikë
    public string Bio { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsAvailableForWork { get; set; }
    public int ServiceRadiusKm { get; set; }
    public bool IsProfileVerified { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    
    // Computed
    public bool IsSubscriptionActive => SubscriptionEndDate > DateTime.UtcNow;
}
```

#### ServiceRequest (Defekti/Kërkesa)
```csharp
public class ServiceRequest
{
    public int Id { get; set; }
    public string UniqueCode { get; set; }  // DEF-XXXXXX
    
    // Përdorues
    public string CitizenId { get; set; }
    public string ProfessionalId { get; set; }
    
    // Detajet
    public string Title { get; set; }
    public string Description { get; set; }
    public ServiceCategory Category { get; set; }
    public RequestStatus Status { get; set; }
    public RequestPriority Priority { get; set; }
    
    // Media
    public string PhotoUrl { get; set; }
    public string Location { get; set; }
    public decimal ClientLatitude { get; set; }
    public decimal ClientLongitude { get; set; }
    
    // Kosto
    public decimal? EstimatedCost { get; set; }
    public decimal? FinalCost { get; set; }
    
    // Datat
    public DateTime CreatedDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}
```

#### TechnicianInterest (Oferta)
```csharp
public class TechnicianInterest
{
    public int Id { get; set; }
    public string TechnicianId { get; set; }
    public int ServiceRequestId { get; set; }
    
    public InterestStatus Status { get; set; }
    public string PreventiveOffer { get; set; }
    public decimal? EstimatedCost { get; set; }
    public int? EstimatedTimeInHours { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime? ResponseDate { get; set; }
}
```

#### Message (Mesazhi)
```csharp
public class Message
{
    public int Id { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public int ServiceRequestId { get; set; }
    
    public string Content { get; set; }
    public MessageType MessageType { get; set; }
    public string AttachmentUrl { get; set; }
    public MessageStatus Status { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime? ReadDate { get; set; }
}
```

### 2.3 Enumerimet

```csharp
public enum UserRole { Citizen, Professional, Admin }

public enum ServiceCategory 
{ 
    Plumbing, Electrical, HVAC, Carpentry, 
    Appliances, IT, GeneralMaintenance 
}

public enum RequestStatus 
{ 
    Created, Matched, InProgress, 
    Completed, Cancelled, Rejected 
}

public enum RequestPriority { Normal, High, Emergency }

public enum InterestStatus { Interested, Accepted, Rejected, Withdrawn }

public enum MessageStatus { Sent, Delivered, Read }
```

---

## 3. Shërbimet (Services)

### 3.1 LocalizationService

Menaxhon përkthimet për 5 gjuhë: sq, en, it, de, fr.

```csharp
public interface ILocalizationService
{
    string T(string key);  // Translate
    string CurrentLanguage { get; }
    Task<Dictionary<string, string>> GetAllTranslationsAsync(string lang);
    Task SetTranslationAsync(string lang, string key, string value);
}
```

**Përdorimi në Razor:**
```html
@inject ILocalizationService L
<h1>@L.T("nav.home")</h1>
```

### 3.2 SiteContentService

Menaxhon përmbajtjen CMS (tekste, blloqe, faqe).

```csharp
public interface ISiteContentService
{
    Task<string> GetContentAsync(string pageKey, string sectionKey, string lang = null);
    Task<ContentBlock> GetBlockAsync(string pageKey, string sectionKey, string lang = null);
    Task<List<ContentBlock>> GetBlocksAsync(string pageKey, string lang = null);
    Task SetContentAsync(string pageKey, string sectionKey, string content, string lang);
}
```

**Përdorimi në Razor:**
```html
@inject ISiteContentService CMS
<h1>@await CMS.GetContentAsync("home", "hero_title")</h1>
```

### 3.3 SiteSettingsService

Menaxhon cilësimet e sajtit (branding, SEO, kontakt, footer).

```csharp
public interface ISiteSettingsService
{
    Task<string> GetAsync(string groupKey, string settingKey, string defaultValue = "");
    Task<Dictionary<string, string>> GetGroupAsync(string groupKey);
    Task SetAsync(string groupKey, string settingKey, string value);
    Task SetGroupAsync(string groupKey, Dictionary<string, string> settings);
}
```

**Grupet e Cilësimeve:**
- `branding`: SiteName, SiteTagline, LogoUrl, FaviconUrl, PrimaryColor
- `contact`: Email, Phone, Address
- `seo`: MetaDescription, MetaKeywords
- `footer`: Description, Facebook, Instagram, Copyright

---

## 4. SignalR Hubs

### 4.1 ChatHub

Menaxhon chat-in real-time për çdo defekt.

```csharp
public class ChatHub : Hub
{
    // Bashkohu në dhomën e defektit
    public async Task JoinDefectRoom(int defectId)
    
    // Largohu nga dhoma
    public async Task LeaveDefectRoom(int defectId)
    
    // Dërgo mesazh
    public async Task SendMessage(int defectId, string content)
    
    // Sinjalo shkruajtje
    public async Task SendTypingIndicator(int defectId)
}
```

**Client Events:**
- `ReceiveMessage(message)` - Merr mesazh të ri
- `UserTyping(userId, userName)` - Dikush po shkruan
- `MessageRead(messageId)` - Mesazhi u lexua

### 4.2 NotificationHub

Menaxhon notifikimet real-time.

```csharp
public class NotificationHub : Hub
{
    // Bashkohu në kanalin personal
    public async Task JoinUserNotifications()
    
    // Merr notifikime të pa-lexuara
    public async Task GetUnreadCount()
}
```

**Client Events:**
- `ReceiveNotification(notification)` - Notifikim i ri
- `UnreadCount(count)` - Numri i pa-lexuarve

---

## 5. API Controllers

### 5.1 DashboardController

```
GET /api/dashboard/stats
Response: { totalUsers, totalDefects, totalRevenue, ... }

GET /api/dashboard/chart-data
Response: { labels: [...], datasets: [...] }
```

### 5.2 MessagesController

```
GET /api/messages/{serviceRequestId}
Response: [{ id, content, senderName, createdDate, ... }, ...]

POST /api/messages/send
Body: { serviceRequestId, content, attachmentUrl? }
Response: { success, message }
```

### 5.3 NotificationsController

```
GET /api/notifications
Response: [{ id, title, message, isRead, createdDate, ... }, ...]

POST /api/notifications/read/{id}
Response: { success }

POST /api/notifications/read-all
Response: { success, markedCount }
```

### 5.4 TechnicianInterestsController

```
GET /api/technician-interests/{serviceRequestId}
Response: [{ id, technicianName, estimatedCost, status, ... }, ...]

POST /api/technician-interests
Body: { serviceRequestId, preventiveOffer, estimatedCost, estimatedTime }
Response: { success, interestId }

PUT /api/technician-interests/{id}/accept
Response: { success }

PUT /api/technician-interests/{id}/reject  
Response: { success }
```

### 5.5 BannersController

```
GET /api/banners/active
Response: [{ id, title, imageUrl, linkUrl, position, ... }, ...]
```

### 5.6 WhatsAppController

```
POST /api/whatsapp/webhook
Body: { from, message, ... }
Response: { response }

// Komandat e mbështetura:
// RAPORT <kategori> <përshkrim>  - Krijo rast të ri
// STATUS <DEF-XXXXXX>            - Kontrollo statusin
// RASTET                         - Lista e rasteve
```

---

## 6. Faqet (Pages)

### 6.1 Faqet Publike

| Faqe | Path | Përshkrimi |
|------|------|------------|
| Home | `/` | Faqja kryesore me hero, kategori, statistika |
| About | `/About` | Rreth nesh, misioni, vizioni |
| How It Works | `/HowItWorks` | Si funksionon për qytetarë dhe teknikë |
| FAQ | `/FAQ` | Pyetje të shpeshta |
| Contact | `/Contact` | Formular kontakti |
| Technicians | `/Technicians` | Lista e teknikëve |
| TechnicianProfile | `/TechnicianProfile/{id}` | Profili i teknikut |
| Terms | `/Terms` | Kushtet e përdorimit |
| Privacy | `/Privacy` | Politika e privatësisë |
| Cookies | `/CookiePolicy` | Politika e cookies |
| Disclaimer | `/Disclaimer` | Disclaimer |

### 6.2 Faqet e Përdoruesit (Autentifikuar)

| Faqe | Path | Përshkrimi |
|------|------|------------|
| Dashboard | `/Dashboard` | Paneli i përdoruesit |
| Report Defect | `/ReportDefect` | Raporto defekt të ri |
| Defect List | `/DefectList` | Lista e defekteve |
| Defect Details | `/DefectDetails/{id}` | Detajet e defektit |
| Chat | `/Chat/{id}` | Chat për defekt |
| Messages | `/Messages` | Të gjitha bisedat |

---

## 7. Admin Panel

### 7.1 Modulet

| Modul | Path | Funksionet |
|-------|------|------------|
| **Dashboard** | `/Admin` | KPI, charts, statistika live |
| **Users** | `/Admin/Users` | Lista, CRUD, ndryshim roli, bllokim |
| **Defects** | `/Admin/Defects` | Monitorim, caktim tekniku, ndryshim statusi |
| **Technicians** | `/Admin/Technicians` | Profile, verifikim, abonime |
| **Payments** | `/Admin/Payments` | Transaksione, konfirmim manual |
| **Content (CMS)** | `/Admin/ContentManager` | Blloqe përmbajtje për çdo faqe |
| **Page Editor** | `/Admin/PageEditor` | Editor vizual (GrapesJS) |
| **Media** | `/Admin/MediaManager` | Upload, listim, fshirje skedarësh |
| **Menus** | `/Admin/MenuManager` | Navigacioni header/footer |
| **Languages** | `/Admin/LanguageManager` | Përkthime për 5 gjuhë |
| **Banners** | `/Admin/Banners` | Reklama, promocione |
| **Chat Monitor** | `/Admin/ChatMonitor` | Monitorim i të gjitha bisedave |
| **GPS Monitor** | `/Admin/GPSMonitor` | Hartë me lokacion teknikësh |
| **Settings** | `/Admin/Settings` | Branding, SEO, kontakt, footer |
| **Audit Log** | `/Admin/AuditLog` | Historia e të gjitha veprimeve |

### 7.2 Dashboard KPIs

```
┌─────────────────────────────────────────────────────────────────┐
│  👥 Total Users    🔧 Active Defects    💰 Revenue    ⭐ Rating │
│      156              23                 €4,520        4.7      │
├─────────────────────────────────────────────────────────────────┤
│  [Chart: Defects by Status]  [Chart: Revenue by Month]         │
│  [Chart: Users Growth]       [Chart: Category Distribution]    │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. Sistemi CMS

### 8.1 Content Blocks

Çdo faqe ka blloqe të editushme:

```
PageKey: "home"
├── hero_title       → "Zgjidhje e Shpejtë..."
├── hero_subtitle    → "Platforma #1 në Shqipëri..."
├── hero_cta_primary → "Raporto Defekt"
├── categories_title → "Shërbimet Tona"
└── [more blocks...]

PageKey: "about"
├── hero_title    → "Rreth TeknoSOS"
├── mission_title → "Misioni Ynë"
├── mission_text  → "Të lidhim..."
└── [more blocks...]
```

### 8.2 Page Editor (Visual)

Editor drag-and-drop me GrapesJS:
- Blloqe të gatshme (Hero, Cards, Features, Testimonials)
- CSS custom për çdo faqe
- Preview para ruajtjes
- Eksport HTML

### 8.3 Media Manager

- Upload imazhesh (jpg, png, gif, svg, webp)
- Upload dokumentesh (pdf, doc, xls)
- Organizim me folders
- File size limit: 10MB
- Image optimization automatike

---

## 9. Lokalizimi

### 9.1 Gjuhët e Mbështetura

| Kodi | Gjuha | Statusi |
|------|-------|---------|
| sq | Shqip | Primary (default) |
| en | English | Full support |
| it | Italiano | Full support |
| de | Deutsch | Full support |
| fr | Français | Full support |

### 9.2 Fallback Chain

```
Requested Language → Albanian (sq) → English (en) → Key
```

### 9.3 Language Cookie

```csharp
// Set via LanguageController
GET /Language/Set?lang=en&returnUrl=/

// Cookie name: "TeknoSOS.Language"
// Duration: 365 days
```

### 9.4 Translation Keys

```json
{
  "nav.home": "Kryefaqja",
  "nav.about": "Rreth Nesh",
  "nav.contact": "Kontakt",
  "common.submit": "Dërgo",
  "common.cancel": "Anulo",
  "defect.title": "Titulli",
  "defect.description": "Përshkrimi",
  // ...500+ keys
}
```

---

## 10. Siguria

### 10.1 Autentifikimi

- ASP.NET Core Identity
- Cookie-based authentication
- Password requirements:
  - Minimum 6 karaktere
  - Të paktën 1 shifër
  - Të paktën 1 shkronjë e madhe
  - Të paktën 1 shkronjë e vogël

### 10.2 Autorizimi

```csharp
// Role-based
[Authorize(Roles = "Admin")]

// Policy-based
[Authorize(Policy = "RequireAdminRole")]

// API returns 401/403 instead of redirect
app.UseStatusCodePagesWithReExecute("/Error/{0}");
```

### 10.3 Rolet

| Rol | Akses |
|-----|-------|
| **Admin** | Gjithçka, Admin panel |
| **Professional** | Dashboard, DefectList, Chat, Profile |
| **Citizen** | Dashboard, ReportDefect, DefectList, Chat |

### 10.4 API Security

```csharp
// API endpoints return 401 for unauthenticated
options.Events.OnRedirectToLogin = context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }
    // ...redirect for pages
};
```

### 10.5 Audit Logging

Çdo veprim administrativ regjistrohet:
- User ID, IP Address, User Agent
- Action Type (Create, Update, Delete)
- Entity Type, Entity ID
- Old Value, New Value
- Timestamp

---

## 11. Konfigurimet

### 11.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TeknoSOS;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 11.2 appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TeknoSOS;..."
  },
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://0.0.0.0:5050" },
      "Https": { "Url": "https://0.0.0.0:5051" }
    }
  }
}
```

### 11.3 Launch Profiles

```json
{
  "profiles": {
    "http": { "commandName": "Project", "launchUrl": "http://localhost:5050" },
    "https": { "commandName": "Project", "launchUrl": "https://localhost:5051" },
    "lan": { "commandName": "Project", "environmentVariables": { "ASPNETCORE_URLS": "http://0.0.0.0:5050" }}
  }
}
```

---

## 12. Deployment

### 12.1 Self-Hosting (Windows)

```powershell
# Quick start
.\START-TEKNOSOS.bat

# Or with PowerShell
.\start.ps1
```

### 12.2 Production Build

```powershell
# Build for production
dotnet publish -c Release -o ./publish

# Run published version
cd publish
dotnet TeknoSOS.WebApp.dll
```

### 12.3 Deployment Scripts

| Script | Qëllimi |
|--------|---------|
| `build-and-package.ps1` | Build dhe package për deployment |
| `deploy-teknosos.ps1` | Deploy në server të largët |
| `configure-firewall.ps1` | Konfiguro Windows Firewall |
| `setup-ssl-letsencrypt.ps1` | Instalo SSL certificate |
| `deploy-to-azure.ps1` | Deploy në Azure App Service |
| `setup-cloudflare-tunnel.ps1` | Setup Cloudflare Tunnel |
| `update-ddns-teknosos.ps1` | Update Dynamic DNS |

### 12.4 IIS Deployment

1. Install .NET 8.0 Hosting Bundle
2. Create IIS Application Pool (No Managed Code)
3. Point to published folder
4. Configure bindings (HTTP/HTTPS)
5. Set Application Pool identity permissions

### 12.5 Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY publish/ /app
WORKDIR /app
EXPOSE 5050
ENTRYPOINT ["dotnet", "TeknoSOS.WebApp.dll"]
```

---

## Appendix A: File Structure

```
TeknoSOS.WebApp/
├── .github/                    # GitHub configuration
├── .vscode/                    # VS Code settings
├── Areas/
│   └── Identity/              # ASP.NET Identity pages
│       └── Pages/Account/     # Login, Register, etc.
├── Controllers/
│   ├── Api/                   # REST API
│   │   ├── BannersController.cs
│   │   ├── DashboardController.cs
│   │   ├── MessagesController.cs
│   │   ├── NotificationsController.cs
│   │   ├── TechnicianInterestsController.cs
│   │   └── WhatsAppController.cs
│   └── LanguageController.cs  # Language switching
├── Data/
│   ├── ApplicationDbContext.cs
│   └── TestDataSeeder.cs
├── Domain/
│   ├── Entities/              # Database models
│   │   ├── ApplicationUser.cs
│   │   ├── ServiceRequest.cs
│   │   ├── TechnicianInterest.cs
│   │   ├── Message.cs
│   │   ├── Review.cs
│   │   ├── Notification.cs
│   │   ├── SubscriptionPayment.cs
│   │   ├── SiteContent.cs
│   │   ├── SiteSetting.cs
│   │   ├── MenuItem.cs
│   │   ├── MediaFile.cs
│   │   ├── AuditLog.cs
│   │   └── Banner.cs
│   └── Enums/
├── Hubs/
│   ├── ChatHub.cs
│   └── NotificationHub.cs
├── Migrations/                # EF Core migrations
├── Models/
│   └── ErrorViewModel.cs
├── Pages/
│   ├── Admin/                 # 14 admin pages
│   ├── Account/               # Account management
│   └── [Public Pages]         # 18+ public pages
├── Properties/
│   └── launchSettings.json
├── Services/
│   ├── ContentSeeder.cs
│   ├── LocalizationService.cs
│   ├── SiteContentService.cs
│   └── SiteSettingsService.cs
├── Views/
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   ├── css/site.css
│   ├── js/site.js
│   ├── images/
│   ├── uploads/
│   └── lib/
├── database/
│   └── database_complete_setup_v2.sql
├── deployment-scripts/        # 13 deployment scripts
├── docs/                      # Documentation
│   ├── DOCUMENTATION.md
│   └── StartUpAlbania/        # Business documentation
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
├── bundleconfig.json
├── Program.cs
├── TeknoSOS.WebApp.csproj
├── README.md
└── [Startup scripts]
```

---

## Appendix B: Database Schema

```sql
-- Core Tables
CREATE TABLE AspNetUsers (...)           -- Identity + Custom fields
CREATE TABLE ServiceRequests (...)       -- Defects
CREATE TABLE TechnicianInterests (...)   -- Bids
CREATE TABLE Messages (...)              -- Chat
CREATE TABLE Reviews (...)               -- Ratings
CREATE TABLE Notifications (...)         -- Alerts
CREATE TABLE SubscriptionPayments (...)  -- Payments

-- CMS Tables
CREATE TABLE SiteContent (...)           -- Page content
CREATE TABLE SiteSettings (...)          -- Site config
CREATE TABLE MenuItems (...)             -- Navigation
CREATE TABLE MediaFiles (...)            -- Uploaded files
CREATE TABLE Banners (...)               -- Promotions

-- Audit Table
CREATE TABLE AuditLogs (...)             -- Change tracking

-- Identity Tables (auto-generated)
CREATE TABLE AspNetRoles (...)
CREATE TABLE AspNetUserRoles (...)
CREATE TABLE AspNetUserClaims (...)
CREATE TABLE AspNetUserLogins (...)
CREATE TABLE AspNetUserTokens (...)
CREATE TABLE AspNetRoleClaims (...)
```

---

**© 2026 TeknoSOS. Dokumentacion Teknik.**
