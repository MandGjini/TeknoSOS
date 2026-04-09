# TEKNOSOS - Arkitektura e Sistemit

**Versioni:** 2.0  
**Data:** Mars 2026  
**Statusi:** Prodhim

---

## 1. Përmbledhje e Arkitekturës

TeknoSOS përdor një arkitekturë moderne tre-shtresore (3-tier) me ndarje të qartë të përgjegjësive dhe mbështetje për shkallëzim horizontal. Sistemi është projektuar për të mbështetur mijëra përdorues konkurrent me kohë përgjigje të ulët.

---

## 2. Diagrami i Arkitekturës së Përgjithshme

```
                    ┌─────────────────────────────────────┐
                    │           KLIENTËT                   │
                    │  ┌─────┐  ┌─────┐  ┌─────────────┐  │
                    │  │ Web │  │Mobile│  │  WhatsApp   │  │
                    │  │Browser│ │ App │  │    Bot      │  │
                    │  └──┬──┘  └──┬──┘  └──────┬──────┘  │
                    └─────┼────────┼────────────┼─────────┘
                          │        │            │
                          ▼        ▼            ▼
┌─────────────────────────────────────────────────────────────────────┐
│                         LOAD BALANCER                                │
│                    (Azure Front Door / Nginx)                        │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        ▼                       ▼                       ▼
┌───────────────┐       ┌───────────────┐       ┌───────────────┐
│   WEB SERVER  │       │   WEB SERVER  │       │   WEB SERVER  │
│   Instance 1  │       │   Instance 2  │       │   Instance N  │
│ ┌───────────┐ │       │ ┌───────────┐ │       │ ┌───────────┐ │
│ │ASP.NET    │ │       │ │ASP.NET    │ │       │ │ASP.NET    │ │
│ │Core 8.0   │ │       │ │Core 8.0   │ │       │ │Core 8.0   │ │
│ └───────────┘ │       │ └───────────┘ │       │ └───────────┘ │
└───────┬───────┘       └───────┬───────┘       └───────┬───────┘
        │                       │                       │
        └───────────────────────┼───────────────────────┘
                                │
                ┌───────────────┴───────────────┐
                ▼                               ▼
        ┌───────────────┐               ┌───────────────┐
        │   SQL SERVER  │               │  AZURE BLOB   │
        │   (Primary)   │◄─────────────►│   STORAGE     │
        │               │   Replication │  (Files/Images)│
        │ ┌───────────┐ │               └───────────────┘
        │ │ Read      │ │
        │ │ Replica   │ │
        │ └───────────┘ │
        └───────────────┘
```

---

## 3. Shtresat e Aplikacionit

### 3.1 Shtresa e Prezantimit (Presentation Layer)

**Teknologjitë:**
- Razor Pages (Server-Side Rendering)
- Bootstrap 5.3.2 (UI Framework)
- Chart.js (Vizualizimet)
- SignalR Client (Real-time)

**Përgjegjësitë:**
- Rendering i faqeve HTML
- Validimi i formave në klient
- Menaxhimi i sesioneve UI
- Komunikimi real-time me serverin

**Struktura e Faqeve:**
```
Pages/
├── Index.cshtml          # Faqja kryesore
├── Dashboard.cshtml      # Paneli i përdoruesit
├── DefectList.cshtml     # Lista e defekteve
├── DefectDetails.cshtml  # Detajet e defektit
├── ReportDefect.cshtml   # Raportimi i defektit
├── Chat.cshtml           # Chat real-time
├── Messages.cshtml       # Lista e bisedave
├── Technicians.cshtml    # Lista e teknikëve
├── TechnicianProfile.cshtml # Profili i teknikut
├── Admin/                # 14 module admin
│   ├── Index.cshtml
│   ├── Dashboard.cshtml
│   ├── Users.cshtml
│   ├── Defects.cshtml
│   ├── Professionals.cshtml
│   ├── Reviews.cshtml
│   ├── Settings.cshtml
│   ├── Content.cshtml
│   ├── Menus.cshtml
│   ├── Notifications.cshtml
│   ├── Reports.cshtml
│   ├── Subscriptions.cshtml
│   ├── Audit.cshtml
│   └── WhatsApp.cshtml
└── [Legal Pages]         # Privacy, Terms, etc.
```

### 3.2 Shtresa e Logjikës së Biznesit (Business Logic Layer)

**Komponentët:**

#### Controllers (API)
```csharp
Controllers/
├── LanguageController.cs       // Ndërrimi i gjuhës
└── Api/
    ├── DashboardController.cs       // GET /api/dashboard/*
    ├── NotificationsController.cs   // GET/POST /api/notifications/*
    ├── MessagesController.cs        // GET/POST /api/messages/*
    ├── TechnicianInterestsController.cs // POST /api/technicianinterests/*
    └── WhatsAppController.cs        // POST /api/whatsapp/webhook
```

#### Services
```csharp
Services/
├── LocalizationService.cs    // 805 rreshta - Menaxhimi i përkthimeve
├── SiteContentService.cs     // CMS i dinamik
├── SiteSettingsService.cs    // Konfigurimet e sitit
└── ContentSeeder.cs          // Mbushja e përmbajtjes fillestare
```

#### SignalR Hubs
```csharp
Hubs/
├── ChatHub.cs           // Chat real-time mes përdoruesve
│   ├── JoinDefectChat()
│   ├── LeaveDefectChat()
│   ├── SendMessage()
│   ├── StartTyping()
│   └── StopTyping()
│
└── NotificationHub.cs   // Njoftimet në kohë reale
    ├── SendNotification()
    └── BroadcastToAll()
```

### 3.3 Shtresa e të Dhënave (Data Access Layer)

**Entity Framework Core 8.0**

#### ApplicationDbContext
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // DbSets (14 tabela)
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<TechnicianInterest> TechnicianInterests { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ProfessionalSpecialty> ProfessionalSpecialties { get; set; }
    public DbSet<TechnicianCertificate> TechnicianCertificates { get; set; }
    public DbSet<TechnicianPortfolio> TechnicianPortfolios { get; set; }
    public DbSet<TechnicianSubscription> TechnicianSubscriptions { get; set; }
    public DbSet<SiteContent> SiteContents { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }
    public DbSet<SiteMenu> SiteMenus { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
}
```

#### Relacionet dhe Indekset
```csharp
// Konfigurimi në OnModelCreating
modelBuilder.Entity<ServiceRequest>()
    .HasIndex(s => s.UniqueCode).IsUnique();
    
modelBuilder.Entity<ServiceRequest>()
    .HasIndex(s => s.Status);
    
modelBuilder.Entity<ServiceRequest>()
    .HasIndex(s => s.CreatedDate);
    
modelBuilder.Entity<Message>()
    .HasIndex(m => new { m.ServiceRequestId, m.CreatedDate });
    
modelBuilder.Entity<Notification>()
    .HasIndex(n => new { n.RecipientId, n.IsRead });
```

---

## 4. Fluksi i të Dhënave

### 4.1 Raportimi i Defektit

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  Qytetar │────►│ReportDefect│───►│  Kontrollo│───►│  Ruaj në │
│  (Form)  │     │   Page   │     │  Validim │     │  Database│
└──────────┘     └──────────┘     └──────────┘     └────┬─────┘
                                                        │
                                                        ▼
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│Notification│◄───│SignalR   │◄───│  Gjenero │◄───│ServiceReq│
│   Hub    │     │Broadcast │     │ UniqueCode│     │ Created  │
└────┬─────┘     └──────────┘     └──────────┘     └──────────┘
     │
     ▼
┌──────────────────────────────────────────────────────────────┐
│              TEKNIKËT E ZONËS (NOTIFIKIM)                     │
│  ┌─────┐  ┌─────┐  ┌─────┐  ┌─────┐  ┌─────┐                 │
│  │Tech1│  │Tech2│  │Tech3│  │Tech4│  │Tech5│                 │
│  └─────┘  └─────┘  └─────┘  └─────┘  └─────┘                 │
└──────────────────────────────────────────────────────────────┘
```

### 4.2 Sistemi i Bidding

```
┌──────────┐     ┌──────────┐     ┌──────────┐
│  Teknik  │────►│ Shiko    │────►│  Dërgo   │
│          │     │ Defektin │     │  Ofertë  │
└──────────┘     └──────────┘     └────┬─────┘
                                       │
                                       ▼
                               ┌──────────────┐
                               │TechnicianInt.│
                               │   Created    │
                               └──────┬───────┘
                                      │
        ┌─────────────────────────────┼─────────────────────────┐
        ▼                             ▼                         ▼
┌──────────────┐            ┌──────────────┐           ┌──────────────┐
│  Notifiko    │            │  Shfaq në    │           │  Qytetari    │
│  Qytetarin   │            │  DefectDetails│           │  Pranon/Refuzon│
└──────────────┘            └──────────────┘           └──────────────┘
```

### 4.3 Chat Real-Time

```
┌─────────┐                                           ┌─────────┐
│ User A  │                                           │ User B  │
│(Qytetar)│                                           │(Teknik) │
└────┬────┘                                           └────┬────┘
     │                                                     │
     │  1. Connect to ChatHub                              │
     │─────────────────────────────────────────────────────│
     │                                                     │
     │  2. JoinDefectChat(defectId)                        │
     │─────────────────────────────────────────────────────│
     │                                                     │
     │  3. SendMessage("Hello")                            │
     │────────────────────┐                                │
     │                    ▼                                │
     │           ┌────────────────┐                        │
     │           │   ChatHub      │                        │
     │           │ - Save to DB   │                        │
     │           │ - Broadcast    │                        │
     │           └───────┬────────┘                        │
     │                   │                                 │
     │                   │  4. ReceiveMessage(msg)         │
     │                   └────────────────────────────────►│
     │                                                     │
     │  5. ReceiveTypingIndicator                          │
     │◄────────────────────────────────────────────────────│
```

---

## 5. Siguria - Arkitektura

### 5.1 Autentifikimi dhe Autorizimi

```
┌─────────────────────────────────────────────────────────────────┐
│                    SECURITY LAYER                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │ ASP.NET      │    │ Role-based   │    │ CSRF         │       │
│  │ Identity     │    │ Authorization│    │ Protection   │       │
│  │              │    │              │    │              │       │
│  │ - Login      │    │ - [Authorize]│    │ - AntiForgery│       │
│  │ - Register   │    │ - Roles      │    │ - Tokens     │       │
│  │ - 2FA        │    │ - Policies   │    │              │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │ Password     │    │ Session      │    │ Input        │       │
│  │ Hashing      │    │ Management   │    │ Validation   │       │
│  │              │    │              │    │              │       │
│  │ - BCrypt     │    │ - Cookies    │    │ - Server     │       │
│  │ - Salting    │    │ - Timeout    │    │ - Client     │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 5.2 Matrici e Rolit-Aksesit

| Resursi | Citizen | Professional | Admin |
|---------|---------|--------------|-------|
| Faqja Kryesore | ✅ | ✅ | ✅ |
| Raporto Defekt | ✅ | ✅ | ✅ |
| Shiko Defektet e Mi | ✅ | ✅ | ✅ |
| Shiko Të Gjitha Defektet | ❌ | ✅ (zona) | ✅ |
| Dërgo Ofertë | ❌ | ✅ | ❌ |
| Pranoj Ofertë | ✅ | ❌ | ✅ |
| Chat | ✅ | ✅ | ✅ |
| Panel Admin | ❌ | ❌ | ✅ |
| Menaxho Përdoruesit | ❌ | ❌ | ✅ |
| Menaxho Përmbajtjen | ❌ | ❌ | ✅ |

---

## 6. Shkallëzimi dhe Performanca

### 6.1 Strategjia e Shkallëzimit

```
                    HORIZONTAL SCALING
    ┌─────────────────────────────────────────────┐
    │                                             │
    │   ┌─────┐   ┌─────┐   ┌─────┐   ┌─────┐   │
    │   │App 1│   │App 2│   │App 3│   │App N│   │
    │   └──┬──┘   └──┬──┘   └──┬──┘   └──┬──┘   │
    │      │         │         │         │       │
    │      └─────────┴────┬────┴─────────┘       │
    │                     │                       │
    │              ┌──────┴──────┐               │
    │              │  Redis/SQL  │               │
    │              │   Session   │               │
    │              │    Store    │               │
    │              └─────────────┘               │
    │                                             │
    └─────────────────────────────────────────────┘
```

### 6.2 Caching Strategy

| Nivel | Teknologjia | Të Dhënat |
|-------|-------------|-----------|
| Browser | Cache-Control | CSS, JS, Images |
| Application | Memory Cache | Translations, Settings |
| Database | Query Cache | Frequent Queries |
| CDN | Azure CDN | Static Assets |

### 6.3 Monitorimi

```
┌────────────────────────────────────────────────────────┐
│                   MONITORING STACK                      │
├────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │ Application │  │   Health    │  │    Error    │    │
│  │  Insights   │  │   Checks    │  │   Logging   │    │
│  │             │  │             │  │             │    │
│  │ - Metrics   │  │ - /health   │  │ - Serilog   │    │
│  │ - Traces    │  │ - DB Check  │  │ - Seq/ELK   │    │
│  │ - Logs      │  │ - External  │  │             │    │
│  └─────────────┘  └─────────────┘  └─────────────┘    │
│                                                         │
└────────────────────────────────────────────────────────┘
```

---

## 7. Deployment Architecture

### 7.1 Azure Deployment (Rekomanduar)

```
┌─────────────────────────────────────────────────────────────────┐
│                        AZURE CLOUD                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    AZURE FRONT DOOR                      │   │
│  │              (CDN + WAF + Load Balancing)                │   │
│  └───────────────────────────┬─────────────────────────────┘   │
│                              │                                   │
│  ┌───────────────────────────┼───────────────────────────┐     │
│  │                           ▼                            │     │
│  │  ┌────────────────────────────────────────────────┐   │     │
│  │  │            AZURE APP SERVICE                    │   │     │
│  │  │                                                 │   │     │
│  │  │  ┌─────────┐  ┌─────────┐  ┌─────────┐        │   │     │
│  │  │  │  Slot:  │  │  Slot:  │  │  Slot:  │        │   │     │
│  │  │  │Production│  │ Staging │  │   Dev   │        │   │     │
│  │  │  └─────────┘  └─────────┘  └─────────┘        │   │     │
│  │  │                                                 │   │     │
│  │  └────────────────────────┬───────────────────────┘   │     │
│  │                           │                            │     │
│  │  ┌────────────────────────┼───────────────────────┐   │     │
│  │  │                        ▼                        │   │     │
│  │  │  ┌─────────────┐  ┌─────────────┐             │   │     │
│  │  │  │ Azure SQL   │  │ Azure Blob  │             │   │     │
│  │  │  │ Database    │  │  Storage    │             │   │     │
│  │  │  │             │  │ (Files)     │             │   │     │
│  │  │  └─────────────┘  └─────────────┘             │   │     │
│  │  │                                                │   │     │
│  │  └────────────────────────────────────────────────┘   │     │
│  │                                                        │     │
│  └────────────────────────────────────────────────────────┘     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. Redundanca dhe Backup

### 8.1 Database Backup Strategy

| Lloji | Frekuenca | Ruajtja |
|-------|-----------|---------|
| Full Backup | Ditore | 30 ditë |
| Differential | 6 orë | 7 ditë |
| Transaction Log | 15 minuta | 24 orë |

### 8.2 Disaster Recovery

- **RPO (Recovery Point Objective):** 15 minuta
- **RTO (Recovery Time Objective):** 1 orë
- **Geo-Redundance:** Azure Paired Regions

---

## 9. Vendor Lock-in Mitigation

TeknoSOS është projektuar për të minimizuar varësinë nga një vendor i vetëm:

| Komponenti | Azure | Alternativa |
|------------|-------|-------------|
| Hosting | App Service | VPS, Docker, AWS |
| Database | Azure SQL | SQL Server, PostgreSQL |
| Storage | Blob Storage | AWS S3, MinIO |
| CDN | Azure CDN | Cloudflare, AWS CloudFront |

---

*Dokumenti i Arkitekturës - TeknoSOS v2.0 - Mars 2026*
