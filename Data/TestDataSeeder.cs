using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Data;

public static class TestDataSeeder
{
    private static Task<ApplicationUser?> FindUserByEmailIncludingInactiveAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        string email)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);
        return db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
    }

    public static async Task SeedTestDataAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        if (await db.ServiceRequests.CountAsync() >= 10)
            return;

        Console.WriteLine("Seeding test data: 30 users + 20 defects + bids...");

        // =====================================================================
        // PROFILI I PERSONALIZUAR: ARMAND GJINI
        // =====================================================================
        var armandEmail = "armandogjini95@gmail.com";
        var armandUser = await FindUserByEmailIncludingInactiveAsync(db, userManager, armandEmail);
        if (armandUser == null)
        {
            armandUser = new ApplicationUser
            {
                UserName = armandEmail,
                Email = armandEmail,
                FirstName = "Armand",
                LastName = "Gjini",
                DisplayUsername = "armand.gjini",
                City = "Tiranë",
                Latitude = 41.3275m,
                Longitude = 19.8187m,
                EmailConfirmed = true,
                IsActive = true,
                IsAvailableForWork = true,
                RegistrationDate = DateTime.UtcNow.AddYears(-3),
                Role = UserRole.Professional,
                Bio = "Teknik i certifikuar me eksperiencë të gjerë në fushën e shërbimeve teknike, i specializuar në zgjidhjen e problemeve komplekse dhe trajnues i shumë teknikëve të rinj. I apasionuar pas teknologjisë dhe inovacionit.",
                YearsOfExperience = 10,
                CompanyName = "TeknoSOS Pro Services",
                ServiceRadiusKm = 100,
                PreferredLanguage = Language.Albanian,
                NotificationsEnabled = true,
                EmailNotificationsEnabled = true,
                AverageRating = 4.9m,
                TotalReviews = 25,
                CompletedJobsCount = 120,
                ProfileImageUrl = "/images/profiles/armand-gjini.jpg"
            };
            try
            {
                var result = await userManager.CreateAsync(armandUser, "Pro#2024");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(armandUser, "Professional");
                }
                else
                {
                    Console.WriteLine($"Warning: could not create Armand user: {string.Join(';', result.Errors.Select(e=>e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: exception creating Armand user: {ex.Message}");
            }
        }

        // Shto certifikata dhe trajnime maksimale
        var certs = new[]
        {
            ("Certifikatë Elektricist i Licencuar", "Enti Rregullator i Energjisë", "/images/certs/electrical-cert.jpg", true),
            ("Trajnim Siguria Elektrike", "Instituti i Sigurisë në Punë", null, true),
            ("Certifikatë HVAC Specialist", "Shoqata e Klimatizimit Shqiptar", "/images/certs/hvac-cert.jpg", true),
            ("CompTIA A+ Certification", "CompTIA International", "/images/certs/it-cert.jpg", true),
            ("Cisco CCNA", "Cisco Systems", null, true),
            ("Microsoft Certified: Azure Fundamentals", "Microsoft", null, false),
            ("Certifikatë Mirëmbajtje e Përgjithshme", "Qendra e Formimit Profesional", "/images/certs/general-cert.jpg", false),
            ("Trajnim Teknik LG Electronics", "LG Albania", null, false)
        };
        foreach (var cert in certs)
        {
            db.TechnicianCertificates.Add(new TechnicianCertificate
            {
                TechnicianId = armandUser.Id,
                Title = cert.Item1,
                IssuedBy = cert.Item2,
                CertificateNumber = $"CERT-{Random.Shared.Next(10000, 99999)}-{DateTime.UtcNow.Year}",
                IssueDate = DateTime.UtcNow.AddYears(-Random.Shared.Next(1, 6)),
                ExpiryDate = cert.Item4 ? DateTime.UtcNow.AddYears(Random.Shared.Next(1, 5)) : (DateTime?)null,
                DocumentUrl = cert.Item3,
                IsVerified = true
            });
        }

        // Shto 25 vlerësime me komente të ndryshme dhe vlerësim mesatar 4.9
        var reviewComments = new[]
        {
            "Shërbim i jashtëzakonshëm, shumë profesional!",
            "Armandi është teknik i shkëlqyer, e rekomandoj!",
            "Zgjidhje të shpejtë dhe të sigurt.",
            "Komunikim i qartë dhe i sjellshëm.",
            "Punë cilësore, shumë i kënaqur.",
            "Gjithmonë i disponueshëm dhe korrekt.",
            "Ekspertizë e lartë teknike.",
            "Çmim i drejtë për shërbimin.",
            "I besueshëm dhe shumë i saktë.",
            "Reagim i shpejtë ndaj kërkesave.",
            "Shumë i sjellshëm dhe i durueshëm.",
            "Punë e pastër dhe e organizuar.",
            "Rekomandoj për çdo problem teknik.",
            "Zgjidhje kreative dhe efikase.",
            "Shumë i përkushtuar ndaj klientit.",
            "Trajner i mirë për teknikë të rinj.",
            "I pajisur me certifikata të shumta.",
            "Shërbim i shpejtë dhe cilësor.",
            "I përgjegjshëm dhe korrekt.",
            "Gjithmonë i gatshëm për ndihmë.",
            "Teknik me përvojë të gjatë.",
            "I sjellshëm dhe profesional.",
            "Shumë i kënaqur nga shërbimi.",
            "Do ta rekomandoja pa hezitim.",
            "Më i miri në treg!"
        };
        for (int i = 0; i < 25; i++)
        {
            // Merr një ServiceRequest ekzistues ose krijo një të ri nëse nuk ka
            var serviceRequest = await db.ServiceRequests.FirstOrDefaultAsync();
            if (serviceRequest == null)
            {
                serviceRequest = new ServiceRequest
                {
                    UniqueCode = $"DEF-ARM-{i:D2}",
                    Title = $"Test Service for Armand {i+1}",
                    Description = "Test review for Armand Gjini.",
                    Category = TeknoSOS.WebApp.Domain.Enums.ServiceCategory.General,
                    Priority = TeknoSOS.WebApp.Domain.Enums.ServiceRequestPriority.Normal,
                    Status = TeknoSOS.WebApp.Domain.Enums.ServiceRequestStatus.Completed,
                    CitizenId = armandUser.Id,
                    ProfessionalId = armandUser.Id,
                    Location = "Tiranë",
                    ClientLatitude = 41.3275m,
                    ClientLongitude = 19.8187m,
                    CreatedDate = DateTime.UtcNow.AddDays(-i-1)
                };
                db.ServiceRequests.Add(serviceRequest);
                await db.SaveChangesAsync();
            }
            // Merr një përdorues ekzistues për ReviewerId (p.sh. qytetarin e parë)
            var reviewer = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Citizen);
            db.Reviews.Add(new Review
            {
                ServiceRequestId = serviceRequest.Id,
                ReviewerId = reviewer != null ? reviewer.Id : armandUser.Id,
                RevieweeId = armandUser.Id,
                Rating = i < 23 ? 5 : 4, // 23 me 5, 2 me 4 për mesatare 4.9
                Comment = reviewComments[i % reviewComments.Length],
                CreatedDate = DateTime.UtcNow.AddDays(-i)
            });
        }
        await db.SaveChangesAsync();

        // =====================================================================
        // CITIZENS (15 users)
        // =====================================================================
        var citizens = new List<ApplicationUser>();
        var citizenData = new[]
        {
            ("Arta", "Hoxha", "arta.hoxha", "Tiranë", 41.3275, 19.8187),
            ("Besnik", "Çela", "besnik.cela", "Durrës", 41.3233, 19.4542),
            ("Driton", "Kashi", "driton.kashi", "Vlorë", 40.4667, 19.4897),
            ("Elira", "Shehu", "elira.shehu", "Shkodër", 42.0693, 19.5126),
            ("Fatjon", "Basha", "fatjon.basha", "Korçë", 40.6186, 20.7808),
            ("Gentiana", "Dervishi", "gentiana.dervishi", "Elbasan", 41.1125, 20.0822),
            ("Hasan", "Muja", "hasan.muja", "Berat", 40.7058, 19.9522),
            ("Ilirjana", "Topi", "ilirjana.topi", "Fier", 40.7239, 19.5561),
            ("Jetmir", "Rama", "jetmir.rama", "Gjirokastër", 40.0758, 20.1389),
            ("Kaltrina", "Leka", "kaltrina.leka", "Pogradec", 40.9025, 20.6525),
            ("Luan", "Gashi", "luan.gashi", "Lushnjë", 40.9419, 19.7050),
            ("Mimoza", "Berisha", "mimoza.berisha", "Kavajë", 41.1856, 19.5569),
            ("Nertil", "Xhafa", "nertil.xhafa", "Sarandë", 39.8661, 20.0050),
            ("Ornela", "Gjoka", "ornela.gjoka", "Kukës", 42.0769, 20.4228),
            ("Petrit", "Zeneli", "petrit.zeneli", "Lezhë", 41.7836, 19.6436),
        };

        foreach (var (first, last, username, city, lat, lng) in citizenData)
        {
            var email = $"{username}@teknosos.local";
            var existing = await FindUserByEmailIncludingInactiveAsync(db, userManager, email);
            if (existing != null)
            {
                citizens.Add(existing);
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = first,
                LastName = last,
                DisplayUsername = username,
                City = city,
                Latitude = (decimal)lat,
                Longitude = (decimal)lng,
                EmailConfirmed = true,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 180)),
                Role = UserRole.Citizen,
                PreferredLanguage = Language.Albanian,
                NotificationsEnabled = true,
                EmailNotificationsEnabled = true
            };
            try
            {
                var result = await userManager.CreateAsync(user, "Citizen#2024");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Citizen");
                    citizens.Add(user);
                }
                else
                {
                    Console.WriteLine($"Warning: could not create citizen {email}: {string.Join(';', result.Errors.Select(e=>e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: exception creating citizen {email}: {ex.Message}");
            }
        }

        // =====================================================================
        // PROFESSIONALS (15 users)
        // =====================================================================
        var professionals = new List<ApplicationUser>();
        var proData = new (string First, string Last, string Username, string City, double Lat, double Lng, ServiceCategory Cat, string Bio, int Years, decimal Rate, string Company)[]
        {
            ("Agron", "Murati", "agron.murati", "Tiranë", 41.3300, 19.8200, ServiceCategory.Plumbing, "Hidraulik me 12 vjet përvojë", 12, 25m, "Hana Plumbing"),
            ("Blerim", "Vata", "blerim.vata", "Tiranë", 41.3250, 19.8150, ServiceCategory.Electrical, "Elektricist i certifikuar", 8, 30m, "VoltPro"),
            ("Çlirim", "Duka", "clirim.duka", "Durrës", 41.3200, 19.4500, ServiceCategory.HVAC, "Specialist HVAC dhe klimatizim", 15, 35m, "CoolAir Durrës"),
            ("Dritan", "Kelmendi", "dritan.kelmendi", "Tiranë", 41.3350, 19.8250, ServiceCategory.General, "Mirëmbajtje e përgjithshme", 6, 20m, "FixAll Tirana"),
            ("Ervin", "Brahimi", "ervin.brahimi", "Vlorë", 40.4650, 19.4900, ServiceCategory.Carpentry, "Marangoz profesionist", 10, 28m, "WoodCraft VL"),
            ("Fatos", "Balliu", "fatos.balliu", "Shkodër", 42.0700, 19.5100, ServiceCategory.Appliance, "Riparim elektroshtepiakesh", 7, 22m, "AppFix Shkodër"),
            ("Gëzim", "Hysa", "gezim.hysa", "Elbasan", 41.1100, 20.0800, ServiceCategory.Mechanical, "Mekanik industrial", 20, 40m, "MechPro EL"),
            ("Hamdi", "Krasniqi", "hamdi.krasniqi", "Korçë", 40.6200, 20.7800, ServiceCategory.ITTechnology, "IT Specialist", 5, 35m, "TechFix Korçë"),
            ("Ilir", "Shala", "ilir.shala", "Tiranë", 41.3280, 19.8170, ServiceCategory.Plumbing, "Hidraulik i specializuar", 14, 27m, "AquaFix TR"),
            ("Jeton", "Ahmeti", "jeton.ahmeti", "Fier", 40.7250, 19.5550, ServiceCategory.Electrical, "Instalime elektrike", 9, 25m, "PowerLine Fier"),
            ("Kastriot", "Bega", "kastriot.bega", "Berat", 40.7060, 19.9520, ServiceCategory.HVAC, "Ngrohje-Ftohje", 11, 30m, "ClimateControl BR"),
            ("Liridon", "Osmani", "liridon.osmani", "Tiranë", 41.3310, 19.8220, ServiceCategory.General, "Punë të ndryshme shtëpie", 4, 18m, "HandyMan TR"),
            ("Mentor", "Çami", "mentor.cami", "Durrës", 41.3240, 19.4530, ServiceCategory.Carpentry, "Mobileri dhe marangozeri", 13, 32m, "WoodMaster DR"),
            ("Naim", "Prifti", "naim.prifti", "Gjirokastër", 40.0760, 20.1380, ServiceCategory.Appliance, "Riparues pajisjesh", 8, 20m, "QuickFix GJ"),
            ("Olsi", "Toçi", "olsi.toci", "Pogradec", 40.9030, 20.6530, ServiceCategory.ITTechnology, "Rrjete dhe kompjutera", 6, 30m, "NetPro PG"),
        };

        foreach (var p in proData)
        {
            var email = $"{p.Username}@teknosos.local";
            var existing = await FindUserByEmailIncludingInactiveAsync(db, userManager, email);
            if (existing != null)
            {
                professionals.Add(existing);
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = p.First,
                LastName = p.Last,
                DisplayUsername = p.Username,
                City = p.City,
                Latitude = (decimal)p.Lat,
                Longitude = (decimal)p.Lng,
                EmailConfirmed = true,
                IsActive = true,
                IsAvailableForWork = true,
                RegistrationDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(60, 365)),
                Role = UserRole.Professional,
                Bio = p.Bio,
                YearsOfExperience = p.Years,
                CompanyName = p.Company,
                ServiceRadiusKm = 50,
                PreferredLanguage = Language.Albanian,
                NotificationsEnabled = true,
                EmailNotificationsEnabled = true,
                AverageRating = Math.Round(3.5m + (decimal)(Random.Shared.NextDouble() * 1.5), 2),
                TotalReviews = Random.Shared.Next(5, 50),
                CompletedJobsCount = Random.Shared.Next(10, 100)
            };
            try
            {
                var result = await userManager.CreateAsync(user, "Pro#2024");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Professional");
                    professionals.Add(user);

                    db.ProfessionalSpecialties.Add(new ProfessionalSpecialty
                    {
                        ProfessionalId = user.Id,
                        Category = p.Cat,
                        HourlyRate = p.Rate,
                        YearsOfExperience = p.Years,
                        IsVerified = Random.Shared.Next(0, 3) > 0
                    });

                    // Add certificates for each professional
                    var certTemplates = GetCertificateTemplates(p.Cat);
                    foreach (var certTemplate in certTemplates)
                    {
                        var issueDate = DateTime.UtcNow.AddYears(-Random.Shared.Next(1, 6)).AddMonths(-Random.Shared.Next(0, 12));
                        var expiryDate = certTemplate.HasExpiry ? issueDate.AddYears(Random.Shared.Next(3, 6)) : (DateTime?)null;
                        
                        db.TechnicianCertificates.Add(new TechnicianCertificate
                        {
                            TechnicianId = user.Id,
                            Title = certTemplate.Title,
                            IssuedBy = certTemplate.IssuedBy,
                            CertificateNumber = $"CERT-{Random.Shared.Next(10000, 99999)}-{DateTime.UtcNow.Year}",
                            IssueDate = issueDate,
                            ExpiryDate = expiryDate,
                            DocumentUrl = certTemplate.DocumentUrl,
                            IsVerified = Random.Shared.Next(0, 3) > 0
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: could not create professional {user.Email}: {string.Join(';', result.Errors.Select(e=>e.Description))}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: exception creating professional {user.Email}: {ex.Message}");
                // If user was created concurrently, try to fetch and add to list
                var concurrentExisting = await FindUserByEmailIncludingInactiveAsync(db, userManager, user.Email!);
                if (concurrentExisting != null)
                {
                    professionals.Add(concurrentExisting);
                }
            }
        }
        await db.SaveChangesAsync();

        if (citizens.Count == 0 || professionals.Count == 0)
        {
            Console.WriteLine("Warning: No users were created.");
            return;
        }

        // =====================================================================
        // 20 DIVERSE DEFECTS
        // =====================================================================
        var defects = new List<ServiceRequest>();

        var defectList = new List<(string Title, string Desc, ServiceCategory Cat, ServiceRequestPriority Pri, ServiceRequestStatus Stat, int CIdx, int PIdx, string Loc, double Lat, double Lng, decimal? Cost, int Days)>
        {
            ("Rubinet i thyer në kuzhinë", "Rubineti i kuzhinës ka filluar të rrjedhë shumë keq, duhet ndërruar urgjentishëm.", ServiceCategory.Plumbing, ServiceRequestPriority.High, ServiceRequestStatus.Created, 0, -1, "Rr. Myslym Shyri 45, Tiranë", 41.3265, 19.8195, null, 1),
            ("Ndërprerje elektrike në katin e dytë", "Kati i dytë i banesës nuk ka dritë prej 2 ditësh. Siguresat duken normalisht.", ServiceCategory.Electrical, ServiceRequestPriority.Emergency, ServiceRequestStatus.Created, 1, -1, "Rr. Bardhyl 12, Durrës", 41.3240, 19.4550, null, 0),
            ("Kondicioneri nuk ftoh", "Kondicioneri punon por nuk nxjerr ajër të ftohtë, vetëm ajër normal.", ServiceCategory.HVAC, ServiceRequestPriority.Normal, ServiceRequestStatus.Created, 2, -1, "Lungomarja, Vlorë", 40.4680, 19.4910, null, 3),
            ("Dera e dhomës së gjumit nuk hapet mirë", "Dera ngec gjithmonë, mentesha duket e konsumuar. Duhet ndërruar.", ServiceCategory.Carpentry, ServiceRequestPriority.Normal, ServiceRequestStatus.Created, 3, -1, "Rr. e Vilave 8, Shkodër", 42.0700, 19.5130, null, 5),
            ("Lavatriçe që nuk centrifugon", "Lavatriçja LG nuk centrifugon fare, rrobat dalin me shumë ujë.", ServiceCategory.Appliance, ServiceRequestPriority.Normal, ServiceRequestStatus.Created, 4, -1, "Blv. Fan Noli, Korçë", 40.6190, 20.7810, null, 2),
            ("WiFi router me probleme konstante", "Routeri ndërpret lidhjen çdo 10-15 minuta. E kam rinisjellur por s'ka efekt.", ServiceCategory.ITTechnology, ServiceRequestPriority.Normal, ServiceRequestStatus.Created, 5, -1, "Rr. 10 Korriku, Elbasan", 41.1130, 20.0830, null, 4),
            ("Tualeti bllokuar plotësisht", "Tualeti nuk shkarkon fare, uji mbetet brenda. Provuam pompe por nuk funksionon.", ServiceCategory.Plumbing, ServiceRequestPriority.High, ServiceRequestStatus.Matched, 6, 0, "Rr. Antipatrea, Berat", 40.7060, 19.9525, 80m, 6),
            ("Prizë që nxjerr shkëndija", "Priza në dhomën e ndenjes nxjerr shkëndija kur futim spinën. Rrezik zjarri!", ServiceCategory.Electrical, ServiceRequestPriority.Emergency, ServiceRequestStatus.Matched, 7, 1, "Rr. Kastriot, Fier", 40.7240, 19.5565, 60m, 3),
            ("Ngrohësi i ujit nuk ngroh", "Boileri nuk ngroh ujin fare. Ka 3 vjet që është blerë.", ServiceCategory.HVAC, ServiceRequestPriority.Normal, ServiceRequestStatus.Matched, 8, 2, "Rr. e Vjetër, Gjirokastër", 40.0760, 20.1390, 120m, 8),
            ("Kompjuteri nuk ndizet fare", "Kompjuteri desktop nuk ndizet. Kur shtyp butonin dëgjohet vetëm një klik.", ServiceCategory.ITTechnology, ServiceRequestPriority.Normal, ServiceRequestStatus.Matched, 9, 7, "Blv. Rilindasit, Pogradec", 40.9030, 20.6525, 50m, 5),
            ("Tubacioni i banjës rrjedh", "Tubacioni nën lavaman rrjedh, ka lagur dyshemenë. Duhet ndërruar tubi i vjetër.", ServiceCategory.Plumbing, ServiceRequestPriority.High, ServiceRequestStatus.InProgress, 10, 8, "Rr. 1 Maji, Lushnjë", 40.9420, 19.7055, 90m, 10),
            ("Instalim ndriçimi LED në lokal", "Duhet ndërruar ndriçimi i vjetër me LED. Totali 24 llampa.", ServiceCategory.Electrical, ServiceRequestPriority.Normal, ServiceRequestStatus.InProgress, 11, 9, "Rr. Republika, Kavajë", 41.1860, 19.5570, 200m, 12),
            ("Riparim dyshemeje druri", "Dyshemeja e drurit në sallon ka nisur të dalë. Disa dërrasa janë thyer.", ServiceCategory.Carpentry, ServiceRequestPriority.Normal, ServiceRequestStatus.InProgress, 12, 4, "Rr. e Portit, Sarandë", 39.8665, 20.0055, 150m, 7),
            ("Frigorifer jo funksional", "Frigoriferi Samsung nuk ftoh. Motorri punon por temperatura nuk zbret.", ServiceCategory.Appliance, ServiceRequestPriority.High, ServiceRequestStatus.InProgress, 13, 5, "Rr. e Re, Kukës", 42.0770, 20.4230, 100m, 4),
            ("Montim mobilje kuzhine", "Montim i mobiljes së re të kuzhinës: 8 dollapë, tavolinë pune.", ServiceCategory.Carpentry, ServiceRequestPriority.Normal, ServiceRequestStatus.Completed, 14, 12, "Rr. Gjergj Fishta, Lezhë", 41.7840, 19.6440, 250m, 20),
            ("Zëvendësim tubi ujësjellësi", "Ndërrimi i tubit kryesor të ujit nga rruga deri në shtëpi.", ServiceCategory.Plumbing, ServiceRequestPriority.Normal, ServiceRequestStatus.Completed, 0, 0, "Rr. Durrësit, Tiranë", 41.3290, 19.8180, 350m, 30),
            ("Instalim sistemi alarmi", "Instalim i sistemit të alarmit me 6 sensorë dhe kamerë.", ServiceCategory.Electrical, ServiceRequestPriority.Normal, ServiceRequestStatus.Completed, 1, 1, "Rr. Adriatik, Durrës", 41.3235, 19.4545, 400m, 25),
            ("Riparim pompë uji", "Pompa e ujit të pusit nuk punonte. U ndërrua motori dhe valvola.", ServiceCategory.Mechanical, ServiceRequestPriority.Normal, ServiceRequestStatus.Completed, 6, 6, "Fshati Mbreshtan, Berat", 40.7100, 19.9480, 180m, 15),
            ("Lyerje e jashtme e shtëpisë", "Duhej lyerje e fasadës por u anulua pasi ndryshuan planet.", ServiceCategory.General, ServiceRequestPriority.Normal, ServiceRequestStatus.Cancelled, 3, -1, "Rr. Bulevardi, Shkodër", 42.0695, 19.5125, null, 40),
            ("Pastrim çisternë uji", "Anuluar - çisterna u zëvendësua me të re.", ServiceCategory.Plumbing, ServiceRequestPriority.Normal, ServiceRequestStatus.Cancelled, 10, -1, "Rr. Myzeqe, Lushnjë", 40.9425, 19.7045, null, 35),
        };

        int codeCounter = await db.ServiceRequests.CountAsync();
        foreach (var d in defectList)
        {
            codeCounter++;
            var citizenId = citizens[d.CIdx % citizens.Count].Id;
            var proId = d.PIdx >= 0 ? professionals[d.PIdx % professionals.Count].Id : null;

            var sr = new ServiceRequest
            {
                UniqueCode = $"DEF-{codeCounter:D6}",
                Title = d.Title,
                Description = d.Desc,
                Category = d.Cat,
                Priority = d.Pri,
                Status = d.Stat,
                CitizenId = citizenId,
                ProfessionalId = proId,
                Location = d.Loc,
                ClientLatitude = (decimal)d.Lat,
                ClientLongitude = (decimal)d.Lng,
                EstimatedCost = d.Cost,
                FinalCost = d.Stat == ServiceRequestStatus.Completed ? d.Cost : null,
                CreatedDate = DateTime.UtcNow.AddDays(-d.Days),
                CompletedDate = d.Stat == ServiceRequestStatus.Completed ? DateTime.UtcNow.AddDays(-d.Days + 3) : null,
                ScheduledDate = (d.Stat == ServiceRequestStatus.InProgress || d.Stat == ServiceRequestStatus.Matched)
                    ? DateTime.UtcNow.AddDays(2) : null,
            };
            db.ServiceRequests.Add(sr);
            defects.Add(sr);
        }
        await db.SaveChangesAsync();

        // =====================================================================
        // TECHNICIAN INTERESTS (BIDS)
        // =====================================================================
        var createdDefects = defects.Where(d => d.Status == ServiceRequestStatus.Created).ToList();
        foreach (var defect in createdDefects)
        {
            var bidCount = Random.Shared.Next(2, 5);
            var shuffledPros = professionals.OrderBy(_ => Random.Shared.Next()).Take(bidCount).ToList();

            foreach (var pro in shuffledPros)
            {
                var alreadyExists = await db.TechnicianInterests
                    .AnyAsync(ti => ti.TechnicianId == pro.Id && ti.ServiceRequestId == defect.Id);
                if (alreadyExists) continue;

                db.TechnicianInterests.Add(new TechnicianInterest
                {
                    TechnicianId = pro.Id,
                    ServiceRequestId = defect.Id,
                    Status = InterestStatus.Interested,
                    PreventiveOffer = GetRandomOffer(defect.Category),
                    EstimatedCost = Math.Round(30m + (decimal)(Random.Shared.NextDouble() * 300), 2),
                    EstimatedTimeInHours = Random.Shared.Next(1, 24),
                    CreatedDate = defect.CreatedDate.AddHours(Random.Shared.Next(1, 48))
                });
            }
        }

        // Matched/InProgress: selected + rejected bids
        var matchedDefects = defects.Where(d => d.Status == ServiceRequestStatus.Matched || d.Status == ServiceRequestStatus.InProgress).ToList();
        foreach (var defect in matchedDefects)
        {
            if (defect.ProfessionalId == null) continue;

            var exists = await db.TechnicianInterests
                .AnyAsync(ti => ti.TechnicianId == defect.ProfessionalId && ti.ServiceRequestId == defect.Id);
            if (!exists)
            {
                db.TechnicianInterests.Add(new TechnicianInterest
                {
                    TechnicianId = defect.ProfessionalId,
                    ServiceRequestId = defect.Id,
                    Status = InterestStatus.Selected,
                    PreventiveOffer = GetRandomOffer(defect.Category),
                    EstimatedCost = defect.EstimatedCost ?? 100m,
                    EstimatedTimeInHours = Random.Shared.Next(2, 12),
                    CreatedDate = defect.CreatedDate.AddHours(Random.Shared.Next(1, 24)),
                    ResponseDate = defect.CreatedDate.AddHours(Random.Shared.Next(24, 72))
                });
            }

            var otherPros = professionals.Where(p => p.Id != defect.ProfessionalId).OrderBy(_ => Random.Shared.Next()).Take(Random.Shared.Next(1, 4));
            foreach (var pro in otherPros)
            {
                var alreadyExists = await db.TechnicianInterests
                    .AnyAsync(ti => ti.TechnicianId == pro.Id && ti.ServiceRequestId == defect.Id);
                if (alreadyExists) continue;

                db.TechnicianInterests.Add(new TechnicianInterest
                {
                    TechnicianId = pro.Id,
                    ServiceRequestId = defect.Id,
                    Status = InterestStatus.Rejected,
                    PreventiveOffer = GetRandomOffer(defect.Category),
                    EstimatedCost = Math.Round(50m + (decimal)(Random.Shared.NextDouble() * 400), 2),
                    EstimatedTimeInHours = Random.Shared.Next(2, 48),
                    CreatedDate = defect.CreatedDate.AddHours(Random.Shared.Next(1, 36)),
                    ResponseDate = defect.CreatedDate.AddHours(Random.Shared.Next(36, 72))
                });
            }
        }

        await db.SaveChangesAsync();

        // =====================================================================
        // REVIEWS for completed defects
        // =====================================================================
        var completedDefects = defects.Where(d => d.Status == ServiceRequestStatus.Completed && d.ProfessionalId != null).ToList();
        foreach (var defect in completedDefects)
        {
            var hasReview = await db.Reviews.AnyAsync(r => r.ServiceRequestId == defect.Id);
            if (hasReview) continue;

            db.Reviews.Add(new Review
            {
                ServiceRequestId = defect.Id,
                ReviewerId = defect.CitizenId,
                RevieweeId = defect.ProfessionalId!,
                Rating = Random.Shared.Next(3, 6),
                Comment = GetRandomReview(),
                CreatedDate = defect.CompletedDate!.Value.AddDays(1)
            });
        }

        // =====================================================================
        // NOTIFICATIONS
        // =====================================================================
        foreach (var defect in createdDefects)
        {
            db.Notifications.Add(new Notification
            {
                RecipientId = defect.CitizenId,
                ServiceRequestId = defect.Id,
                Title = "Kërkesa u krijua",
                Message = $"Kërkesa juaj \"{defect.Title}\" u regjistrua me sukses.",
                Type = NotificationType.CaseCreated,
                IsRead = true,
                CreatedDate = defect.CreatedDate
            });
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"Test data seeded: {citizens.Count} citizens, {professionals.Count} professionals, {defects.Count} defects with bids!");
    }

    private static string GetRandomOffer(ServiceCategory category)
    {
        var offers = new[]
        {
            "Inspektim i plotë + riparim me garanci 1 vjeçare. Material cilësor.",
            "Ndërroj pjesën e dëmtuar me material italian. Garanci 2 vjet.",
            "Diagnostikim falas + riparim brenda ditës. Transporti i përfshirë.",
            "Riparim i shpejtë me çmim të arsyeshëm, përvojë 10+ vjet.",
            "Kontroll profesional + riparim. Material të certifikuar CE.",
            "Ndërhyrje e shpejtë, garanci pune 6 muaj.",
            "Servis i plotë me testim performance dhe optimizim.",
            "Punë cilësore, çmim i drejtë, garanci e plotë."
        };
        return offers[Random.Shared.Next(offers.Length)];
    }

    private static string GetRandomReview()
    {
        var reviews = new[]
        {
            "Punë shumë e mirë! Profesionist i vërtetë, e rekomandoj.",
            "I shpejtë dhe i saktë. Çmimi ishte i drejtë.",
            "Punë cilësore, erdhi në kohë dhe përfundoi brenda afatit.",
            "Shumë i kënaqur! Do ta thërriste përsëri pa hezitim.",
            "Punë e shkëlqyer, komunikim i mirë gjatë gjithë procesit.",
            "Profesionist i besueshëm. Zgjidhja ishte efikase.",
            "Shumë faleminderit për punën! Rezultati ishte më i mirë se çprisja.",
            "I përgatitur, i sjellshëm, çmim i arsyeshëm. Top!",
        };
        return reviews[Random.Shared.Next(reviews.Length)];
    }

    // =====================================================================
    // BULK USER SEEDER - 1000 users
    // =====================================================================
    public static async Task SeedBulkUsersAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        var existingCount = await db.Users.IgnoreQueryFilters().CountAsync();
        if (existingCount >= 500)
        {
            Console.WriteLine($"Bulk seed skipped: {existingCount} users already exist.");
            return;
        }

        Console.WriteLine("Seeding 1000 users (700 citizens + 300 professionals)...");

        var firstNamesMale = new[] {
            "Agim", "Alban", "Altin", "Arben", "Ardit", "Arian", "Armand", "Astrit", "Bajram", "Bashkim",
            "Bekim", "Berat", "Besart", "Besmir", "Bledar", "Blerim", "Bujar", "Dardan", "Dorian", "Dritan",
            "Edison", "Edmond", "Elton", "Endri", "Enkel", "Erald", "Erion", "Ermal", "Ervin", "Esat",
            "Fatmir", "Fatos", "Flamur", "Florian", "Gazmend", "Gentian", "Gezim", "Gramoz", "Hasan", "Hysen",
            "Ilir", "Indrit", "Jetmir", "Julian", "Kastriot", "Klajdi", "Klodian", "Korab", "Kujtim", "Landi",
            "Ledio", "Liridon", "Lorenc", "Luan", "Lulzim", "Marjan", "Mentor", "Mergim", "Migen", "Mirgen",
            "Naim", "Nertil", "Olsi", "Orgest", "Pandi", "Pellumb", "Petrit", "Redi", "Renato", "Roland",
            "Saimir", "Sali", "Selim", "Shkelqim", "Sokol", "Spartak", "Taulant", "Trevis", "Valmir", "Viktor"
        };
        var firstNamesFemale = new[] {
            "Adelina", "Albana", "Alketa", "Alma", "Anila", "Arjola", "Arta", "Aurela", "Bardha", "Blerta",
            "Brunilda", "Denisa", "Diana", "Dorina", "Edlira", "Elda", "Elira", "Elona", "Elvira", "Ema",
            "Eni", "Entela", "Era", "Erisa", "Ermira", "Esmeralda", "Etleva", "Fatime", "Flora", "Flutura",
            "Genta", "Gentiana", "Ilda", "Ilirjana", "Iris", "Jonida", "Juliana", "Kaltrina", "Klaudia", "Kozeta",
            "Lindita", "Lorena", "Lule", "Manjola", "Marsela", "Merita", "Migena", "Mimoza", "Mirela", "Nora",
            "Odeta", "Oltiana", "Ornela", "Pranvera", "Rea", "Rovena", "Rudina", "Sara", "Sidorela", "Silva",
            "Sonila", "Suela", "Tea", "Teuta", "Valbona", "Valmira", "Vera", "Vjollca", "Xhensila", "Zamira"
        };
        var lastNames = new[] {
            "Hoxha", "Shehu", "Basha", "Berisha", "Rama", "Dervishi", "Kelmendi", "Murati", "Gashi", "Ahmeti",
            "Osmani", "Krasniqi", "Shala", "Brahimi", "Leka", "Zeneli", "Xhafa", "Gjoka", "Çela", "Duka",
            "Balliu", "Hysa", "Prifti", "Toci", "Vata", "Bega", "Çami", "Muja", "Topi", "Kashi",
            "Kurti", "Meta", "Nishani", "Topalli", "Ruci", "Caku", "Dushi", "Elezi", "Fagu", "Gega",
            "Halili", "Idrizi", "Jakupi", "Kola", "Lleshi", "Mala", "Nexhipi", "Oka", "Paci", "Qerimi",
            "Rexha", "Salihu", "Tahiri", "Uka", "Veliu", "Ymeri", "Zaimi", "Abazi", "Beqiri", "Cani",
            "Dibra", "Eshja", "Frasheri", "Gradeci", "Hasa", "Ibrahimi", "Jegeni", "Kaziu", "Laci", "Malaj"
        };
        var cities = new (string Name, double Lat, double Lng)[] {
            ("Tiranë", 41.3275, 19.8187), ("Durrës", 41.3233, 19.4542), ("Vlorë", 40.4667, 19.4897),
            ("Shkodër", 42.0693, 19.5126), ("Korçë", 40.6186, 20.7808), ("Elbasan", 41.1125, 20.0822),
            ("Berat", 40.7058, 19.9522), ("Fier", 40.7239, 19.5561), ("Gjirokastër", 40.0758, 20.1389),
            ("Pogradec", 40.9025, 20.6525), ("Lushnjë", 40.9419, 19.7050), ("Kavajë", 41.1856, 19.5569),
            ("Sarandë", 39.8661, 20.0050), ("Kukës", 42.0769, 20.4228), ("Lezhë", 41.7836, 19.6436),
            ("Peshkopi", 41.6856, 20.4311), ("Krujë", 41.5092, 19.7928), ("Tepelenë", 40.2956, 20.0189),
            ("Permet", 40.2336, 20.3517), ("Librazhd", 41.1833, 20.3167), ("Bilisht", 40.6264, 21.0006),
            ("Bulqizë", 41.4917, 20.2222), ("Gramsh", 40.8700, 20.1847), ("Roskovec", 40.7372, 19.7025),
            ("Pukë", 42.0444, 19.8972), ("Mallakastër", 40.5569, 19.7767), ("Mat", 41.5819, 20.0753),
            ("Divjakë", 40.9958, 19.5289), ("Skrapar", 40.5367, 20.2856), ("Konispol", 39.6569, 20.1353)
        };
        var categories = Enum.GetValues<ServiceCategory>();
        var bios = new[] {
            "Specialist me përvojë të gjatë në fushë", "Profesionist i certifikuar", "Punë cilësore me garanci",
            "Shërbim 24/7 në të gjithë qytetin", "Ekspert me mbi 10 vjet përvojë", "Zgjidhje të shpejta dhe efikase",
            "Çmime kompetitive dhe punë profesionale", "Riparime urgjente brenda ditës",
            "Shërbim premium me material të certifikuar", "Ekip profesional i trajnuar"
        };
        var companies = new[] {
            "FixPro", "TechService", "QuickFix", "ProRepair", "MasterCraft", "EliteService",
            "FastFix", "PremiumTech", "CityRepair", "AlbaniaFix", "TopService", "EcoFix",
            "SmartRepair", "ReliablePro", "ExpertFix", "TrustService", "QualityPro", "SpeedFix"
        };

        var allFirstNames = firstNamesMale.Concat(firstNamesFemale).ToArray();
        var usedEmails = new HashSet<string>(
            await db.Users.IgnoreQueryFilters().Select(u => u.Email!).ToListAsync(),
            StringComparer.OrdinalIgnoreCase);

        int citizenCount = 0, proCount = 0;
        int targetPros = 300;
        var batch = new List<(ApplicationUser user, string password, string role, ServiceCategory? cat, decimal? rate, int? years)>();

        for (int i = 0; i < 1000; i++)
        {
            bool isPro = i < targetPros;
            var firstNames = (i % 2 == 0) ? firstNamesMale : firstNamesFemale;
            var firstName = firstNames[Random.Shared.Next(firstNames.Length)];
            var lastName = lastNames[Random.Shared.Next(lastNames.Length)];
            var city = cities[Random.Shared.Next(cities.Length)];

            // Generate unique email
            string email;
            string username;
            int attempt = 0;
            do
            {
                var suffix = attempt == 0 ? "" : $"{attempt}";
                username = $"{firstName.ToLower()}.{lastName.ToLower()}{suffix}";
                email = $"{username}@teknosos.local";
                attempt++;
            } while (usedEmails.Contains(email));
            usedEmails.Add(email);

            var latOffset = (Random.Shared.NextDouble() - 0.5) * 0.05;
            var lngOffset = (Random.Shared.NextDouble() - 0.5) * 0.05;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                DisplayUsername = username,
                City = city.Name,
                Latitude = (decimal)(city.Lat + latOffset),
                Longitude = (decimal)(city.Lng + lngOffset),
                EmailConfirmed = true,
                IsActive = true,
                RegistrationDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365)),
                PreferredLanguage = Random.Shared.Next(3) == 0 ? Language.English : Language.Albanian,
                NotificationsEnabled = true,
                EmailNotificationsEnabled = Random.Shared.Next(2) == 0,
                LastLoginDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30))
            };

            ServiceCategory? cat = null;
            decimal? rate = null;
            int? years = null;

            if (isPro)
            {
                cat = categories[Random.Shared.Next(categories.Length)];
                years = Random.Shared.Next(1, 25);
                rate = Math.Round(15m + (decimal)(Random.Shared.NextDouble() * 35), 2);

                user.Role = UserRole.Professional;
                user.IsAvailableForWork = Random.Shared.Next(5) > 0; // 80% available
                user.Bio = bios[Random.Shared.Next(bios.Length)];
                user.YearsOfExperience = years.Value;
                user.CompanyName = $"{companies[Random.Shared.Next(companies.Length)]} {city.Name}";
                user.ServiceRadiusKm = Random.Shared.Next(10, 80);
                user.AverageRating = Math.Round(2.5m + (decimal)(Random.Shared.NextDouble() * 2.5), 2);
                user.TotalReviews = Random.Shared.Next(0, 120);
                user.CompletedJobsCount = Random.Shared.Next(0, 200);
            }
            else
            {
                user.Role = UserRole.Citizen;
            }

            batch.Add((user, isPro ? "Pro#2024" : "Citizen#2024", isPro ? "Professional" : "Citizen", cat, rate, years));

            // Process in batches of 50
            if (batch.Count >= 50 || i == 999)
            {
                foreach (var (u, pwd, role, uCat, uRate, uYears) in batch)
                {
                    var existing = await FindUserByEmailIncludingInactiveAsync(db, userManager, u.Email!);
                    if (existing != null)
                    {
                        // Ensure existing user has the expected role
                        if (!await userManager.IsInRoleAsync(existing, role))
                            await userManager.AddToRoleAsync(existing, role);

                        // If professional, ensure there is at least one specialty for them
                        if (role == "Professional")
                        {
                            var hasSpecialty = await db.ProfessionalSpecialties.AnyAsync(s => s.ProfessionalId == existing.Id);
                            if (!hasSpecialty && uCat.HasValue)
                            {
                                db.ProfessionalSpecialties.Add(new ProfessionalSpecialty
                                {
                                    ProfessionalId = existing.Id,
                                    Category = uCat.Value,
                                    HourlyRate = uRate ?? 25m,
                                    YearsOfExperience = uYears ?? 5,
                                    IsVerified = Random.Shared.Next(3) > 0
                                });
                            }

                            proCount++;
                        }
                        else
                        {
                            citizenCount++;
                        }

                        continue;
                    }

                    try
                    {
                        var result = await userManager.CreateAsync(u, pwd);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(u, role);
                            if (role == "Professional")
                            {
                                proCount++;
                                if (uCat.HasValue)
                                {
                                    db.ProfessionalSpecialties.Add(new ProfessionalSpecialty
                                    {
                                        ProfessionalId = u.Id,
                                        Category = uCat.Value,
                                        HourlyRate = uRate ?? 25m,
                                        YearsOfExperience = uYears ?? 5,
                                        IsVerified = Random.Shared.Next(3) > 0
                                    });
                                }
                            }
                            else
                            {
                                citizenCount++;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Warning: bulk create user failed for {u.Email}: {string.Join(';', result.Errors.Select(e=>e.Description))}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: exception creating bulk user {u.Email}: {ex.Message}");
                    }
                }
                await db.SaveChangesAsync();
                batch.Clear();
                Console.WriteLine($"  Progress: {i + 1}/1000 users created...");
            }
        }

        Console.WriteLine($"Bulk seed complete: {citizenCount} citizens + {proCount} professionals = {citizenCount + proCount} total new users!");
    }

    // Certificate templates by service category
    private static List<(string Title, string IssuedBy, string? DocumentUrl, bool HasExpiry)> GetCertificateTemplates(ServiceCategory category)
    {
        var templates = new List<(string Title, string IssuedBy, string? DocumentUrl, bool HasExpiry)>();
        
        switch (category)
        {
            case ServiceCategory.Plumbing:
                templates.Add(("Certifikatë Hidraulik Profesionist", "Ministria e Infrastrukturës", "/images/certs/plumbing-cert.jpg", true));
                if (Random.Shared.Next(2) == 0)
                    templates.Add(("Licencë Punëtori Ujësjellës", "Ujësjellës Kanalizime Shqipëri", null, true));
                break;
                
            case ServiceCategory.Electrical:
                templates.Add(("Certifikatë Elektricist i Licencuar", "Enti Rregullator i Energjisë", "/images/certs/electrical-cert.jpg", true));
                templates.Add(("Trajnim Siguria Elektrike", "Instituti i Sigurisë në Punë", null, true));
                break;
                
            case ServiceCategory.HVAC:
                templates.Add(("Certifikatë HVAC Specialist", "Shoqata e Klimatizimit Shqiptar", "/images/certs/hvac-cert.jpg", true));
                if (Random.Shared.Next(2) == 0)
                    templates.Add(("Licencë Gazsjellës F-Gas", "Agjencia Kombëtare e Mjedisit", null, true));
                break;
                
            case ServiceCategory.Carpentry:
                templates.Add(("Certifikatë Marangoz Profesionist", "Dhoma e Zejtarisë Tiranë", "/images/certs/carpentry-cert.jpg", false));
                break;
                
            case ServiceCategory.Appliance:
                templates.Add(("Certifikatë Riparimi Elektroshtepiakesh", "Samsung Authorized Service", "/images/certs/appliance-cert.jpg", true));
                if (Random.Shared.Next(2) == 0)
                    templates.Add(("Trajnim Teknik LG Electronics", "LG Albania", null, false));
                break;
                
            case ServiceCategory.Mechanical:
                templates.Add(("Certifikatë Mekanik Industrial", "Instituti Politeknik Tiranë", "/images/certs/mechanical-cert.jpg", false));
                templates.Add(("Licencë Operimi Makinerish", "Inspektoriati Shtetëror Teknik", null, true));
                break;
                
            case ServiceCategory.ITTechnology:
                templates.Add(("CompTIA A+ Certification", "CompTIA International", "/images/certs/it-cert.jpg", true));
                if (Random.Shared.Next(2) == 0)
                    templates.Add(("Cisco CCNA", "Cisco Systems", null, true));
                if (Random.Shared.Next(3) == 0)
                    templates.Add(("Microsoft Certified: Azure Fundamentals", "Microsoft", null, false));
                break;
                
            default: // General
                templates.Add(("Certifikatë Mirëmbajtje e Përgjithshme", "Qendra e Formimit Profesional", "/images/certs/general-cert.jpg", false));
                break;
        }
        
        return templates;
    }

    /// <summary>
    /// Seeds certificates for all existing technicians who don't have any yet.
    /// </summary>
    public static async Task SeedCertificatesForExistingTechniciansAsync(ApplicationDbContext db)
    {
        var techniciansWithoutCerts = await db.Users
            .Include(u => u.Certificates)
            .Include(u => u.Specialties)
            .Where(u => u.Role == UserRole.Professional && !u.Certificates.Any())
            .ToListAsync();

        if (!techniciansWithoutCerts.Any())
        {
            Console.WriteLine("All technicians already have certificates.");
            return;
        }

        int created = 0;
        foreach (var tech in techniciansWithoutCerts)
        {
            var primarySpecialty = tech.Specialties.FirstOrDefault();
            var category = primarySpecialty?.Category ?? ServiceCategory.General;

            var certTemplates = GetCertificateTemplates(category);
            foreach (var cert in certTemplates)
            {
                var issueDate = DateTime.UtcNow.AddYears(-Random.Shared.Next(1, 6)).AddMonths(-Random.Shared.Next(0, 12));
                var expiryDate = cert.HasExpiry ? issueDate.AddYears(Random.Shared.Next(3, 6)) : (DateTime?)null;

                db.TechnicianCertificates.Add(new TechnicianCertificate
                {
                    TechnicianId = tech.Id,
                    Title = cert.Title,
                    IssuedBy = cert.IssuedBy,
                    CertificateNumber = $"CERT-{Random.Shared.Next(10000, 99999)}-{DateTime.UtcNow.Year}",
                    IssueDate = issueDate,
                    ExpiryDate = expiryDate,
                    DocumentUrl = cert.DocumentUrl,
                    IsVerified = Random.Shared.Next(0, 3) > 0
                });
                created++;
            }
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"Seeded {created} certificates for {techniciansWithoutCerts.Count} technicians.");
    }
}
