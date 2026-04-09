# TEKNOSOS - Dokumenti Teknik i Platformës

**Versioni:** 2.0  
**Data:** Mars 2026  
**Statusi:** Gati për Prodhim

---

## 1. Përmbledhje Ekzekutive

TeknoSOS është një platformë SaaS (Software as a Service) e avancuar që lidh qytetarët me teknikë të certifikuar për shërbime mirëmbajtjeje të ndërtesave dhe shtëpive. Platforma është projektuar posaçërisht për tregun e Kosovës dhe Shqipërisë, duke adresuar nevojën kritike për një zgjidhje digjitale që modernizon sektorin e shërbimeve teknike.

### 1.1 Karakteristikat Kryesore

- **Raportim i Defekteve në Kohë Reale** me GPS dhe foto
- **Sistem Biding** për teknikët profesionistë
- **Chat në Kohë Reale** me SignalR
- **Panel Administrimi i Plotë** me 14 module menaxhimi
- **Mbështetje 5-Gjuhëshe** (Shqip, Anglisht, Italisht, Gjermanisht, Frëngjisht)
- **Integrim WhatsApp** për raportim automatik
- **Sistem Abonimesh** me 30 ditë provë falas

---

## 2. Arkitektura Teknike

### 2.1 Stack Teknologjik

| Komponenti | Teknologjia | Versioni |
|------------|-------------|----------|
| Backend | ASP.NET Core | 8.0 LTS |
| Frontend | Razor Pages + Bootstrap | 5.3.2 |
| Database | Microsoft SQL Server | 2022 |
| ORM | Entity Framework Core | 8.0 |
| Real-time | SignalR | 8.0 |
| Autentifikim | ASP.NET Identity | 8.0 |
| Charts | Chart.js | 4.x |
| Icons | Bootstrap Icons | 1.11.2 |

### 2.2 Arkitektura e Sistemit

```
┌─────────────────────────────────────────────────────────────┐
│                    FRONTEND LAYER                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │ Razor Pages │  │  Bootstrap  │  │  Chart.js   │          │
│  │   (MVC)     │  │    5.3.2    │  │   Dashbord  │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │ Controllers │  │  Services   │  │ SignalR Hubs│          │
│  │   (API)     │  │  (Business) │  │  (Real-time)│          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     DATA LAYER                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │ EF Core 8.0 │  │ SQL Server  │  │ Migrations  │          │
│  │    ORM      │  │   Database  │  │   System    │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
```

### 2.3 Struktura e Projektit

```
TeknoSOS.WebApp/
├── Areas/Identity/          # ASP.NET Identity (autentifikim)
├── Controllers/
│   ├── Api/                # REST API endpoints
│   │   ├── DashboardController.cs
│   │   ├── MessagesController.cs
│   │   ├── NotificationsController.cs
│   │   ├── TechnicianInterestsController.cs
│   │   └── WhatsAppController.cs
│   └── LanguageController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── TestDataSeeder.cs
├── Domain/
│   ├── Entities/           # 14 modele të dhënash
│   └── Enums/              # Enumerimet e sistemit
├── Hubs/
│   ├── ChatHub.cs          # SignalR për chat
│   └── NotificationHub.cs  # SignalR për njoftimet
├── Pages/
│   ├── Admin/              # 14 faqe administrimi
│   └── [Public Pages]      # 18 faqe publike
├── Services/
│   ├── LocalizationService.cs
│   ├── SiteContentService.cs
│   └── SiteSettingsService.cs
├── Views/Shared/           # Layout dhe komponente të përbashkëta
└── wwwroot/                # CSS, JS, imazhe
```

---

## 3. Modeli i të Dhënave

### 3.1 Entitetet Kryesore (14 tabela)

#### ApplicationUser (Përdoruesi)
```csharp
- Id, Email, FirstName, LastName
- Role (Citizen/Professional/Admin)
- Address, City, PostalCode, Latitude, Longitude
- ProfileImageUrl, Bio, CompanyName
- AverageRating, TotalReviews, CompletedJobsCount
- IsAvailableForWork, ServiceRadiusKm
- SubscriptionEndDate
- NotificationsEnabled, EmailNotificationsEnabled
```

#### ServiceRequest (Kërkesa për Shërbim)
```csharp
- Id, UniqueCode (DEF-XXXXXX)
- Title, Description, Category
- Priority (Low/Medium/High/Urgent)
- Status (Created/Matched/InProgress/Completed/Cancelled)
- CitizenId, ProfessionalId
- Latitude, Longitude, Address
- PhotoUrl, EstimatedCost, FinalCost
- CreatedDate, CompletedDate
```

#### TechnicianInterest (Oferta nga Tekniku)
```csharp
- Id, ServiceRequestId, TechnicianId
- ProposedPrice, EstimatedDays
- CoverLetter, Status
- CreatedDate, ResponseDate
```

#### Message (Mesazh Chat)
```csharp
- Id, SenderId, ReceiverId, ServiceRequestId
- Content, MessageType (text/image/document)
- AttachmentUrl, Status (Sent/Delivered/Read)
- CreatedDate, DeliveredDate, ReadDate
```

#### Notification (Njoftim)
```csharp
- Id, RecipientId, Title, Message
- Type (10 lloje: CaseCreated, CaseMatched, etc.)
- IsRead, ServiceRequestId
- CreatedDate
```

#### Review (Vlerësim)
```csharp
- Id, ServiceRequestId
- ReviewerId, ReviewedUserId
- Rating (1-5), Comment
- CreatedDate
```

### 3.2 Relacionet e Bazës së të Dhënave

```
ApplicationUser ──┬── ServiceRequest (Citizen)
                  ├── ServiceRequest (Professional)
                  ├── TechnicianInterest
                  ├── Message (Sender/Receiver)
                  ├── Notification
                  ├── Review (Reviewer/Reviewed)
                  ├── ProfessionalSpecialty
                  ├── TechnicianCertificate
                  ├── TechnicianPortfolio
                  └── TechnicianSubscription

ServiceRequest ──┬── TechnicianInterest
                 ├── Message
                 ├── Notification
                 └── Review
```

---

## 4. API Endpoints

### 4.1 Dashboard API (`/api/dashboard`)
| Metoda | Endpoint | Përshkrimi |
|--------|----------|------------|
| GET | `/stats` | Statistika të përgjithshme |
| GET | `/chart-data` | Të dhëna për grafikët |
| GET | `/recent-requests` | Kërkesat e fundit |

### 4.2 Notifications API (`/api/notifications`)
| Metoda | Endpoint | Përshkrimi |
|--------|----------|------------|
| GET | `/` | Lista e njoftimeve |
| GET | `/unread-count` | Numri i palexuara |
| POST | `/{id}/read` | Shëno si të lexuar |
| POST | `/read-all` | Shëno të gjitha |

### 4.3 Messages API (`/api/messages`)
| Metoda | Endpoint | Përshkrimi |
|--------|----------|------------|
| GET | `/{defectId}` | Mesazhet e një çështjeje |
| POST | `/send` | Dërgo mesazh |
| GET | `/unread-count` | Mesazhe të palexuara |

### 4.4 TechnicianInterests API (`/api/technicianinterests`)
| Metoda | Endpoint | Përshkrimi |
|--------|----------|------------|
| POST | `/` | Krijo interes |
| POST | `/{id}/accept` | Pranoj ofertën |
| POST | `/{id}/reject` | Refuzo ofertën |

### 4.5 WhatsApp API (`/api/whatsapp`)
| Metoda | Endpoint | Përshkrimi |
|--------|----------|------------|
| POST | `/webhook` | Webhook për mesazhe |
| GET | `/webhook` | Verifikim Meta |

---

## 5. Funksionalitetet Real-Time (SignalR)

### 5.1 ChatHub
```csharp
// Metodat e disponueshme
JoinDefectChat(int defectId)     // Bashkohu në chat
LeaveDefectChat(int defectId)   // Largohu nga chat
SendMessage(int defectId, string message, string type, string url, string fileName)
StartTyping(int defectId)       // Indikator shkrimit
StopTyping(int defectId)
```

### 5.2 NotificationHub
```csharp
// Metodat e disponueshme
SendNotification(userId, notification)  // Dërgo njoftim
BroadcastToAll(notification)            // Broadcast global
```

---

## 6. Siguria dhe Autentifikimi

### 6.1 ASP.NET Identity
- Autentifikim me email/password
- Rolet: **Citizen**, **Professional**, **Admin**
- Mbështetje për 2FA (Two-Factor Authentication)
- Menaxhim i sesioneve dhe cookie-ve

### 6.2 Autorizimi i bazuar në Role
```csharp
[Authorize]                    // Vetëm të loguar
[Authorize(Roles = "Admin")]   // Vetëm admin
[Authorize(Roles = "Professional")] // Vetëm profesionistë
```

### 6.3 Mbrojtja CSRF/XSS
- Anti-forgery tokens në të gjitha format
- Validimi i input-eve në server
- Enkodimi i output-eve

---

## 7. Performanca dhe Shkallëzimi

### 7.1 Optimizimet e Implementuara
- Lazy loading për relacionet
- Indekse në kolonat kryesore
- Caching i përmbajtjes statike
- Minifikimi CSS/JS

### 7.2 Metrika të Pritshme
| Metrikë | Vlera e Synuar |
|---------|----------------|
| Koha e ngarkimit | < 2 sekonda |
| Përdorues konkurrent | 1000+ |
| Uptime | 99.9% |
| Response time API | < 200ms |

---

## 8. Deployment dhe Infrastruktura

### 8.1 Kërkesat e Serverit
- **OS:** Windows Server 2019+ ose Linux
- **Runtime:** .NET 8.0 Runtime
- **Database:** SQL Server 2019+ ose Azure SQL
- **RAM:** Minimum 4GB, Rekomanduar 8GB
- **Storage:** 50GB+ për uploads

### 8.2 Opsionet e Deployment
1. **Azure App Service** (Rekomanduar)
2. **VPS/Dedicated Server**
3. **Docker Container**
4. **On-Premises**

### 8.3 CI/CD Pipeline
- Build automatik me dotnet build
- Teste automatike
- Deployment automatik në staging/production

---

## 9. Roadmap Teknik

### Faza 1 (Kompletuar) ✅
- Platforma bazë me autentifikim
- Raportim defektesh me foto/GPS
- Sistem biding për teknikë
- Chat real-time
- Panel administrimi

### Faza 2 (Q2 2026)
- Aplikacion mobil (React Native)
- Integrimi me pagesa online
- Sistem notifikimesh push
- Analytics i avancuar

### Faza 3 (Q3-Q4 2026)
- AI për kategorizim automatik
- Sistem rekomandimesh
- Zgjerim rajonal

---

## 10. Kontakt Teknik

**Ekipi i Zhvillimit TeknoSOS**  
Email: dev@teknosos.com  
Web: https://teknosos.com

---

*Ky dokument është gjeneruar automatikisht bazuar në kodin aktual të projektit TeknoSOS.*
