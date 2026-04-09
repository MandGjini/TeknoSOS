using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    /// <summary>
    /// Seeds all default CMS content for all pages and languages.
    /// This runs on first startup if no content exists.
    /// Admin can edit all content from the admin panel afterwards.
    /// </summary>
    public static class ContentSeeder
    {
        public static List<SiteContent> GetDefaultContent()
        {
            var content = new List<SiteContent>();

            // ================================================================
            // HOME PAGE - Albanian
            // ================================================================
            content.AddRange(HomePageAlbanian());
            content.AddRange(HomePageEnglish());

            // ================================================================
            // ABOUT PAGE
            // ================================================================
            content.AddRange(AboutPageAlbanian());
            content.AddRange(AboutPageEnglish());

            // ================================================================
            // HOW IT WORKS PAGE
            // ================================================================
            content.AddRange(HowItWorksPageAlbanian());
            content.AddRange(HowItWorksPageEnglish());

            // ================================================================
            // FAQ PAGE
            // ================================================================
            content.AddRange(FaqPageAlbanian());
            content.AddRange(FaqPageEnglish());

            // ================================================================
            // CONTACT PAGE
            // ================================================================
            content.AddRange(ContactPageAlbanian());
            content.AddRange(ContactPageEnglish());

            // ================================================================
            // TERMS PAGE
            // ================================================================
            content.AddRange(TermsPageAlbanian());
            content.AddRange(TermsPageEnglish());

            // ================================================================
            // PRIVACY PAGE
            // ================================================================
            content.AddRange(PrivacyPageAlbanian());
            content.AddRange(PrivacyPageEnglish());

            // ================================================================
            // COOKIE POLICY PAGE
            // ================================================================
            content.AddRange(CookiePolicyPageAlbanian());
            content.AddRange(CookiePolicyPageEnglish());

            // ================================================================
            // DISCLAIMER PAGE
            // ================================================================
            content.AddRange(DisclaimerPageAlbanian());
            content.AddRange(DisclaimerPageEnglish());

            // ================================================================
            // DATA PROCESSING PAGE
            // ================================================================
            content.AddRange(DataProcessingPageAlbanian());
            content.AddRange(DataProcessingPageEnglish());

            return content;
        }

        // ====================================================================
        // HOME PAGE - ALBANIAN
        // ====================================================================
        private static List<SiteContent> HomePageAlbanian() => new()
        {
            new() { PageKey = "home", SectionKey = "hero_title", Language = "sq", Content = "Zgjidhja juaj e besuar per sherbimet teknike", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "home", SectionKey = "hero_subtitle", Language = "sq", Content = "Raportoni defekte, gjeni profesioniste te certifikuar dhe ndiqni zgjidhjen ne kohe reale. TeknoSOS lidh qytetaret me tekniket me te mire.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "home", SectionKey = "hero_cta_primary", Language = "sq", Content = "Raporto Defekt", ContentType = "text", SortOrder = 3 },
            new() { PageKey = "home", SectionKey = "hero_cta_secondary", Language = "sq", Content = "Si Funksionon", ContentType = "text", SortOrder = 4 },
            new() { PageKey = "home", SectionKey = "hero_image", Language = "sq", ImageUrl = "/images/hero_image.jpg", Content = "", ContentType = "image", SortOrder = 5 },

            // Service Categories
            new() { PageKey = "home", SectionKey = "categories_title", Language = "sq", Content = "Kategori te Sherbimeve", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "home", SectionKey = "categories_subtitle", Language = "sq", Content = "Mbulojme nje game te gjere sherbimesh per cdo nevoje teknike", ContentType = "text", SortOrder = 11 },
            new() { PageKey = "home", SectionKey = "category_1", Language = "sq", Title = "Hidraulike", Content = "Riparime tubash, instalime sanitare, ngrohje qendrore dhe probleme uji. Tekniket tane zgjidhin cdo problem hidraulik brenda ditës.", IconClass = "bi-droplet-half", ImageUrl = "/images/category_plumbing.jpg", ContentType = "feature", SortOrder = 12 },
            new() { PageKey = "home", SectionKey = "category_2", Language = "sq", Title = "Elektrike", Content = "Instalime elektrike, riparime panelit, ndricim, priza dhe sisteme sigurie elektrike per shtepi dhe biznese.", IconClass = "bi-lightning-charge", ImageUrl = "/images/category_electrical.jpg", ContentType = "feature", SortOrder = 13 },
            new() { PageKey = "home", SectionKey = "category_3", Language = "sq", Title = "Ngrohje/Ftohje (HVAC)", Content = "Instalim dhe mirembajtje kondicioneri, ngrohje qendrore, ventilim dhe sisteme per klimatizim te ambienteve.", IconClass = "bi-thermometer-half", ImageUrl = "/images/category_hvac.jpg", ContentType = "feature", SortOrder = 14 },
            new() { PageKey = "home", SectionKey = "category_4", Language = "sq", Title = "Zdrukthtari", Content = "Prodhim mobilesh, riparime derash e dritaresh, parkete dhe pune druri me cilesi te larte profesionale.", IconClass = "bi-hammer", ImageUrl = "/images/category_carpentry.jpg", ContentType = "feature", SortOrder = 15 },
            new() { PageKey = "home", SectionKey = "category_5", Language = "sq", Title = "Pajisje Shtepie", Content = "Riparime lavatricesh, furrash, frigorifere, enelarshe dhe te gjitha pajisjeve shtëpiake.", IconClass = "bi-house-gear", ImageUrl = "/images/category_appliance.jpg", ContentType = "feature", SortOrder = 16 },
            new() { PageKey = "home", SectionKey = "category_6", Language = "sq", Title = "Sherbime te Pergjithshme", Content = "Lyerje, pastrim profesional, transport, montim mobilesh dhe sherbime te tjera teknike te perditshme.", IconClass = "bi-gear", ImageUrl = "/images/category_general.jpg", ContentType = "feature", SortOrder = 17 },

            // Why Choose Us
            new() { PageKey = "home", SectionKey = "why_title", Language = "sq", Content = "Pse te zgjidhni TeknoSOS", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "home", SectionKey = "why_subtitle", Language = "sq", Content = "Platforma jone ofron avantazhe te qarta ndaj metodave tradicionale te gjetjes se teknikeve", ContentType = "text", SortOrder = 21 },
            new() { PageKey = "home", SectionKey = "why_1", Language = "sq", Title = "Tekniket e Verifikuar", Content = "Cdo profesionist kalon nje proces verifikimi para se te pranohej ne platforme. Kontrollojme dokumentet, pervojen dhe referencat per te siguruar cilesine.", IconClass = "bi-shield-check", ContentType = "feature", SortOrder = 22 },
            new() { PageKey = "home", SectionKey = "why_2", Language = "sq", Title = "Pergjigje e Shpejte", Content = "Mesatarisht brenda 15 minutash merrni pergjigje nga profesionistet e zones suaj. Nuk keni nevoje te prisni dite per nje teknik.", IconClass = "bi-clock-history", ContentType = "feature", SortOrder = 23 },
            new() { PageKey = "home", SectionKey = "why_3", Language = "sq", Title = "Cmime Transparente", Content = "Shikoni kostot e parashikuara para se te pranoni. Nuk ka kosto te fshehura apo surpriza ne fund te punes.", IconClass = "bi-currency-exchange", ContentType = "feature", SortOrder = 24 },
            new() { PageKey = "home", SectionKey = "why_4", Language = "sq", Title = "Vleresime Reale", Content = "Lexoni vleresimet e klienteve te meparshem. Cdo vleresim eshte i verifikuar nga nje pune e perfunduar permes platformes.", IconClass = "bi-star-half", ContentType = "feature", SortOrder = 25 },

            // How It Works Steps
            new() { PageKey = "home", SectionKey = "steps_title", Language = "sq", Content = "Si Funksionon", ContentType = "text", SortOrder = 30 },
            new() { PageKey = "home", SectionKey = "step_1", Language = "sq", Title = "Raportoni Defektin", Content = "Pershkruani problemin, ngarkoni foto dhe zgjidhni kategorine. Formulari yne i thjeshte ju udheheq ne cdo hap.", IconClass = "bi-pencil-square", ContentType = "step", SortOrder = 31, Metadata = "{\"stepNumber\":\"1\"}" },
            new() { PageKey = "home", SectionKey = "step_2", Language = "sq", Title = "Merrni Oferta", Content = "Profesionistet e zones suaj shohin kerkesen dhe dergojne oferta brenda minutash. Krahasoni cmimet dhe profilen e tyre.", IconClass = "bi-people", ContentType = "step", SortOrder = 32, Metadata = "{\"stepNumber\":\"2\"}" },
            new() { PageKey = "home", SectionKey = "step_3", Language = "sq", Title = "Zgjidhni Teknikun", Content = "Shqyrtoni profilet, vleresimet dhe cmimet. Zgjidhni profesionistin qe ju pershtatet me mire per punen tuaj.", IconClass = "bi-person-check", ContentType = "step", SortOrder = 33, Metadata = "{\"stepNumber\":\"3\"}" },
            new() { PageKey = "home", SectionKey = "step_4", Language = "sq", Title = "Pune e Perfunduar", Content = "Profesionisti kryen punen. Ju vleresoni sherbimin dhe ndihmoni komunitetin te gjeje tekniket me te mire.", IconClass = "bi-check-circle", ContentType = "step", SortOrder = 34, Metadata = "{\"stepNumber\":\"4\"}" },

            // Statistics
            new() { PageKey = "home", SectionKey = "stats_title", Language = "sq", Content = "Platforma ne numra", ContentType = "text", SortOrder = 40 },
            new() { PageKey = "home", SectionKey = "stat_1", Language = "sq", Title = "500+", Content = "Profesioniste te Regjistruar", IconClass = "bi-people-fill", ContentType = "stat", SortOrder = 41 },
            new() { PageKey = "home", SectionKey = "stat_2", Language = "sq", Title = "2,000+", Content = "Pune te Perfunduara", IconClass = "bi-check-circle-fill", ContentType = "stat", SortOrder = 42 },
            new() { PageKey = "home", SectionKey = "stat_3", Language = "sq", Title = "4.8/5", Content = "Vleresimi Mesatar", IconClass = "bi-star-fill", ContentType = "stat", SortOrder = 43 },
            new() { PageKey = "home", SectionKey = "stat_4", Language = "sq", Title = "15 min", Content = "Koha Mesatare e Pergjigjes", IconClass = "bi-lightning-fill", ContentType = "stat", SortOrder = 44 },

            // Testimonials
            new() { PageKey = "home", SectionKey = "testimonials_title", Language = "sq", Content = "Qka thone klientet tane", ContentType = "text", SortOrder = 50 },
            new() { PageKey = "home", SectionKey = "testimonial_1", Language = "sq", Title = "Arben M., Tirane", Content = "Kisha nje problem me tubat e ujit ne mengjez dhe brenda ores kisha nje teknik profesional ne derë. Puna u krye shpejt dhe me cilesi. E rekomandoj pa asnje rezerve.", ContentType = "testimonial", SortOrder = 51, Metadata = "{\"rating\":\"5\",\"service\":\"Hidraulike\"}" },
            new() { PageKey = "home", SectionKey = "testimonial_2", Language = "sq", Title = "Elona K., Durres", Content = "Nuk i besoja platformave online per sherbime teknike, por TeknoSOS me bindosite. Elektricisti qe erdhi ishte profesional, i sjellshem dhe beri punen me cmim te arsyeshem. Tani e kam opsionin e pare.", ContentType = "testimonial", SortOrder = 52, Metadata = "{\"rating\":\"5\",\"service\":\"Elektrike\"}" },
            new() { PageKey = "home", SectionKey = "testimonial_3", Language = "sq", Title = "Driton H., Shkoder", Content = "Si profesionist, TeknoSOS me ka ndihmuar te gjej kliente te rinj ne zonen time. Platforma eshte e thjeshte per t'u perdorur dhe sistemi i vleresimeve me motivon te jap gjithmone sherbimin me te mire.", ContentType = "testimonial", SortOrder = 53, Metadata = "{\"rating\":\"5\",\"service\":\"Profesionist\"}" },

            // CTA
            new() { PageKey = "home", SectionKey = "cta_title", Language = "sq", Content = "Gati per te zgjidhur problemin tuaj teknik?", ContentType = "text", SortOrder = 60 },
            new() { PageKey = "home", SectionKey = "cta_subtitle", Language = "sq", Content = "Regjistrohuni falas dhe gjeni profesionistin e duhur brenda minutash. Sherbimi yne eshte i disponueshem 24 ore ne dite, 7 dite ne jave.", ContentType = "text", SortOrder = 61 },
            new() { PageKey = "home", SectionKey = "cta_button", Language = "sq", Content = "Regjistrohu Tani", ContentType = "text", SortOrder = 62 },

            // For Professionals Section
            new() { PageKey = "home", SectionKey = "pro_title", Language = "sq", Content = "Jeni Profesionist?", ContentType = "text", SortOrder = 70 },
            new() { PageKey = "home", SectionKey = "pro_subtitle", Language = "sq", Content = "Bashkohuni me qindra profesioniste te tjere qe perdorin TeknoSOS per te zgjeruar biznesin e tyre", ContentType = "text", SortOrder = 71 },
            new() { PageKey = "home", SectionKey = "pro_benefit_1", Language = "sq", Title = "Kliente te Rinj", Content = "Merrni kerkesa punes direkt ne telefonin tuaj nga kliente ne zonen tuaj gjeografike.", IconClass = "bi-geo-alt", ContentType = "feature", SortOrder = 72 },
            new() { PageKey = "home", SectionKey = "pro_benefit_2", Language = "sq", Title = "Profil Profesional", Content = "Ndertoni profilin tuaj me vleresime, foto te puneve dhe certifikata per te terhequr me shume kliente.", IconClass = "bi-person-badge", ContentType = "feature", SortOrder = 73 },
            new() { PageKey = "home", SectionKey = "pro_benefit_3", Language = "sq", Title = "Menaxhim i Lehtë", Content = "Menaxhoni punet, komunikimet me kliente dhe pagesat ne nje vend te vetem, ne menyre te thjeshte.", IconClass = "bi-kanban", ContentType = "feature", SortOrder = 74 },
        };

        // ====================================================================
        // HOME PAGE - ENGLISH
        // ====================================================================
        private static List<SiteContent> HomePageEnglish() => new()
        {
            new() { PageKey = "home", SectionKey = "hero_title", Language = "en", Content = "Your Trusted Solution for Technical Services", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "home", SectionKey = "hero_subtitle", Language = "en", Content = "Report defects, find certified professionals, and track resolution in real time. TeknoSOS connects citizens with the best technicians.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "home", SectionKey = "hero_cta_primary", Language = "en", Content = "Report Defect", ContentType = "text", SortOrder = 3 },
            new() { PageKey = "home", SectionKey = "hero_cta_secondary", Language = "en", Content = "How It Works", ContentType = "text", SortOrder = 4 },
            new() { PageKey = "home", SectionKey = "hero_image", Language = "en", ImageUrl = "/images/hero_image.jpg", Content = "", ContentType = "image", SortOrder = 5 },

            new() { PageKey = "home", SectionKey = "categories_title", Language = "en", Content = "Service Categories", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "home", SectionKey = "categories_subtitle", Language = "en", Content = "We cover a wide range of services for every technical need", ContentType = "text", SortOrder = 11 },
            new() { PageKey = "home", SectionKey = "category_1", Language = "en", Title = "Plumbing", Content = "Pipe repairs, sanitary installations, central heating, and water problems. Our technicians solve any plumbing issue within the day.", IconClass = "bi-droplet-half", ImageUrl = "/images/category_plumbing.jpg", ContentType = "feature", SortOrder = 12 },
            new() { PageKey = "home", SectionKey = "category_2", Language = "en", Title = "Electrical", Content = "Electrical installations, panel repairs, lighting, outlets, and electrical security systems for homes and businesses.", IconClass = "bi-lightning-charge", ImageUrl = "/images/category_electrical.jpg", ContentType = "feature", SortOrder = 13 },
            new() { PageKey = "home", SectionKey = "category_3", Language = "en", Title = "HVAC", Content = "Installation and maintenance of air conditioners, central heating, ventilation, and climate control systems for all environments.", IconClass = "bi-thermometer-half", ImageUrl = "/images/category_hvac.jpg", ContentType = "feature", SortOrder = 14 },
            new() { PageKey = "home", SectionKey = "category_4", Language = "en", Title = "Carpentry", Content = "Furniture production, door and window repairs, flooring, and high-quality professional woodwork.", IconClass = "bi-hammer", ImageUrl = "/images/category_carpentry.jpg", ContentType = "feature", SortOrder = 15 },
            new() { PageKey = "home", SectionKey = "category_5", Language = "en", Title = "Appliances", Content = "Repairs for washing machines, ovens, refrigerators, dishwashers, and all household appliances.", IconClass = "bi-house-gear", ImageUrl = "/images/category_appliance.jpg", ContentType = "feature", SortOrder = 16 },
            new() { PageKey = "home", SectionKey = "category_6", Language = "en", Title = "General Services", Content = "Painting, professional cleaning, transport, furniture assembly, and other everyday technical services.", IconClass = "bi-gear", ImageUrl = "/images/category_general.jpg", ContentType = "feature", SortOrder = 17 },

            new() { PageKey = "home", SectionKey = "why_title", Language = "en", Content = "Why Choose TeknoSOS", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "home", SectionKey = "why_subtitle", Language = "en", Content = "Our platform offers clear advantages over traditional methods of finding technicians", ContentType = "text", SortOrder = 21 },
            new() { PageKey = "home", SectionKey = "why_1", Language = "en", Title = "Verified Technicians", Content = "Every professional goes through a verification process before being accepted on the platform. We check documents, experience, and references to ensure quality.", IconClass = "bi-shield-check", ContentType = "feature", SortOrder = 22 },
            new() { PageKey = "home", SectionKey = "why_2", Language = "en", Title = "Fast Response", Content = "On average, you receive responses from professionals in your area within 15 minutes. No need to wait days for a technician.", IconClass = "bi-clock-history", ContentType = "feature", SortOrder = 23 },
            new() { PageKey = "home", SectionKey = "why_3", Language = "en", Title = "Transparent Pricing", Content = "See estimated costs before accepting. No hidden fees or surprises at the end of the job.", IconClass = "bi-currency-exchange", ContentType = "feature", SortOrder = 24 },
            new() { PageKey = "home", SectionKey = "why_4", Language = "en", Title = "Real Reviews", Content = "Read reviews from previous clients. Every review is verified from a job completed through the platform.", IconClass = "bi-star-half", ContentType = "feature", SortOrder = 25 },

            new() { PageKey = "home", SectionKey = "steps_title", Language = "en", Content = "How It Works", ContentType = "text", SortOrder = 30 },
            new() { PageKey = "home", SectionKey = "step_1", Language = "en", Title = "Report the Defect", Content = "Describe the problem, upload photos, and select the category. Our simple form guides you through every step.", IconClass = "bi-pencil-square", ContentType = "step", SortOrder = 31, Metadata = "{\"stepNumber\":\"1\"}" },
            new() { PageKey = "home", SectionKey = "step_2", Language = "en", Title = "Receive Offers", Content = "Professionals in your area see the request and send offers within minutes. Compare prices and profiles.", IconClass = "bi-people", ContentType = "step", SortOrder = 32, Metadata = "{\"stepNumber\":\"2\"}" },
            new() { PageKey = "home", SectionKey = "step_3", Language = "en", Title = "Choose a Technician", Content = "Review profiles, ratings, and prices. Select the professional that best fits your needs.", IconClass = "bi-person-check", ContentType = "step", SortOrder = 33, Metadata = "{\"stepNumber\":\"3\"}" },
            new() { PageKey = "home", SectionKey = "step_4", Language = "en", Title = "Job Completed", Content = "The professional completes the work. You rate the service and help the community find the best technicians.", IconClass = "bi-check-circle", ContentType = "step", SortOrder = 34, Metadata = "{\"stepNumber\":\"4\"}" },

            new() { PageKey = "home", SectionKey = "stats_title", Language = "en", Content = "Platform in Numbers", ContentType = "text", SortOrder = 40 },
            new() { PageKey = "home", SectionKey = "stat_1", Language = "en", Title = "500+", Content = "Registered Professionals", IconClass = "bi-people-fill", ContentType = "stat", SortOrder = 41 },
            new() { PageKey = "home", SectionKey = "stat_2", Language = "en", Title = "2,000+", Content = "Completed Jobs", IconClass = "bi-check-circle-fill", ContentType = "stat", SortOrder = 42 },
            new() { PageKey = "home", SectionKey = "stat_3", Language = "en", Title = "4.8/5", Content = "Average Rating", IconClass = "bi-star-fill", ContentType = "stat", SortOrder = 43 },
            new() { PageKey = "home", SectionKey = "stat_4", Language = "en", Title = "15 min", Content = "Average Response Time", IconClass = "bi-lightning-fill", ContentType = "stat", SortOrder = 44 },

            new() { PageKey = "home", SectionKey = "testimonials_title", Language = "en", Content = "What Our Clients Say", ContentType = "text", SortOrder = 50 },
            new() { PageKey = "home", SectionKey = "testimonial_1", Language = "en", Title = "Arben M., Tirana", Content = "I had a pipe problem in the morning and within an hour I had a professional technician at my door. The work was done quickly and with quality. I recommend without any reservation.", ContentType = "testimonial", SortOrder = 51, Metadata = "{\"rating\":\"5\",\"service\":\"Plumbing\"}" },
            new() { PageKey = "home", SectionKey = "testimonial_2", Language = "en", Title = "Elona K., Durres", Content = "I did not trust online platforms for technical services, but TeknoSOS convinced me. The electrician was professional, polite, and did the job at a reasonable price. Now it is my first option.", ContentType = "testimonial", SortOrder = 52, Metadata = "{\"rating\":\"5\",\"service\":\"Electrical\"}" },
            new() { PageKey = "home", SectionKey = "testimonial_3", Language = "en", Title = "Driton H., Shkoder", Content = "As a professional, TeknoSOS has helped me find new clients in my area. The platform is simple to use and the rating system motivates me to always provide the best service.", ContentType = "testimonial", SortOrder = 53, Metadata = "{\"rating\":\"5\",\"service\":\"Professional\"}" },

            new() { PageKey = "home", SectionKey = "cta_title", Language = "en", Content = "Ready to solve your technical problem?", ContentType = "text", SortOrder = 60 },
            new() { PageKey = "home", SectionKey = "cta_subtitle", Language = "en", Content = "Register for free and find the right professional within minutes. Our service is available 24 hours a day, 7 days a week.", ContentType = "text", SortOrder = 61 },
            new() { PageKey = "home", SectionKey = "cta_button", Language = "en", Content = "Register Now", ContentType = "text", SortOrder = 62 },

            new() { PageKey = "home", SectionKey = "pro_title", Language = "en", Content = "Are You a Professional?", ContentType = "text", SortOrder = 70 },
            new() { PageKey = "home", SectionKey = "pro_subtitle", Language = "en", Content = "Join hundreds of other professionals who use TeknoSOS to expand their business", ContentType = "text", SortOrder = 71 },
            new() { PageKey = "home", SectionKey = "pro_benefit_1", Language = "en", Title = "New Clients", Content = "Receive work requests directly on your phone from clients in your geographic area.", IconClass = "bi-geo-alt", ContentType = "feature", SortOrder = 72 },
            new() { PageKey = "home", SectionKey = "pro_benefit_2", Language = "en", Title = "Professional Profile", Content = "Build your profile with reviews, work photos, and certificates to attract more clients.", IconClass = "bi-person-badge", ContentType = "feature", SortOrder = 73 },
            new() { PageKey = "home", SectionKey = "pro_benefit_3", Language = "en", Title = "Easy Management", Content = "Manage jobs, client communications, and payments in one simple place.", IconClass = "bi-kanban", ContentType = "feature", SortOrder = 74 },
        };

        // ====================================================================
        // ABOUT PAGE - ALBANIAN
        // ====================================================================
        private static List<SiteContent> AboutPageAlbanian() => new()
        {
            new() { PageKey = "about", SectionKey = "hero_title", Language = "sq", Content = "Rreth TeknoSOS", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "about", SectionKey = "hero_subtitle", Language = "sq", Content = "Platforma qe po transformon menyrën si qytetarët gjejnë dhe angazhojnë sherbimet teknike ne Shqiperi.", ContentType = "text", SortOrder = 2 },

            new() { PageKey = "about", SectionKey = "mission_title", Language = "sq", Content = "Misioni Yne", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "about", SectionKey = "mission_text", Language = "sq", Content = "TeknoSOS u krijua me nje qellim te qarte: te beje me te lehte, me te shpejtë dhe me transparente procesin e gjetjes se profesionisteve teknike te besueshem. Ne besojme se cdo qytetar meriton qasje te shpejtë ne sherbime cilesore, dhe cdo profesionist meriton nje platforme te drejte per te treguar aftesite e tij.", ContentType = "html", SortOrder = 11 },

            new() { PageKey = "about", SectionKey = "vision_title", Language = "sq", Content = "Vizioni Yne", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "about", SectionKey = "vision_text", Language = "sq", Content = "Synojme te behemi platforma kryesore per sherbimet teknike ne rajon, duke ndertuar nje rrjet besimi ndermjet qytetareve dhe profesionisteve. Vizioni yne eshte nje bote ku cdo problem teknik ka nje zgjidhje profesionale vetem disa klikime larg.", ContentType = "html", SortOrder = 21 },

            new() { PageKey = "about", SectionKey = "values_title", Language = "sq", Content = "Vlerat Tona", ContentType = "text", SortOrder = 30 },
            new() { PageKey = "about", SectionKey = "value_1", Language = "sq", Title = "Transparence", Content = "Cmimet, vleresimet dhe informacioni jane gjithmone te dukshme. Nuk ka kosto te fshehura apo surpriza.", IconClass = "bi-eye", ContentType = "feature", SortOrder = 31 },
            new() { PageKey = "about", SectionKey = "value_2", Language = "sq", Title = "Cilesi", Content = "Pranojme ne platforme vetem profesioniste te verifikuar qe plotesojne standardet tona te larta.", IconClass = "bi-award", ContentType = "feature", SortOrder = 32 },
            new() { PageKey = "about", SectionKey = "value_3", Language = "sq", Title = "Besueshmeri", Content = "Ndertojme besim permes vleresimeve reale, verifikimit te profesionisteve dhe mbeshtetjes se vazhdueshme.", IconClass = "bi-shield-check", ContentType = "feature", SortOrder = 33 },
            new() { PageKey = "about", SectionKey = "value_4", Language = "sq", Title = "Inovacion", Content = "Perdorim teknologjine per te thjeshezuar procesin dhe per te ofruar pervoje te shkëlqyer per te gjithe.", IconClass = "bi-lightbulb", ContentType = "feature", SortOrder = 34 },

            new() { PageKey = "about", SectionKey = "team_title", Language = "sq", Content = "Ekipi Yne", ContentType = "text", SortOrder = 40 },
            new() { PageKey = "about", SectionKey = "team_text", Language = "sq", Content = "TeknoSOS eshte ndertuar nga nje ekip i perkushtuar profesionistesh te teknologjise dhe industrise se sherbimeve, te bashkuar nga deshira per te permiresuar cilesine e jetës se qytetareve permes teknologjise.", ContentType = "html", SortOrder = 41 },

            new() { PageKey = "about", SectionKey = "numbers_title", Language = "sq", Content = "TeknoSOS ne Numra", ContentType = "text", SortOrder = 50 },
            new() { PageKey = "about", SectionKey = "number_1", Language = "sq", Title = "500+", Content = "Profesioniste te Regjistruar", ContentType = "stat", SortOrder = 51 },
            new() { PageKey = "about", SectionKey = "number_2", Language = "sq", Title = "10,000+", Content = "Kerkesa te Procesuara", ContentType = "stat", SortOrder = 52 },
            new() { PageKey = "about", SectionKey = "number_3", Language = "sq", Title = "50+", Content = "Qytete te Mbuluara", ContentType = "stat", SortOrder = 53 },
            new() { PageKey = "about", SectionKey = "number_4", Language = "sq", Title = "98%", Content = "Klientë te Kenaqur", ContentType = "stat", SortOrder = 54 },

            new() { PageKey = "about", SectionKey = "partners_title", Language = "sq", Content = "Partnere dhe Bashkepunime", ContentType = "text", SortOrder = 60 },
            new() { PageKey = "about", SectionKey = "partners_text", Language = "sq", Content = "Bashkepunojme me organizata profesionale, institucione arsimore dhe ente qeveritare per te ngritur standardet e sherbimeve teknike ne Shqiperi. Programet tona te certifikimit ndihmojna profesionistet te zhvillohen vazhdimisht.", ContentType = "html", SortOrder = 61 },
        };

        // ====================================================================
        // ABOUT PAGE - ENGLISH
        // ====================================================================
        private static List<SiteContent> AboutPageEnglish() => new()
        {
            new() { PageKey = "about", SectionKey = "hero_title", Language = "en", Content = "About TeknoSOS", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "about", SectionKey = "hero_subtitle", Language = "en", Content = "The platform that is transforming how citizens find and engage technical services in Albania.", ContentType = "text", SortOrder = 2 },

            new() { PageKey = "about", SectionKey = "mission_title", Language = "en", Content = "Our Mission", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "about", SectionKey = "mission_text", Language = "en", Content = "TeknoSOS was created with a clear goal: to make the process of finding trustworthy technical professionals easier, faster, and more transparent. We believe every citizen deserves quick access to quality services, and every professional deserves a fair platform to showcase their skills.", ContentType = "html", SortOrder = 11 },

            new() { PageKey = "about", SectionKey = "vision_title", Language = "en", Content = "Our Vision", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "about", SectionKey = "vision_text", Language = "en", Content = "We aim to become the leading platform for technical services in the region, building a network of trust between citizens and professionals. Our vision is a world where every technical problem has a professional solution just a few clicks away.", ContentType = "html", SortOrder = 21 },

            new() { PageKey = "about", SectionKey = "values_title", Language = "en", Content = "Our Values", ContentType = "text", SortOrder = 30 },
            new() { PageKey = "about", SectionKey = "value_1", Language = "en", Title = "Transparency", Content = "Prices, reviews, and information are always visible. No hidden costs or surprises.", IconClass = "bi-eye", ContentType = "feature", SortOrder = 31 },
            new() { PageKey = "about", SectionKey = "value_2", Language = "en", Title = "Quality", Content = "We only accept verified professionals who meet our high standards.", IconClass = "bi-award", ContentType = "feature", SortOrder = 32 },
            new() { PageKey = "about", SectionKey = "value_3", Language = "en", Title = "Trust", Content = "We build trust through real reviews, professional verification, and continuous support.", IconClass = "bi-shield-check", ContentType = "feature", SortOrder = 33 },
            new() { PageKey = "about", SectionKey = "value_4", Language = "en", Title = "Innovation", Content = "We use technology to simplify the process and provide an excellent experience for everyone.", IconClass = "bi-lightbulb", ContentType = "feature", SortOrder = 34 },

            new() { PageKey = "about", SectionKey = "team_title", Language = "en", Content = "Our Team", ContentType = "text", SortOrder = 40 },
            new() { PageKey = "about", SectionKey = "team_text", Language = "en", Content = "TeknoSOS is built by a dedicated team of technology and service industry professionals, united by the desire to improve citizens' quality of life through technology.", ContentType = "html", SortOrder = 41 },

            new() { PageKey = "about", SectionKey = "numbers_title", Language = "en", Content = "TeknoSOS in Numbers", ContentType = "text", SortOrder = 50 },
            new() { PageKey = "about", SectionKey = "number_1", Language = "en", Title = "500+", Content = "Registered Professionals", ContentType = "stat", SortOrder = 51 },
            new() { PageKey = "about", SectionKey = "number_2", Language = "en", Title = "10,000+", Content = "Processed Requests", ContentType = "stat", SortOrder = 52 },
            new() { PageKey = "about", SectionKey = "number_3", Language = "en", Title = "50+", Content = "Cities Covered", ContentType = "stat", SortOrder = 53 },
            new() { PageKey = "about", SectionKey = "number_4", Language = "en", Title = "98%", Content = "Satisfied Clients", ContentType = "stat", SortOrder = 54 },

            new() { PageKey = "about", SectionKey = "partners_title", Language = "en", Content = "Partners and Collaborations", ContentType = "text", SortOrder = 60 },
            new() { PageKey = "about", SectionKey = "partners_text", Language = "en", Content = "We collaborate with professional organizations, educational institutions, and government entities to raise the standards of technical services in Albania. Our certification programs help professionals develop continuously.", ContentType = "html", SortOrder = 61 },
        };

        // ====================================================================
        // HOW IT WORKS PAGE - ALBANIAN
        // ====================================================================
        private static List<SiteContent> HowItWorksPageAlbanian() => new()
        {
            new() { PageKey = "howitworks", SectionKey = "hero_title", Language = "sq", Content = "Si Funksionon TeknoSOS", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "howitworks", SectionKey = "hero_subtitle", Language = "sq", Content = "Procesi yne i thjeshte ju con nga problemi tek zgjidhja ne vetem disa hapa.", ContentType = "text", SortOrder = 2 },

            // For Citizens
            new() { PageKey = "howitworks", SectionKey = "citizen_title", Language = "sq", Content = "Per Qytetaret", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_1", Language = "sq", Title = "Hapi 1: Krijoni Llogarine", Content = "Regjistrohuni falas duke plotesuar formularin e regjistrimit me te dhenat tuaja baze. Procesi merr vetem 2 minuta. Mund te regjistroheni me email ose me llogarine tuaj te Google.", ContentType = "step", SortOrder = 11, Metadata = "{\"stepNumber\":\"1\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_2", Language = "sq", Title = "Hapi 2: Raportoni Defektin", Content = "Plotesoni formularin e raportimit duke pershkruar problemin, duke zgjidhur kategorine (hidraulike, elektrike, HVAC, etj.), dhe duke ngarkuar foto te problemit. Sa me shume detaje te jepni, aq me mire profesionistet mund t'ju ndihmojne.", ContentType = "step", SortOrder = 12, Metadata = "{\"stepNumber\":\"2\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_3", Language = "sq", Title = "Hapi 3: Merrni Oferta", Content = "Profesionistet e zones suaj shohin kerkesen tuaj dhe dergojne oferta me cmimet e tyre dhe kohën e mundshme te nderhyrjes. Zakonisht merrni pergjigje brenda 15 minutash.", ContentType = "step", SortOrder = 13, Metadata = "{\"stepNumber\":\"3\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_4", Language = "sq", Title = "Hapi 4: Zgjidhni Profesionistin", Content = "Krahasoni ofertat, shikoni profilet e profesionisteve, lexoni vleresimet e klienteve te tjere dhe zgjidhni teknikun qe ju pershtatet me mire. Kontaktoni drejtperdrejt permes platformes.", ContentType = "step", SortOrder = 14, Metadata = "{\"stepNumber\":\"4\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_5", Language = "sq", Title = "Hapi 5: Vleresoni Sherbimin", Content = "Pas perfundimit te punes, vleresoni profesionistin. Vleresimi juaj ndihmon qytetaret e tjere te gjejne tekniket me te mire dhe motivon profesionistet te japin gjithmone sherbim cilesor.", ContentType = "step", SortOrder = 15, Metadata = "{\"stepNumber\":\"5\"}" },

            // For Professionals
            new() { PageKey = "howitworks", SectionKey = "professional_title", Language = "sq", Content = "Per Profesionistet", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "howitworks", SectionKey = "pro_step_1", Language = "sq", Title = "Hapi 1: Regjistrohuni si Profesionist", Content = "Krijoni profilin tuaj profesional duke shtuar specialitetin, zonen gjeografike te mbulimit, certifikatat dhe pervojen tuaj. Shtoni foto te puneve te meparshme per te terhequr me shume kliente.", ContentType = "step", SortOrder = 21, Metadata = "{\"stepNumber\":\"1\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_2", Language = "sq", Title = "Hapi 2: Merrni Njoftime per Punë", Content = "Kur nje qytetar raporton nje defekt ne zonen tuaj dhe kategorine tuaj te specialitetit, merrni njoftim menjehere. Shikoni detajet e punes dhe vendosni nese doni te jepni oferte.", ContentType = "step", SortOrder = 22, Metadata = "{\"stepNumber\":\"2\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_3", Language = "sq", Title = "Hapi 3: Dergoni Oferten", Content = "Dergoni oferten tuaj me cmimin e parashikuar, kohen e nderhyrjes dhe nje pershkrim te shkurter te qasjes suaj. Cmimi juaj konkurrues dhe profili i forte do t'ju ndihmojne te fitoni punen.", ContentType = "step", SortOrder = 23, Metadata = "{\"stepNumber\":\"3\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_4", Language = "sq", Title = "Hapi 4: Kryeni Punen", Content = "Nese klienti ju zgjedh, komunikoni per detajet, shkoni ne vendndodhjen e klientit dhe kryeni punen me profesionalizem. Sigurohuni qe klienti eshte i kenaqur.", ContentType = "step", SortOrder = 24, Metadata = "{\"stepNumber\":\"4\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_5", Language = "sq", Title = "Hapi 5: Ndertoni Reputacionin", Content = "Cdo pune e perfunduar me sukses forcon profilin tuaj. Vleresimet pozitive rrisin renditjen tuaj ne platforme dhe terhiqin me shume kliente.", ContentType = "step", SortOrder = 25, Metadata = "{\"stepNumber\":\"5\"}" },

            // Tips Section
            new() { PageKey = "howitworks", SectionKey = "tips_title", Language = "sq", Content = "Keshilla per Perdorimin me te Mire", ContentType = "text", SortOrder = 30 },
            new() { PageKey = "howitworks", SectionKey = "tip_1", Language = "sq", Title = "Pershkruani ne detaje", Content = "Sa me shume informacion te jepni per problemin, aq me te sakta do te jene ofertat e profesionisteve. Perfshini foto dhe dimensionet nese eshte e mundur.", IconClass = "bi-info-circle", ContentType = "feature", SortOrder = 31 },
            new() { PageKey = "howitworks", SectionKey = "tip_2", Language = "sq", Title = "Krahasoni disa oferta", Content = "Mos pranoni oferten e pare qe merrni. Prisni disa minuta per te marre disa oferta dhe krahasoni cmimet, pervojen dhe vleresimet e profesionisteve.", IconClass = "bi-search", ContentType = "feature", SortOrder = 32 },
            new() { PageKey = "howitworks", SectionKey = "tip_3", Language = "sq", Title = "Lexoni vleresimet", Content = "Vleresimet e klienteve te tjere jane burimi me i mire i informacionit per cilesine e punes se nje profesionisti. Kushtojini vemendje.", IconClass = "bi-chat-square-text", ContentType = "feature", SortOrder = 33 },
            new() { PageKey = "howitworks", SectionKey = "tip_4", Language = "sq", Title = "Komunikoni qarte", Content = "Pas zgjedhjes se profesionistit, komunikoni qarte per kohen, vendin dhe detajet e tjera te punes per te shmangur keqkuptime.", IconClass = "bi-chat-dots", ContentType = "feature", SortOrder = 34 },
        };

        // ====================================================================
        // HOW IT WORKS PAGE - ENGLISH
        // ====================================================================
        private static List<SiteContent> HowItWorksPageEnglish() => new()
        {
            new() { PageKey = "howitworks", SectionKey = "hero_title", Language = "en", Content = "How TeknoSOS Works", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "howitworks", SectionKey = "hero_subtitle", Language = "en", Content = "Our simple process takes you from problem to solution in just a few steps.", ContentType = "text", SortOrder = 2 },

            new() { PageKey = "howitworks", SectionKey = "citizen_title", Language = "en", Content = "For Citizens", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_1", Language = "en", Title = "Step 1: Create Your Account", Content = "Register for free by filling out the registration form with your basic data. The process takes only 2 minutes. You can register with email or with your Google account.", ContentType = "step", SortOrder = 11, Metadata = "{\"stepNumber\":\"1\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_2", Language = "en", Title = "Step 2: Report the Defect", Content = "Fill out the report form describing the problem, selecting the category (plumbing, electrical, HVAC, etc.), and uploading photos. The more details you provide, the better professionals can help you.", ContentType = "step", SortOrder = 12, Metadata = "{\"stepNumber\":\"2\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_3", Language = "en", Title = "Step 3: Receive Offers", Content = "Professionals in your area see your request and send offers with their prices and possible intervention times. You usually receive responses within 15 minutes.", ContentType = "step", SortOrder = 13, Metadata = "{\"stepNumber\":\"3\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_4", Language = "en", Title = "Step 4: Choose a Professional", Content = "Compare offers, view professional profiles, read other clients' reviews, and choose the technician that best fits your needs. Contact directly through the platform.", ContentType = "step", SortOrder = 14, Metadata = "{\"stepNumber\":\"4\"}" },
            new() { PageKey = "howitworks", SectionKey = "citizen_step_5", Language = "en", Title = "Step 5: Rate the Service", Content = "After the job is completed, rate the professional. Your review helps other citizens find the best technicians and motivates professionals to always provide quality service.", ContentType = "step", SortOrder = 15, Metadata = "{\"stepNumber\":\"5\"}" },

            new() { PageKey = "howitworks", SectionKey = "professional_title", Language = "en", Content = "For Professionals", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "howitworks", SectionKey = "pro_step_1", Language = "en", Title = "Step 1: Register as a Professional", Content = "Create your professional profile by adding your specialty, geographic coverage area, certificates, and experience. Add photos of previous work to attract more clients.", ContentType = "step", SortOrder = 21, Metadata = "{\"stepNumber\":\"1\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_2", Language = "en", Title = "Step 2: Get Job Notifications", Content = "When a citizen reports a defect in your area and your specialty category, you receive an immediate notification. View the job details and decide if you want to bid.", ContentType = "step", SortOrder = 22, Metadata = "{\"stepNumber\":\"2\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_3", Language = "en", Title = "Step 3: Send Your Offer", Content = "Send your offer with the estimated price, intervention time, and a brief description of your approach. Your competitive pricing and strong profile will help you win the job.", ContentType = "step", SortOrder = 23, Metadata = "{\"stepNumber\":\"3\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_4", Language = "en", Title = "Step 4: Complete the Job", Content = "If the client chooses you, communicate about the details, go to the client location, and complete the job professionally. Make sure the client is satisfied.", ContentType = "step", SortOrder = 24, Metadata = "{\"stepNumber\":\"4\"}" },
            new() { PageKey = "howitworks", SectionKey = "pro_step_5", Language = "en", Title = "Step 5: Build Your Reputation", Content = "Every successfully completed job strengthens your profile. Positive reviews increase your ranking on the platform and attract more clients.", ContentType = "step", SortOrder = 25, Metadata = "{\"stepNumber\":\"5\"}" },

            new() { PageKey = "howitworks", SectionKey = "tips_title", Language = "en", Content = "Tips for Best Use", ContentType = "text", SortOrder = 30 },
            new() { PageKey = "howitworks", SectionKey = "tip_1", Language = "en", Title = "Describe in detail", Content = "The more information you provide about the problem, the more accurate the professionals' offers will be. Include photos and dimensions if possible.", IconClass = "bi-info-circle", ContentType = "feature", SortOrder = 31 },
            new() { PageKey = "howitworks", SectionKey = "tip_2", Language = "en", Title = "Compare several offers", Content = "Do not accept the first offer you receive. Wait a few minutes to get several offers and compare prices, experience, and professional ratings.", IconClass = "bi-search", ContentType = "feature", SortOrder = 32 },
            new() { PageKey = "howitworks", SectionKey = "tip_3", Language = "en", Title = "Read reviews", Content = "Other clients' reviews are the best source of information about the quality of a professional's work. Pay attention to them.", IconClass = "bi-chat-square-text", ContentType = "feature", SortOrder = 33 },
            new() { PageKey = "howitworks", SectionKey = "tip_4", Language = "en", Title = "Communicate clearly", Content = "After choosing the professional, communicate clearly about timing, location, and other job details to avoid misunderstandings.", IconClass = "bi-chat-dots", ContentType = "feature", SortOrder = 34 },
        };

        // ====================================================================
        // FAQ PAGE - ALBANIAN
        // ====================================================================
        private static List<SiteContent> FaqPageAlbanian() => new()
        {
            new() { PageKey = "faq", SectionKey = "hero_title", Language = "sq", Content = "Pyetjet e Bera Shpesh", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "faq", SectionKey = "hero_subtitle", Language = "sq", Content = "Gjeni pergjigjet per pyetjet me te shpeshta rreth platformes TeknoSOS.", ContentType = "text", SortOrder = 2 },

            new() { PageKey = "faq", SectionKey = "faq_1", Language = "sq", Title = "Qka eshte TeknoSOS?", Content = "TeknoSOS eshte nje platforme online qe lidh qytetaret qe kane nevoje per sherbime teknike me profesioniste te verifikuar ne zonen e tyre. Platforma mbulon sherbime si hidraulike, elektrike, HVAC, zdrukthtari, pajisje shtëpiake dhe sherbime te pergjithshme.", ContentType = "faq", SortOrder = 10 },
            new() { PageKey = "faq", SectionKey = "faq_2", Language = "sq", Title = "Si mund te regjistroj nje defekt?", Content = "Pas regjistrimit si qytetar, klikoni butonin 'Raporto Defekt' ne panelin tuaj. Plotesoni formularin duke pershkruar problemin, zgjidhni kategorine perkatese, vendndodhjen dhe ngarkoni foto nese keni. Kerkesa juaj do te jete e dukshme per profesionistet e zones.", ContentType = "faq", SortOrder = 11 },
            new() { PageKey = "faq", SectionKey = "faq_3", Language = "sq", Title = "Sa kushton perdorimi i platformes?", Content = "Regjistrimi dhe raportimi i defekteve eshte plotesisht falas per qytetaret. Profesionistet mund te perdorin nje plan falas me funksionalitet baze, ose te zgjedhin nje plan premium per mundesi te avancuara. Nuk ka kosto te fshehura.", ContentType = "faq", SortOrder = 12 },
            new() { PageKey = "faq", SectionKey = "faq_4", Language = "sq", Title = "Si verifikoni profesionistet?", Content = "Cdo profesionist qe regjistrohet ne TeknoSOS kalon nje proces verifikimi qe perfshini kontrollin e dokumenteve te identitetit, certifikatave profesionale, referencave te punes se meparshme dhe vleresimeve nga klientet. Vetem profesionistet qe plotesojne standardet pranohen.", ContentType = "faq", SortOrder = 13 },
            new() { PageKey = "faq", SectionKey = "faq_5", Language = "sq", Title = "Sa shpejt merr pergjigje pas raportimit?", Content = "Mesatarisht, profesionistet pergjigjen brenda 15 minutash pas publikimit te kerkeses suaj. Koha mund te ndryshoje ne varesi te zones gjeografike dhe ores se dites. Per emergjenca, rekomanndojme te shenoni kerkesen si urgjente.", ContentType = "faq", SortOrder = 14 },
            new() { PageKey = "faq", SectionKey = "faq_6", Language = "sq", Title = "A mund te anuloj nje kerkesë?", Content = "Po, mund te anuloni nje kerkese ne cdo moment para se ajo te pranohej nga nje profesionist. Nese nje profesionist eshte zgjedhur tashme, duhet te kontaktoni drejtperdrejt per te koordinuar anulimin.", ContentType = "faq", SortOrder = 15 },
            new() { PageKey = "faq", SectionKey = "faq_7", Language = "sq", Title = "Si funksionon sistemi i vleresimit?", Content = "Pas perfundimit te nje pune, qytetari mund te vleresoje profesionistin me 1 deri ne 5 yje dhe te lere nje koment. Vleresimet jane te dukshme pubilkisht dhe ndihmojne perdoruesit e tjere te marrin vendime te informuara.", ContentType = "faq", SortOrder = 16 },
            new() { PageKey = "faq", SectionKey = "faq_8", Language = "sq", Title = "Cfare ndodh nese nuk jam i kenaqur me shërbimin?", Content = "Nese nuk jeni te kenaqur me punen e kryer, mund te lini nje vleresim negativ dhe te kontaktoni ekipin e suportit tone. Ne shqyrtojme cdo ankese dhe marrim masa ndaj profesionisteve qe nuk permbushin standardet.", ContentType = "faq", SortOrder = 17 },
            new() { PageKey = "faq", SectionKey = "faq_9", Language = "sq", Title = "A mund te perdorë platformen ne celular?", Content = "Po, TeknoSOS eshte plotesisht responsive dhe funksionon ne menyre te shkëlqyer ne te gjitha pajisjet mobile. Mund ta perdorni nepermjet shfletuesit te celularit ose tabletit pa asnje limitim.", ContentType = "faq", SortOrder = 18 },
            new() { PageKey = "faq", SectionKey = "faq_10", Language = "sq", Title = "Si mund te behem profesionist ne platforme?", Content = "Regjistrohuni duke zgjedhur rolin 'Profesionist', plotesoni profilin me specialitetin tuaj, zonen e mbulimit, certifikatat dhe pervojen. Pas verifikimit nga ekipi yne, llogaria juaj aktivizohet dhe filloni te merreni kerkesa.", ContentType = "faq", SortOrder = 19 },
            new() { PageKey = "faq", SectionKey = "faq_11", Language = "sq", Title = "A jane te sigurta te dhenat e mia?", Content = "Po, ne marrim seriozisht mbrojtjen e te dhenave. Perdorim enkriptim SSL per te gjitha komunikimet, ruajme te dhenat ne servera te sigurte dhe respektojme plotesisht rregulloren per mbrojtjen e te dhenave personale sipas ligjit shqiptar.", ContentType = "faq", SortOrder = 20 },
            new() { PageKey = "faq", SectionKey = "faq_12", Language = "sq", Title = "Si mund te kontaktoj suportin?", Content = "Mund te na kontaktoni permes faqes se kontaktit ne platforme, me email ne info@teknosos.app, ose me telefon. Ekipi yne i suportit eshte i disponueshem nga e hena deri te premten, 08:00-18:00.", ContentType = "faq", SortOrder = 21 },
        };

        // ====================================================================
        // FAQ PAGE - ENGLISH
        // ====================================================================
        private static List<SiteContent> FaqPageEnglish() => new()
        {
            new() { PageKey = "faq", SectionKey = "hero_title", Language = "en", Content = "Frequently Asked Questions", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "faq", SectionKey = "hero_subtitle", Language = "en", Content = "Find answers to the most common questions about the TeknoSOS platform.", ContentType = "text", SortOrder = 2 },

            new() { PageKey = "faq", SectionKey = "faq_1", Language = "en", Title = "What is TeknoSOS?", Content = "TeknoSOS is an online platform that connects citizens who need technical services with verified professionals in their area. The platform covers services such as plumbing, electrical, HVAC, carpentry, household appliances, and general services.", ContentType = "faq", SortOrder = 10 },
            new() { PageKey = "faq", SectionKey = "faq_2", Language = "en", Title = "How can I report a defect?", Content = "After registering as a citizen, click the 'Report Defect' button on your dashboard. Fill out the form describing the problem, select the appropriate category, location, and upload photos if available. Your request will be visible to professionals in the area.", ContentType = "faq", SortOrder = 11 },
            new() { PageKey = "faq", SectionKey = "faq_3", Language = "en", Title = "How much does platform use cost?", Content = "Registration and defect reporting is completely free for citizens. Professionals can use a free plan with basic functionality, or choose a premium plan for advanced features. No hidden costs.", ContentType = "faq", SortOrder = 12 },
            new() { PageKey = "faq", SectionKey = "faq_4", Language = "en", Title = "How do you verify professionals?", Content = "Every professional who registers on TeknoSOS goes through a verification process that includes checking identity documents, professional certificates, work references, and client reviews. Only professionals who meet standards are accepted.", ContentType = "faq", SortOrder = 13 },
            new() { PageKey = "faq", SectionKey = "faq_5", Language = "en", Title = "How quickly do I get a response?", Content = "On average, professionals respond within 15 minutes of your request being published. Time may vary depending on geographic area and time of day. For emergencies, we recommend marking the request as urgent.", ContentType = "faq", SortOrder = 14 },
            new() { PageKey = "faq", SectionKey = "faq_6", Language = "en", Title = "Can I cancel a request?", Content = "Yes, you can cancel a request at any time before it is accepted by a professional. If a professional has already been selected, you must contact directly to coordinate the cancellation.", ContentType = "faq", SortOrder = 15 },
            new() { PageKey = "faq", SectionKey = "faq_7", Language = "en", Title = "How does the rating system work?", Content = "After completing a job, the citizen can rate the professional with 1 to 5 stars and leave a comment. Reviews are publicly visible and help other users make informed decisions.", ContentType = "faq", SortOrder = 16 },
            new() { PageKey = "faq", SectionKey = "faq_8", Language = "en", Title = "What if I am not satisfied with the service?", Content = "If you are not satisfied with the work done, you can leave a negative review and contact our support team. We review every complaint and take action against professionals who do not meet standards.", ContentType = "faq", SortOrder = 17 },
            new() { PageKey = "faq", SectionKey = "faq_9", Language = "en", Title = "Can I use the platform on mobile?", Content = "Yes, TeknoSOS is fully responsive and works excellently on all mobile devices. You can use it through your phone or tablet browser without any limitations.", ContentType = "faq", SortOrder = 18 },
            new() { PageKey = "faq", SectionKey = "faq_10", Language = "en", Title = "How can I become a professional on the platform?", Content = "Register by choosing the 'Professional' role, complete your profile with your specialty, coverage area, certificates, and experience. After verification by our team, your account is activated and you start receiving requests.", ContentType = "faq", SortOrder = 19 },
            new() { PageKey = "faq", SectionKey = "faq_11", Language = "en", Title = "Is my data safe?", Content = "Yes, we take data protection seriously. We use SSL encryption for all communications, store data on secure servers, and fully comply with personal data protection regulations under Albanian law.", ContentType = "faq", SortOrder = 20 },
            new() { PageKey = "faq", SectionKey = "faq_12", Language = "en", Title = "How can I contact support?", Content = "You can contact us through the contact page on the platform, by email at info@teknosos.app, or by phone. Our support team is available Monday to Friday, 08:00-18:00.", ContentType = "faq", SortOrder = 21 },
        };

        // ====================================================================
        // CONTACT PAGE
        // ====================================================================
        private static List<SiteContent> ContactPageAlbanian() => new()
        {
            new() { PageKey = "contact", SectionKey = "hero_title", Language = "sq", Content = "Na Kontaktoni", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "contact", SectionKey = "hero_subtitle", Language = "sq", Content = "Keni pyetje, sugjerime ose keni nevoje per ndihme? Ne jemi ketu per ju.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "contact", SectionKey = "form_title", Language = "sq", Content = "Dergoni nje Mesazh", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "contact", SectionKey = "form_name", Language = "sq", Content = "Emri i plote", ContentType = "text", SortOrder = 11 },
            new() { PageKey = "contact", SectionKey = "form_email", Language = "sq", Content = "Adresa email", ContentType = "text", SortOrder = 12 },
            new() { PageKey = "contact", SectionKey = "form_subject", Language = "sq", Content = "Subjekti", ContentType = "text", SortOrder = 13 },
            new() { PageKey = "contact", SectionKey = "form_message", Language = "sq", Content = "Mesazhi juaj", ContentType = "text", SortOrder = 14 },
            new() { PageKey = "contact", SectionKey = "form_button", Language = "sq", Content = "Dergo Mesazhin", ContentType = "text", SortOrder = 15 },
            new() { PageKey = "contact", SectionKey = "info_title", Language = "sq", Content = "Informacione Kontakti", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "contact", SectionKey = "info_address", Language = "sq", Title = "Adresa", Content = "Tiranë, Shqipëri", IconClass = "bi-geo-alt", ContentType = "feature", SortOrder = 21 },
            new() { PageKey = "contact", SectionKey = "info_email", Language = "sq", Title = "Email", Content = "info@teknosos.app", IconClass = "bi-envelope", ContentType = "feature", SortOrder = 22 },
            new() { PageKey = "contact", SectionKey = "info_phone", Language = "sq", Title = "Telefon", Content = "+355 69 000 0000", IconClass = "bi-telephone", ContentType = "feature", SortOrder = 23 },
            new() { PageKey = "contact", SectionKey = "info_hours", Language = "sq", Title = "Oret e Punes", Content = "E hene - E premte: 08:00 - 18:00", IconClass = "bi-clock", ContentType = "feature", SortOrder = 24 },
        };

        private static List<SiteContent> ContactPageEnglish() => new()
        {
            new() { PageKey = "contact", SectionKey = "hero_title", Language = "en", Content = "Contact Us", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "contact", SectionKey = "hero_subtitle", Language = "en", Content = "Have questions, suggestions, or need help? We are here for you.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "contact", SectionKey = "form_title", Language = "en", Content = "Send a Message", ContentType = "text", SortOrder = 10 },
            new() { PageKey = "contact", SectionKey = "form_name", Language = "en", Content = "Full name", ContentType = "text", SortOrder = 11 },
            new() { PageKey = "contact", SectionKey = "form_email", Language = "en", Content = "Email address", ContentType = "text", SortOrder = 12 },
            new() { PageKey = "contact", SectionKey = "form_subject", Language = "en", Content = "Subject", ContentType = "text", SortOrder = 13 },
            new() { PageKey = "contact", SectionKey = "form_message", Language = "en", Content = "Your message", ContentType = "text", SortOrder = 14 },
            new() { PageKey = "contact", SectionKey = "form_button", Language = "en", Content = "Send Message", ContentType = "text", SortOrder = 15 },
            new() { PageKey = "contact", SectionKey = "info_title", Language = "en", Content = "Contact Information", ContentType = "text", SortOrder = 20 },
            new() { PageKey = "contact", SectionKey = "info_address", Language = "en", Title = "Address", Content = "Tirana, Albania", IconClass = "bi-geo-alt", ContentType = "feature", SortOrder = 21 },
            new() { PageKey = "contact", SectionKey = "info_email", Language = "en", Title = "Email", Content = "info@teknosos.app", IconClass = "bi-envelope", ContentType = "feature", SortOrder = 22 },
            new() { PageKey = "contact", SectionKey = "info_phone", Language = "en", Title = "Phone", Content = "+355 69 000 0000", IconClass = "bi-telephone", ContentType = "feature", SortOrder = 23 },
            new() { PageKey = "contact", SectionKey = "info_hours", Language = "en", Title = "Working Hours", Content = "Monday - Friday: 08:00 - 18:00", IconClass = "bi-clock", ContentType = "feature", SortOrder = 24 },
        };

        // ====================================================================
        // TERMS PAGE
        // ====================================================================
        private static List<SiteContent> TermsPageAlbanian() => new()
        {
            new() { PageKey = "terms", SectionKey = "hero_title", Language = "sq", Content = "Kushtet e Perdorimit", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "terms", SectionKey = "hero_subtitle", Language = "sq", Content = "Kushtet qe rregullojne perdorimin e platformes TeknoSOS. Perditesuar se fundmi me 01.01.2024.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "terms", SectionKey = "section_1", Language = "sq", Title = "1. Pranimi i Kushteve", Content = "Duke u regjistruar dhe perdorur platformen TeknoSOS, ju pranoni keto kushte perdorimi ne teresi. Nese nuk jeni dakord me ndonje kusht, ju lutem mos perdorni platformen. TeknoSOS rezervon te drejten te ndryshoje keto kushte ne cdo kohe duke njoftuar perdoruesit e regjistruar.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "terms", SectionKey = "section_2", Language = "sq", Title = "2. Pershkrimi i Sherbimit", Content = "TeknoSOS eshte nje platforme ndermjetesuese qe lidh qytetaret me profesioniste te sherbimeve teknike. Ne nuk jemi pale ne kontraten e sherbimit ndermjet qytetarit dhe profesionistit. Roli yne eshte te facilitojme lidhjen dhe te ofrojme mjete per menaxhimin e procesit.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "terms", SectionKey = "section_3", Language = "sq", Title = "3. Regjistrimi dhe Llogaria", Content = "Per te perdorur platformen, duhet te krijoni nje llogari me informacione te sakta dhe te plota. Ju jeni pergjegjes per ruajtjen e konfidencialitetit te te dhenave tuaja te hyrjes. Cdo aktivitet qe ndodh nen llogarine tuaj eshte pergjegjesia juaj. Duhet te na njoftoni menjehere per cdo perdorim te paautorizuar.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "terms", SectionKey = "section_4", Language = "sq", Title = "4. Detyrimet e Perdoruesve", Content = "Perdoruesit angazhohen te: ofrojne informacione te sakta e te plota, te mos keqperdorin platformen, te respektojne te drejtat e perdoruesve te tjere, te mos publikojne permbajtje te papershtatshme ose te paligjshme, te respektojne te drejtat e pronesise intelektuale.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "terms", SectionKey = "section_5", Language = "sq", Title = "5. Cmimi dhe Pagesa", Content = "Cmimet per sherbimet teknike vendosen drejtperdrejt ndermjet qytetarit dhe profesionistit. TeknoSOS nuk eshte pergjegjes per mosmarreveshje ne lidhje me cmimet. Platforma mund te aplikoje komisione per sherbime premium, te cilat do te komunikohen qarte para blerjes.", ContentType = "html", SortOrder = 14 },
            new() { PageKey = "terms", SectionKey = "section_6", Language = "sq", Title = "6. Vleresimet dhe Komentet", Content = "Perdoruesit mund te lene vleresime pas perfundimit te nje sherbimi. Vleresimet duhet te jene te sinqerta, te sakta dhe te bazuara ne pervoje reale. TeknoSOS rezervon te drejten te heqe vleresime qe perbejne fyerje, te dhena te pavereta ose qe shkelin keto kushte.", ContentType = "html", SortOrder = 15 },
            new() { PageKey = "terms", SectionKey = "section_7", Language = "sq", Title = "7. Perjashtimi i Pergjegjësise", Content = "TeknoSOS nuk garanton cilesine e sherbimeve te ofruara nga profesionistet. Ne nuk jemi pergjegjes per demtime direkte ose indirekte qe rezultojne nga perdorimi i platformes ose nga sherbimet e marra permes platformes. Cdo marredhenie pune eshte drejtperdrejt ndermjet qytetarit dhe profesionistit.", ContentType = "html", SortOrder = 16 },
            new() { PageKey = "terms", SectionKey = "section_8", Language = "sq", Title = "8. Pronesia Intelektuale", Content = "Te gjitha te drejtat e pronesise intelektuale mbi platformen, duke perfshire por pa u limituar ne logo, dizajn, kod dhe permbajtje, i perkasin TeknoSOS. Ndalohet riprodhimi, shperndarja ose perdorimi komercial pa miratimin tone me shkrim.", ContentType = "html", SortOrder = 17 },
            new() { PageKey = "terms", SectionKey = "section_9", Language = "sq", Title = "9. Mbyllja e Llogarise", Content = "TeknoSOS rezervon te drejten te pezulloje ose te mbyllë llogarine tuaj nese: shkelni keto kushte, ofroni informacione te pavereta, keqperdorni platformen, ose nese marrim ankesa te perseritira nga perdorues te tjere.", ContentType = "html", SortOrder = 18 },
            new() { PageKey = "terms", SectionKey = "section_10", Language = "sq", Title = "10. Ligji i Zbatueshëm", Content = "Keto kushte rregullohen nga ligji i Republikes se Shqiperise. Per cdo mosmarreveshje qe nuk zgjidhet me mirekuptim, do te jene kompetente gjykatat e Tiranes.", ContentType = "html", SortOrder = 19 },
        };

        private static List<SiteContent> TermsPageEnglish() => new()
        {
            new() { PageKey = "terms", SectionKey = "hero_title", Language = "en", Content = "Terms of Use", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "terms", SectionKey = "hero_subtitle", Language = "en", Content = "Terms governing the use of the TeknoSOS platform. Last updated on 01.01.2024.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "terms", SectionKey = "section_1", Language = "en", Title = "1. Acceptance of Terms", Content = "By registering and using the TeknoSOS platform, you accept these terms of use in their entirety. If you do not agree with any term, please do not use the platform. TeknoSOS reserves the right to change these terms at any time by notifying registered users.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "terms", SectionKey = "section_2", Language = "en", Title = "2. Service Description", Content = "TeknoSOS is an intermediary platform connecting citizens with technical service professionals. We are not a party to the service contract between the citizen and the professional. Our role is to facilitate the connection and provide tools for process management.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "terms", SectionKey = "section_3", Language = "en", Title = "3. Registration and Account", Content = "To use the platform, you must create an account with accurate and complete information. You are responsible for maintaining the confidentiality of your login data. All activity under your account is your responsibility. You must notify us immediately of any unauthorized use.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "terms", SectionKey = "section_4", Language = "en", Title = "4. User Obligations", Content = "Users commit to: providing accurate and complete information, not misusing the platform, respecting other users' rights, not publishing inappropriate or illegal content, and respecting intellectual property rights.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "terms", SectionKey = "section_5", Language = "en", Title = "5. Pricing and Payment", Content = "Prices for technical services are set directly between the citizen and the professional. TeknoSOS is not responsible for pricing disputes. The platform may apply commissions for premium services, which will be clearly communicated before purchase.", ContentType = "html", SortOrder = 14 },
            new() { PageKey = "terms", SectionKey = "section_6", Language = "en", Title = "6. Reviews and Comments", Content = "Users may leave reviews after completing a service. Reviews must be honest, accurate, and based on real experience. TeknoSOS reserves the right to remove reviews that constitute insults, false information, or violate these terms.", ContentType = "html", SortOrder = 15 },
            new() { PageKey = "terms", SectionKey = "section_7", Language = "en", Title = "7. Disclaimer of Liability", Content = "TeknoSOS does not guarantee the quality of services offered by professionals. We are not responsible for direct or indirect damages resulting from platform use or services received through the platform. All work relationships are directly between the citizen and the professional.", ContentType = "html", SortOrder = 16 },
            new() { PageKey = "terms", SectionKey = "section_8", Language = "en", Title = "8. Intellectual Property", Content = "All intellectual property rights on the platform, including but not limited to logo, design, code, and content, belong to TeknoSOS. Reproduction, distribution, or commercial use without our written approval is prohibited.", ContentType = "html", SortOrder = 17 },
            new() { PageKey = "terms", SectionKey = "section_9", Language = "en", Title = "9. Account Closure", Content = "TeknoSOS reserves the right to suspend or close your account if you: violate these terms, provide false information, misuse the platform, or if we receive repeated complaints from other users.", ContentType = "html", SortOrder = 18 },
            new() { PageKey = "terms", SectionKey = "section_10", Language = "en", Title = "10. Applicable Law", Content = "These terms are governed by the laws of the Republic of Albania. For any dispute not resolved amicably, the courts of Tirana will have jurisdiction.", ContentType = "html", SortOrder = 19 },
        };

        // ====================================================================
        // PRIVACY POLICY PAGE
        // ====================================================================
        private static List<SiteContent> PrivacyPageAlbanian() => new()
        {
            new() { PageKey = "privacy", SectionKey = "hero_title", Language = "sq", Content = "Politika e Privatesise", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "privacy", SectionKey = "hero_subtitle", Language = "sq", Content = "Si i mbledhim, perdorim dhe mbrojme te dhenat tuaja personale. Perditesuar me 01.01.2024.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "privacy", SectionKey = "section_1", Language = "sq", Title = "1. Te Dhenat qe Mbledhim", Content = "Ne mbledhim te dhenat e meposhtme: informacione identifikimi (emer, mbiemer, email, telefon), te dhena lokacioni (adrese, qytet, koordinatat gjeografike per gjetje te profesionisteve ne afersi), informacione profesionale (per profesionistet: certifikata, specialitete, pervoja), te dhena teknike (adresa IP, lloji i shfletuesit, data dhe ora e qasjes).", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "privacy", SectionKey = "section_2", Language = "sq", Title = "2. Qellimi i Perpunimit", Content = "Te dhenat tuaja perdoren per: ofrimin e sherbimit te platformes, lidhjen e qytetareve me profesioniste, permiresimin e pervojes se perdoruesit, dergimin e njoftimeve per statusin e kerkesave, analiza statistikore te anonimizuara per permiresimin e sherbimit.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "privacy", SectionKey = "section_3", Language = "sq", Title = "3. Baza Ligjore", Content = "Perpunimi i te dhenave behet ne baze te: pelqimit tuaj (neni 6.1.a i Ligjit nr. 9887), ekzekutimit te kontrates se sherbimit (neni 6.1.b), detyrimit ligjor (neni 6.1.c), interesit legjitim te platformes (neni 6.1.f).", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "privacy", SectionKey = "section_4", Language = "sq", Title = "4. Ndarja e te Dhenave", Content = "Te dhenat tuaja ndahen vetem me: profesioniste/qytetare ne kuadrin e nje kerkese aktive, ofruesit e sherbimeve teknike (hosting, email), autoritetet publike kur kerkohet me ligj. Nuk i shesim asnjehere te dhenat tuaja paleve te treta.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "privacy", SectionKey = "section_5", Language = "sq", Title = "5. Ruajtja e te Dhenave", Content = "Te dhenat tuaja ruhen per aq kohë sa eshte e nevojshme per ofrimin e sherbimit. Pas mbylljes se llogarise, te dhenat fshihen brenda 30 diteve, pervec rasteve kur ligji kerkon ruajtje me te gjate.", ContentType = "html", SortOrder = 14 },
            new() { PageKey = "privacy", SectionKey = "section_6", Language = "sq", Title = "6. Te Drejtat Tuaja", Content = "Ju keni te drejten: te qaseni ne te dhenat tuaja, te kerkoni korrigjimin e te dhenave jo te sakta, te kerkoni fshirjen e te dhenave (e drejta per t'u harruar), te kufizoni perpunimin, te kundershtoni perpunimin, te kerkoni transferimin e te dhenave (portabilitetin).", ContentType = "html", SortOrder = 15 },
            new() { PageKey = "privacy", SectionKey = "section_7", Language = "sq", Title = "7. Siguria e te Dhenave", Content = "Perdorim masa teknike dhe organizative per te mbrojtur te dhenat tuaja, duke perfshire: enkriptim SSL/TLS, firewall dhe sisteme zbulimi te nderhyrjeve, qasje e kufizuar ne baze te rolit, kopje rezerve te rregullta, monitorim i vazhdueshëm i sigurise.", ContentType = "html", SortOrder = 16 },
            new() { PageKey = "privacy", SectionKey = "section_8", Language = "sq", Title = "8. Cookie", Content = "Perdorim cookie per funksionalitetin e platformes, duke perfshire ruajtjen e preferncave te gjuhes. Per me shume informacion, shikoni Politiken e Cookie.", ContentType = "html", SortOrder = 17 },
            new() { PageKey = "privacy", SectionKey = "section_9", Language = "sq", Title = "9. Kontakti per Privatesi", Content = "Per pyetje rreth privatesise ose per te ushtruar te drejtat tuaja, na kontaktoni ne: info@teknosos.app. Pergjigjemi brenda 30 diteve kalendarike.", ContentType = "html", SortOrder = 18 },
        };

        private static List<SiteContent> PrivacyPageEnglish() => new()
        {
            new() { PageKey = "privacy", SectionKey = "hero_title", Language = "en", Content = "Privacy Policy", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "privacy", SectionKey = "hero_subtitle", Language = "en", Content = "How we collect, use, and protect your personal data. Updated on 01.01.2024.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "privacy", SectionKey = "section_1", Language = "en", Title = "1. Data We Collect", Content = "We collect the following data: identification information (name, surname, email, phone), location data (address, city, geographic coordinates for finding nearby professionals), professional information (for professionals: certificates, specialties, experience), technical data (IP address, browser type, access date and time).", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "privacy", SectionKey = "section_2", Language = "en", Title = "2. Purpose of Processing", Content = "Your data is used for: providing the platform service, connecting citizens with professionals, improving user experience, sending notifications about request status, anonymized statistical analyses for service improvement.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "privacy", SectionKey = "section_3", Language = "en", Title = "3. Legal Basis", Content = "Data processing is based on: your consent (Article 6.1.a of Law no. 9887), execution of the service contract (Article 6.1.b), legal obligation (Article 6.1.c), legitimate interest of the platform (Article 6.1.f).", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "privacy", SectionKey = "section_4", Language = "en", Title = "4. Data Sharing", Content = "Your data is shared only with: professionals/citizens within an active request, technical service providers (hosting, email), public authorities when required by law. We never sell your data to third parties.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "privacy", SectionKey = "section_5", Language = "en", Title = "5. Data Retention", Content = "Your data is kept for as long as necessary to provide the service. After account closure, data is deleted within 30 days, except where the law requires longer retention.", ContentType = "html", SortOrder = 14 },
            new() { PageKey = "privacy", SectionKey = "section_6", Language = "en", Title = "6. Your Rights", Content = "You have the right to: access your data, request correction of inaccurate data, request deletion of data (right to be forgotten), restrict processing, object to processing, request data transfer (portability).", ContentType = "html", SortOrder = 15 },
            new() { PageKey = "privacy", SectionKey = "section_7", Language = "en", Title = "7. Data Security", Content = "We use technical and organizational measures to protect your data, including: SSL/TLS encryption, firewall and intrusion detection systems, role-based limited access, regular backups, continuous security monitoring.", ContentType = "html", SortOrder = 16 },
            new() { PageKey = "privacy", SectionKey = "section_8", Language = "en", Title = "8. Cookies", Content = "We use cookies for platform functionality, including storing language preferences. For more information, see our Cookie Policy.", ContentType = "html", SortOrder = 17 },
            new() { PageKey = "privacy", SectionKey = "section_9", Language = "en", Title = "9. Privacy Contact", Content = "For privacy questions or to exercise your rights, contact us at: info@teknosos.app. We respond within 30 calendar days.", ContentType = "html", SortOrder = 18 },
        };

        // ====================================================================
        // COOKIE POLICY PAGE
        // ====================================================================
        private static List<SiteContent> CookiePolicyPageAlbanian() => new()
        {
            new() { PageKey = "cookie", SectionKey = "hero_title", Language = "sq", Content = "Politika e Cookie", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "cookie", SectionKey = "hero_subtitle", Language = "sq", Content = "Informacion per perdorimin e cookie ne platformen TeknoSOS.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "cookie", SectionKey = "section_1", Language = "sq", Title = "1. Cfare jane Cookie?", Content = "Cookie jane skedare te vegjel teksti qe ruhen ne pajisjen tuaj kur vizitoni nje faqe web. Ata na ndihmojne te ofrojme nje pervoje me te mire duke mbajtur mend preferencat tuaja.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "cookie", SectionKey = "section_2", Language = "sq", Title = "2. Cookie qe Perdorim", Content = "Cookie te nevojshme: per funksionimin e platformes (sesion, autentikim, token sigurie). Cookie preferencash: per ruajtjen e gjuhes se zgjedhur (teknosos_lang, vlefshmeri 1 vit). Cookie analitike: per te kuptuar perdorimin e platformes (anonimizuar).", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "cookie", SectionKey = "section_3", Language = "sq", Title = "3. Menaxhimi i Cookie", Content = "Mund te menaxhoni cookie permes cilesimeve te shfletuesit tuaj. Vini re qe bllokimi i cookie te nevojshme mund te ndikoje funksionalitetin e platformes.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "cookie", SectionKey = "section_4", Language = "sq", Title = "4. Cookie te Paleve te Treta", Content = "Platforma mund te perdore sherbime te paleve te treta qe vendosin cookie te tyre (p.sh. Google Analytics, sherbime CDN per stilet Bootstrap). Keto cookie rregullohen nga politikat e privatesise te ofruesve perkattes.", ContentType = "html", SortOrder = 13 },
        };

        private static List<SiteContent> CookiePolicyPageEnglish() => new()
        {
            new() { PageKey = "cookie", SectionKey = "hero_title", Language = "en", Content = "Cookie Policy", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "cookie", SectionKey = "hero_subtitle", Language = "en", Content = "Information about the use of cookies on the TeknoSOS platform.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "cookie", SectionKey = "section_1", Language = "en", Title = "1. What are Cookies?", Content = "Cookies are small text files stored on your device when you visit a website. They help us provide a better experience by remembering your preferences.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "cookie", SectionKey = "section_2", Language = "en", Title = "2. Cookies We Use", Content = "Necessary cookies: for platform functionality (session, authentication, security tokens). Preference cookies: for saving selected language (teknosos_lang, validity 1 year). Analytics cookies: for understanding platform usage (anonymized).", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "cookie", SectionKey = "section_3", Language = "en", Title = "3. Managing Cookies", Content = "You can manage cookies through your browser settings. Note that blocking necessary cookies may affect platform functionality.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "cookie", SectionKey = "section_4", Language = "en", Title = "4. Third-Party Cookies", Content = "The platform may use third-party services that set their own cookies (e.g., Google Analytics, Bootstrap CDN services). These cookies are governed by the privacy policies of the respective providers.", ContentType = "html", SortOrder = 13 },
        };

        // ====================================================================
        // DISCLAIMER PAGE
        // ====================================================================
        private static List<SiteContent> DisclaimerPageAlbanian() => new()
        {
            new() { PageKey = "disclaimer", SectionKey = "hero_title", Language = "sq", Content = "Mohim Pergjëgjësie", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "disclaimer", SectionKey = "hero_subtitle", Language = "sq", Content = "Informacion i rendesishem per kufizimet e pergjegjsise se platformes TeknoSOS.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "disclaimer", SectionKey = "section_1", Language = "sq", Title = "1. Natyra e Sherbimit", Content = "TeknoSOS eshte nje platforme ndermjetesuese qe lidh qytetaret me profesioniste te sherbimeve teknike. Ne nuk kryejme vete sherbime teknike dhe nuk jemi pale ne kontraten ndermjet qytetarit dhe profesionistit.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "disclaimer", SectionKey = "section_2", Language = "sq", Title = "2. Cilesia e Sherbimeve", Content = "Megjithese verifikojme profesionistet, nuk mund te garantojme cilesine e cdo sherbimi individual. Vleresimet e perdoruesve jane nje udherrefyes, por vendimi perfundimtar eshte pergjegjesia e qytetarit.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "disclaimer", SectionKey = "section_3", Language = "sq", Title = "3. Perjashtimi i Demeve", Content = "TeknoSOS nuk mban pergjegjesi per: deme te shkaktuara nga sherbimet teknike, humbje financiare nga marreveshjet ndermjet perdoruesve, vonesa ose mosperfundim te punes, deme ndaj pasurise se paluajtshme ose te luajtshme.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "disclaimer", SectionKey = "section_4", Language = "sq", Title = "4. Saktesia e Informacionit", Content = "Informacioni ne platforme ofrohet 'sic eshte'. Ne bejme perpjekje per saktesine e te dhenave, por nuk garantojme qe te gjitha informacionet jane te plota, te sakta ose te perditeuara ne cdo moment.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "disclaimer", SectionKey = "section_5", Language = "sq", Title = "5. Disponueshmerija", Content = "Ne perpiqemi te mbajme platformen operative 24/7, por nuk garantojme disponueshmeri te panderprerë. Platformia mund te jete e padisponueshme per shkak te mirembajtjes, perditesimeve, ose problemeve teknike.", ContentType = "html", SortOrder = 14 },
        };

        private static List<SiteContent> DisclaimerPageEnglish() => new()
        {
            new() { PageKey = "disclaimer", SectionKey = "hero_title", Language = "en", Content = "Disclaimer", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "disclaimer", SectionKey = "hero_subtitle", Language = "en", Content = "Important information about the limitations of liability of the TeknoSOS platform.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "disclaimer", SectionKey = "section_1", Language = "en", Title = "1. Nature of Service", Content = "TeknoSOS is an intermediary platform connecting citizens with technical service professionals. We do not perform technical services ourselves and are not a party to the contract between the citizen and the professional.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "disclaimer", SectionKey = "section_2", Language = "en", Title = "2. Service Quality", Content = "Although we verify professionals, we cannot guarantee the quality of every individual service. User reviews are a guide, but the final decision is the citizen's responsibility.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "disclaimer", SectionKey = "section_3", Language = "en", Title = "3. Exclusion of Damages", Content = "TeknoSOS is not liable for: damages caused by technical services, financial losses from agreements between users, delays or non-completion of work, damages to movable or immovable property.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "disclaimer", SectionKey = "section_4", Language = "en", Title = "4. Accuracy of Information", Content = "Information on the platform is provided 'as is'. We make efforts for data accuracy, but do not guarantee that all information is complete, accurate, or updated at all times.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "disclaimer", SectionKey = "section_5", Language = "en", Title = "5. Availability", Content = "We strive to keep the platform operational 24/7, but do not guarantee uninterrupted availability. The platform may be unavailable due to maintenance, updates, or technical problems.", ContentType = "html", SortOrder = 14 },
        };

        // ====================================================================
        // DATA PROCESSING PAGE
        // ====================================================================
        private static List<SiteContent> DataProcessingPageAlbanian() => new()
        {
            new() { PageKey = "dataprocessing", SectionKey = "hero_title", Language = "sq", Content = "Marreveshja per Perpunimin e te Dhenave", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "dataprocessing", SectionKey = "hero_subtitle", Language = "sq", Content = "Dokumenti i DPA (Data Processing Agreement) sipas kerkesave ligjore.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "dataprocessing", SectionKey = "section_1", Language = "sq", Title = "1. Objektivat e Perpunimit", Content = "Ky dokument percakton kushtet e perpunimit te te dhenave personale ne kuadrin e sherbimit te platformes TeknoSOS, ne perputhje me Ligjin nr. 9887 'Per Mbrojtjen e te Dhenave Personale' te Republikes se Shqiperise.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "dataprocessing", SectionKey = "section_2", Language = "sq", Title = "2. Kontrolluesi i te Dhenave", Content = "TeknoSOS vepron si kontrollues i te dhenave per informacionet e perdoruesve te platformes. Kontrolluesi percakton qellimet dhe mjetet e perpunimit te te dhenave personale.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "dataprocessing", SectionKey = "section_3", Language = "sq", Title = "3. Perpunuesit", Content = "TeknoSOS mund te angazhoje perpunues te te dhenave (p.sh. ofrues te sherbimeve cloud, emaili) qe perpunojne te dhena ne emer te kontroluuesit. Te gjithe perpunuesit jane te detyruar me kontrate per te respektuar standardet e sigurise.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "dataprocessing", SectionKey = "section_4", Language = "sq", Title = "4. Masat e Sigurise", Content = "Zbatohen masat e meposhtme teknike dhe organizative: enkriptim i te dhenave ne tranzit dhe ne ruajtje, kontrolle te qasjes ne baze te rolit, regjistrim i te gjitha veprimeve mbi te dhenat, testim i rregullt i sistemeve te sigurise, plane per rimekembjen nga fatkeqesite.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "dataprocessing", SectionKey = "section_5", Language = "sq", Title = "5. Transferimi Nderkombetar", Content = "Te dhenat personale perpunohen kryesisht brenda Shqiperise. Ne rast transferimi jashte vendit, sigurohemi qe vendi prites ofron nivel te pershtatshem mbrojtjeje ose zbatojme klauzola kontraktore standarde.", ContentType = "html", SortOrder = 14 },
        };

        private static List<SiteContent> DataProcessingPageEnglish() => new()
        {
            new() { PageKey = "dataprocessing", SectionKey = "hero_title", Language = "en", Content = "Data Processing Agreement", ContentType = "text", SortOrder = 1 },
            new() { PageKey = "dataprocessing", SectionKey = "hero_subtitle", Language = "en", Content = "DPA (Data Processing Agreement) document according to legal requirements.", ContentType = "text", SortOrder = 2 },
            new() { PageKey = "dataprocessing", SectionKey = "section_1", Language = "en", Title = "1. Processing Objectives", Content = "This document defines the terms of personal data processing within the TeknoSOS platform service, in compliance with Law no. 9887 'On Protection of Personal Data' of the Republic of Albania.", ContentType = "html", SortOrder = 10 },
            new() { PageKey = "dataprocessing", SectionKey = "section_2", Language = "en", Title = "2. Data Controller", Content = "TeknoSOS acts as the data controller for platform user information. The controller determines the purposes and means of personal data processing.", ContentType = "html", SortOrder = 11 },
            new() { PageKey = "dataprocessing", SectionKey = "section_3", Language = "en", Title = "3. Processors", Content = "TeknoSOS may engage data processors (e.g., cloud service providers, email) that process data on behalf of the controller. All processors are contractually obligated to respect security standards.", ContentType = "html", SortOrder = 12 },
            new() { PageKey = "dataprocessing", SectionKey = "section_4", Language = "en", Title = "4. Security Measures", Content = "The following technical and organizational measures are applied: encryption of data in transit and at rest, role-based access controls, logging of all data operations, regular testing of security systems, disaster recovery plans.", ContentType = "html", SortOrder = 13 },
            new() { PageKey = "dataprocessing", SectionKey = "section_5", Language = "en", Title = "5. International Transfer", Content = "Personal data is primarily processed within Albania. In case of transfer outside the country, we ensure the destination country provides an adequate level of protection or we apply standard contractual clauses.", ContentType = "html", SortOrder = 14 },
        };
    }
}
