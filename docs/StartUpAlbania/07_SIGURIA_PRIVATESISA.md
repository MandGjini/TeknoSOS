# TEKNOSOS - Siguria dhe Privatësia

**Versioni:** 2.0  
**Data:** Mars 2026  
**Përputhshmëria:** GDPR, Ligji 9887/2008

---

## 1. Përmbledhje e Sigurisë

TeknoSOS është ndërtuar me parimin "Security by Design" - siguria nuk është një shtesë, por është e integruar në çdo aspekt të platformës.

---

## 2. Arkitektura e Sigurisë

### 2.1 Defense in Depth Model

```
┌─────────────────────────────────────────────────────────────────┐
│                    DEFENSE IN DEPTH                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ╔════════════════════════════════════════════════════════════╗ │
│  ║  LAYER 1: PERIMETER SECURITY                               ║ │
│  ║  ┌────────────────────────────────────────────────────┐    ║ │
│  ║  │ • Azure Web Application Firewall (WAF)             │    ║ │
│  ║  │ • DDoS Protection Standard                         │    ║ │
│  ║  │ • Azure Front Door                                 │    ║ │
│  ║  │ • IP Filtering & Geo-Blocking                      │    ║ │
│  ║  └────────────────────────────────────────────────────┘    ║ │
│  ╚════════════════════════════════════════════════════════════╝ │
│                             │                                    │
│  ╔════════════════════════════════════════════════════════════╗ │
│  ║  LAYER 2: NETWORK SECURITY                                 ║ │
│  ║  ┌────────────────────────────────────────────────────┐    ║ │
│  ║  │ • HTTPS/TLS 1.3 Encryption                         │    ║ │
│  ║  │ • Certificate Management (Let's Encrypt)           │    ║ │
│  ║  │ • Virtual Network Isolation                        │    ║ │
│  ║  │ • Private Endpoints                                │    ║ │
│  ║  └────────────────────────────────────────────────────┘    ║ │
│  ╚════════════════════════════════════════════════════════════╝ │
│                             │                                    │
│  ╔════════════════════════════════════════════════════════════╗ │
│  ║  LAYER 3: APPLICATION SECURITY                             ║ │
│  ║  ┌────────────────────────────────────────────────────┐    ║ │
│  ║  │ • ASP.NET Identity Authentication                  │    ║ │
│  ║  │ • Role-Based Access Control (RBAC)                 │    ║ │
│  ║  │ • Anti-Forgery Tokens (CSRF Protection)            │    ║ │
│  ║  │ • Input Validation & Sanitization                  │    ║ │
│  ║  │ • Content Security Policy Headers                  │    ║ │
│  ║  └────────────────────────────────────────────────────┘    ║ │
│  ╚════════════════════════════════════════════════════════════╝ │
│                             │                                    │
│  ╔════════════════════════════════════════════════════════════╗ │
│  ║  LAYER 4: DATA SECURITY                                    ║ │
│  ║  ┌────────────────────────────────────────────────────┐    ║ │
│  ║  │ • SQL Server Transparent Data Encryption           │    ║ │
│  ║  │ • Parameterized Queries (SQL Injection)            │    ║ │
│  ║  │ • Password Hashing (BCrypt)                        │    ║ │
│  ║  │ • Azure Blob Encryption                            │    ║ │
│  ║  └────────────────────────────────────────────────────┘    ║ │
│  ╚════════════════════════════════════════════════════════════╝ │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Autentifikimi dhe Autorizimi

### 3.1 ASP.NET Identity Implementation

```csharp
// Konfigurimi i Identity në Program.cs
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password Policy
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Lockout Policy
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User Policy
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### 3.2 Role-Based Access Control

```
┌─────────────────────────────────────────────────────────────────┐
│                    ROLE HIERARCHY                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│                      ┌─────────────────┐                         │
│                      │     ADMIN       │                         │
│                      │  Full Access    │                         │
│                      └────────┬────────┘                         │
│                               │                                  │
│           ┌───────────────────┼───────────────────┐              │
│           │                   │                   │              │
│           ▼                   ▼                   ▼              │
│   ┌───────────────┐   ┌───────────────┐   ┌───────────────┐     │
│   │  MODERATOR    │   │ PROFESSIONAL  │   │   CITIZEN     │     │
│   │  User mgmt    │   │  Services     │   │   Requests    │     │
│   └───────────────┘   └───────────────┘   └───────────────┘     │
│                                                                  │
│  PERMISSIONS MATRIX                                              │
│  ──────────────────                                             │
│  ┌─────────────┬───────┬───────────┬────────────┬─────────┐    │
│  │ Resource    │ Admin │ Moderator │Professional│ Citizen │    │
│  ├─────────────┼───────┼───────────┼────────────┼─────────┤    │
│  │ All Users   │  RWD  │    R      │     -      │    -    │    │
│  │ All Defects │  RWD  │   RW      │     R      │    -    │    │
│  │ Own Defects │  RWD  │   RW      │    RW      │   RW    │    │
│  │ Settings    │  RWD  │    -      │     -      │    -    │    │
│  │ Own Profile │  RWD  │   RW      │    RW      │   RW    │    │
│  │ Messages    │  RWD  │   RW      │    RW      │   RW    │    │
│  │ Reports     │  RWD  │    R      │     -      │    -    │    │
│  └─────────────┴───────┴───────────┴────────────┴─────────┘    │
│                                                                  │
│  R=Read, W=Write, D=Delete                                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 3.3 Session Management

```csharp
// Cookie Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;           // Prevent XSS access
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
```

---

## 4. Mbrojtja nga Sulme

### 4.1 OWASP Top 10 Protection

| Sulmi | Mbrojtja | Implementimi |
|-------|----------|--------------|
| **SQL Injection** | Parameterized queries | Entity Framework Core |
| **XSS** | Output encoding | Razor auto-encoding |
| **CSRF** | Anti-forgery tokens | [ValidateAntiForgeryToken] |
| **Broken Auth** | Identity framework | ASP.NET Identity |
| **Sensitive Data** | Encryption | TLS + DB encryption |
| **XXE** | Disabled | Not using XML parsing |
| **Broken Access** | RBAC | [Authorize] attributes |
| **Security Misconfig** | Hardened config | Production settings |
| **Insecure Deserialization** | Type checking | JSON only, typed |
| **Components** | Updated | Regular updates |

### 4.2 Input Validation

```csharp
// Example: Service Request Validation
public class ServiceRequestModel
{
    [Required(ErrorMessage = "Titulli është i detyrueshëm")]
    [StringLength(200, MinimumLength = 10)]
    [RegularExpression(@"^[a-zA-ZëËçÇ\s\d\-.,!?]+$")]
    public string Title { get; set; }
    
    [Required]
    [StringLength(2000, MinimumLength = 20)]
    public string Description { get; set; }
    
    [Range(-90, 90)]
    public double? Latitude { get; set; }
    
    [Range(-180, 180)]
    public double? Longitude { get; set; }
    
    [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg" })]
    [MaxFileSize(5 * 1024 * 1024)] // 5MB
    public IFormFile? Photo { get; set; }
}
```

### 4.3 Rate Limiting

```csharp
// API Rate Limiting Configuration
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 10;
    });
    
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(15);
    });
});
```

---

## 5. Enkriptimi i të Dhënave

### 5.1 Data at Rest

```
┌─────────────────────────────────────────────────────────────────┐
│                 ENCRYPTION AT REST                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  DATABASE (SQL Server)                                           │
│  ─────────────────────                                          │
│  • Transparent Data Encryption (TDE): AES-256                    │
│  • Column-level encryption for sensitive data                    │
│  • Encrypted backups                                             │
│                                                                  │
│  FILE STORAGE (Azure Blob)                                       │
│  ─────────────────────────                                      │
│  • Server-Side Encryption (SSE): AES-256                         │
│  • Customer-managed keys (optional)                              │
│                                                                  │
│  PASSWORDS                                                       │
│  ─────────                                                      │
│  • BCrypt hashing algorithm                                      │
│  • Salt per password                                             │
│  • Work factor: 12                                               │
│                                                                  │
│  SENSITIVE FIELDS                                                │
│  ────────────────                                               │
│  • Phone numbers: Masked display (*****1234)                     │
│  • Email: Partial mask (u***@example.com)                        │
│  • Address: Full encryption in DB                                │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 5.2 Data in Transit

```
TLS Configuration:
├── Protocol: TLS 1.3 (minimum TLS 1.2)
├── Cipher Suites: AES-256-GCM, ChaCha20
├── Certificate: Let's Encrypt / Azure-managed
├── HSTS: Enabled (max-age=31536000)
└── Certificate Pinning: Mobile apps
```

---

## 6. Privatësia dhe GDPR

### 6.1 Të Drejtat e Subjektit të të Dhënave

```
┌─────────────────────────────────────────────────────────────────┐
│                   GDPR COMPLIANCE                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ARTIKULLI 15: E DREJTA E AKSESIT                               │
│  ─────────────────────────────────                              │
│  ✓ Përdoruesi mund të shkarkoj të dhënat e tij                  │
│  ✓ Format: JSON / PDF                                            │
│  ✓ Endpoint: /Account/DownloadMyData                             │
│                                                                  │
│  ARTIKULLI 16: E DREJTA E KORRIGJIMIT                           │
│  ────────────────────────────────────                           │
│  ✓ Editim i profilit nga përdoruesi                             │
│  ✓ Request për korrigjim via support                             │
│  ✓ Endpoint: /Account/EditProfile                                │
│                                                                  │
│  ARTIKULLI 17: E DREJTA E FSHIRJES                              │
│  ─────────────────────────────────                              │
│  ✓ "Delete my account" functionality                             │
│  ✓ 30-ditë periudhë konfirmimi                                  │
│  ✓ Anonimizim vs fshirje totale                                 │
│  ✓ Endpoint: /Account/DeleteAccount                              │
│                                                                  │
│  ARTIKULLI 20: PORTABILITETI                                    │
│  ───────────────────────────                                    │
│  ✓ Export në format standard (JSON)                              │
│  ✓ Përmban të gjitha të dhënat                                  │
│  ✓ Endpoint: /Account/ExportData                                 │
│                                                                  │
│  ARTIKULLI 21: KUNDËRSHTIMI                                     │
│  ──────────────────────────                                     │
│  ✓ Opt-out nga marketing                                         │
│  ✓ Preference në profile settings                                │
│  ✓ Unsubscribe link në email                                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 6.2 Consent Management

```csharp
// Consent tracking in ApplicationUser
public class ApplicationUser : IdentityUser
{
    // Privacy consents
    public bool MarketingConsent { get; set; }
    public DateTime? MarketingConsentDate { get; set; }
    
    public bool DataProcessingConsent { get; set; }
    public DateTime DataProcessingConsentDate { get; set; }
    
    public bool ThirdPartyDataConsent { get; set; }
    public DateTime? ThirdPartyDataConsentDate { get; set; }
    
    // Cookie preferences
    public bool AnalyticsCookiesConsent { get; set; }
    public bool MarketingCookiesConsent { get; set; }
}
```

### 6.3 Data Retention Policy

| Lloji i të Dhënave | Periudha e Ruajtjes | Pas Skadimit |
|--------------------|---------------------|--------------|
| Llogari aktive | Pakufizuar | N/A |
| Llogari të fshira | 30 ditë | Anonimizim |
| Mesazhet | 2 vjet | Fshirje |
| Logs | 90 ditë | Fshirje |
| Transaksionet | 7 vjet (ligjor) | Arkivim |
| Backup | 30 ditë | Fshirje |

---

## 7. Audit dhe Logging

### 7.1 Audit Log Entity

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }        // CREATE, UPDATE, DELETE, LOGIN
    public string EntityType { get; set; }    // User, ServiceRequest, etc.
    public string EntityId { get; set; }
    public string OldValues { get; set; }     // JSON
    public string NewValues { get; set; }     // JSON
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### 7.2 Events Logged

```
┌─────────────────────────────────────────────────────────────────┐
│                    AUDIT EVENTS                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  AUTHENTICATION EVENTS                                           │
│  ─────────────────────                                          │
│  • Login success/failure                                         │
│  • Logout                                                        │
│  • Password change                                               │
│  • Password reset request                                        │
│  • Account lockout                                               │
│                                                                  │
│  AUTHORIZATION EVENTS                                            │
│  ────────────────────                                           │
│  • Access denied attempts                                        │
│  • Role changes                                                  │
│  • Permission changes                                            │
│                                                                  │
│  DATA EVENTS                                                     │
│  ───────────                                                    │
│  • Create/Update/Delete operations                               │
│  • Sensitive data access                                         │
│  • Data export requests                                          │
│  • Account deletion requests                                     │
│                                                                  │
│  ADMIN EVENTS                                                    │
│  ────────────                                                   │
│  • Settings changes                                              │
│  • User management actions                                       │
│  • Content modifications                                         │
│  • System configuration                                          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 7.3 Monitoring & Alerting

```
Alert Triggers:
├── 5+ failed login attempts → Account lockout notification
├── Unusual location login → Security alert email
├── Admin action → Audit log + notification
├── High error rate → DevOps alert
└── Potential attack → WAF block + incident log
```

---

## 8. Incident Response Plan

### 8.1 Response Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│                 INCIDENT RESPONSE                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  FAZA 1: IDENTIFIKIM (0-1 orë)                                  │
│  ─────────────────────────────                                  │
│  • Monitoring alerts                                             │
│  • User reports                                                  │
│  • Automated detection                                           │
│  • Initial assessment                                            │
│                                                                  │
│  FAZA 2: MBYLLJE (1-4 orë)                                      │
│  ─────────────────────────                                      │
│  • Izolim i sistemit të prekur                                  │
│  • Bllokimi i IP-ve të dyshimta                                 │
│  • Disable i accounts të komprometuar                           │
│  • Evidence preservation                                         │
│                                                                  │
│  FAZA 3: ELEMINIM (4-24 orë)                                    │
│  ────────────────────────────                                   │
│  • Identifikimi i root cause                                     │
│  • Patching i vulnerabilities                                    │
│  • Malware removal (if any)                                      │
│  • System hardening                                              │
│                                                                  │
│  FAZA 4: RIKUPERIM (24-72 orë)                                  │
│  ──────────────────────────────                                 │
│  • Restore from clean backups                                    │
│  • System verification                                           │
│  • Gradual service restoration                                   │
│  • Enhanced monitoring                                           │
│                                                                  │
│  FAZA 5: POST-INCIDENT (1-2 javë)                               │
│  ────────────────────────────────                               │
│  • GDPR notification (72 orë nëse duhet)                        │
│  • User notification                                             │
│  • Incident report                                               │
│  • Lessons learned                                               │
│  • Policy updates                                                │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 8.2 Contact Information

| Roli | Kontakti | Përgjegjësia |
|------|----------|--------------|
| Security Lead | security@teknosos.com | Incident lead |
| CTO | cto@teknosos.com | Technical decisions |
| Legal | legal@teknosos.com | Compliance, GDPR |
| PR | pr@teknosos.com | Communications |
| DPO | dpo@teknosos.com | Data protection |

---

## 9. Backup dhe Disaster Recovery

### 9.1 Backup Strategy

```
┌─────────────────────────────────────────────────────────────────┐
│                    BACKUP STRATEGY                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  DATABASE BACKUPS                                                │
│  ────────────────                                               │
│  ┌────────────┬───────────────┬─────────────┬────────────────┐  │
│  │ Lloji      │ Frekuenca     │ Ruajtja     │ Lokacioni      │  │
│  ├────────────┼───────────────┼─────────────┼────────────────┤  │
│  │ Full       │ Ditore 02:00  │ 30 ditë     │ Azure Geo      │  │
│  │ Diff       │ Çdo 6 orë     │ 7 ditë      │ Azure          │  │
│  │ Log        │ Çdo 15 min    │ 24 orë      │ Azure          │  │
│  └────────────┴───────────────┴─────────────┴────────────────┘  │
│                                                                  │
│  FILE BACKUPS                                                    │
│  ────────────                                                   │
│  • Azure Blob: Geo-redundant (GRS)                               │
│  • Soft delete: 14 ditë                                          │
│  • Versioning: Enabled                                           │
│                                                                  │
│  CODE & CONFIG                                                   │
│  ─────────────                                                  │
│  • GitHub: Multiple copies                                       │
│  • Secrets: Azure Key Vault + backup                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

RTO (Recovery Time Objective): 4 orë
RPO (Recovery Point Objective): 15 minuta
```

---

## 10. Security Checklist

### 10.1 Pre-Deployment

- [x] HTTPS enabled everywhere
- [x] Security headers configured
- [x] Input validation implemented
- [x] Authentication/Authorization working
- [x] Secrets in environment variables
- [x] Dependency vulnerabilities checked
- [x] Code review completed
- [x] Penetration testing (planned)

### 10.2 Ongoing

- [ ] Monthly security updates
- [ ] Quarterly penetration testing
- [ ] Annual security audit
- [ ] Continuous vulnerability scanning
- [ ] Employee security training

---

## 11. Përfundime

TeknoSOS implementon një framework të plotë sigurie që:

- **Mbron të dhënat** e përdoruesve në çdo nivel
- **Respekton privatësinë** dhe kërkesat e GDPR
- **Parandalon sulme** sipas standardeve OWASP
- **Monitoron aktivitete** me audit të plotë
- **Reagon shpejt** ndaj incidenteve

Siguria dhe privatësia janë prioritet absolut për TeknoSOS.

---

*Dokumenti i Sigurisë dhe Privatësisë - TeknoSOS v2.0 - Mars 2026*
