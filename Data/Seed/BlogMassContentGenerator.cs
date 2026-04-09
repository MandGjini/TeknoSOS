using System.Text;
using System.Text.RegularExpressions;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Data.Seed;

public static class BlogMassContentGenerator
{
    public static List<BlogPost> GenerateSeoPosts()
    {
        var posts = new List<BlogPost>();
        posts.AddRange(GenerateForLanguage("sq", 70));
        posts.AddRange(GenerateForLanguage("en", 70));
        return posts;
    }

    private static IEnumerable<BlogPost> GenerateForLanguage(string language, int count)
    {
        var categoriesSq = new[]
        {
            "Energjia Elektrike", "Hidraulika", "Siguria në Shtëpi", "Mirëmbajtja",
            "Karikues EV", "Shtëpi Inteligjente", "Këshilla Ligjore", "Kursim Energie"
        };

        var categoriesEn = new[]
        {
            "Electrical Services", "Plumbing", "Home Safety", "Maintenance",
            "EV Charging", "Smart Home", "Legal Tips", "Energy Savings"
        };

        var now = DateTime.UtcNow;

        for (int i = 1; i <= count; i++)
        {
            var category = language == "sq"
                ? categoriesSq[(i - 1) % categoriesSq.Length]
                : categoriesEn[(i - 1) % categoriesEn.Length];

            var title = language == "sq"
                ? $"Udhëzues Praktik {i}: {category} në Shqipëri me Fokus SEO dhe Zgjidhje Reale"
                : $"Practical Guide {i}: {category} in Albania with SEO-Driven Real Solutions";

            var slug = language == "sq"
                ? $"blog-sq-seo-{i:000}"
                : $"blog-en-seo-{i:000}";

            var html = language == "sq"
                ? BuildAlbanianHtml(i, category)
                : BuildEnglishHtml(i, category);

            html = EnsureWordRange(html, language, 700, 1000);
            var plain = ToPlainText(html);

            yield return new BlogPost
            {
                Slug = slug,
                Title = title,
                MetaDescription = Truncate(plain, 158),
                Excerpt = Truncate(plain, 280),
                Category = category,
                CategoryIcon = "bi-journal-richtext",
                Author = "TeknoSOS Editorial",
                PublishedDate = now.AddDays(-i),
                ReadTime = "8-10 min",
                ImageIcon = "bi-lightning-charge",
                Tags = language == "sq"
                    ? "seo shqiperi,google adsense,shtepi,teknosos,sherbime teknike,albania"
                    : "albania seo,google adsense,home maintenance,teknosos,technical services",
                Content = html,
                IsActive = true,
                SortOrder = i,
                CreatedDate = now.AddDays(-i)
            };
        }
    }

    private static string BuildAlbanianHtml(int index, string category)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h2>{category}: Strategjia e Plotë për Përdoruesit në Shqipëri</h2>");
        sb.AppendLine($"<p>Ky artikull i serisë {index} është ndërtuar për familjet dhe bizneset në Shqipëri që duan të marrin vendime të sakta për {category.ToLower()}, të shmangin kostot e panevojshme dhe të kuptojnë si funksionon një proces profesional nga diagnostikimi deri te zgjidhja përfundimtare. Qëllimi ynë është të ofrojmë përmbajtje të gjatë, të qartë dhe të dobishme, që jo vetëm ndihmon lexuesin por edhe ndërton besim afatgjatë te marka. Nëse një qytetar kërkon ndihmë për një problem urgjent, informacioni i saktë në kohën e duhur ul stresin dhe rrit probabilitetin që shërbimi të kryhet pa gabime. Në këtë udhëzues do të gjesh hapa praktikë, kontroll lista dhe mënyra konkrete për të bashkëpunuar me profesionistë të verifikuar.</p>");

        for (var s = 1; s <= 6; s++)
        {
            sb.AppendLine($"<h3>Hapi {s}: Çfarë duhet të kontrollosh para se të marrësh vendim</h3>");
            sb.AppendLine("<p>Hapi i parë për çdo ndërhyrje të suksesshme është verifikimi i gjendjes reale. Shumë përdorues kërkojnë zgjidhje të menjëhershme pa dokumentuar simptomat, dhe kjo shpesh sjell kosto shtesë. Prandaj rekomandohet të mbash shënim kur ka nisur problemi, në cilat kushte përsëritet dhe çfarë veprimesh e përkeqësojnë situatën. Ky informacion i ndihmon teknikët të përgatiten me materialet e duhura dhe ta zgjidhin çështjen në vizitën e parë. Në tregun shqiptar, transparenca është thelbësore: kërko gjithmonë ofertë të qartë, afat realizimi dhe garanci të punës. Kur procesi ndiqet me disiplinë, rreziku i vonesave bie ndjeshëm dhe rezultati përfundimtar është më i qëndrueshëm për familjen ose biznesin.</p>");
        }

        sb.AppendLine("<h3>SEO Lokale dhe Google AdSense: si lidhen me përmbajtjen cilësore</h3>");
        sb.AppendLine("<p>Për të performuar mirë në Google në Shqipëri, artikujt duhet të jenë të detajuar, me strukturë logjike dhe gjuhë natyrale. Përmbajtja e cekët nuk mjafton më për të fituar pozicione të larta në kërkim. Kur një tekst jep përgjigje reale, përdor terma lokalë dhe mban standard editorial, kohëqëndrimi i përdoruesit rritet. Kjo ndikon pozitivisht edhe në potencialin e monetizimit me Google AdSense, sepse reklamat shfaqen më mirë në faqe me trafik organik të qëndrueshëm dhe audiencë të kualifikuar. Strategjia fituese është e thjeshtë: shkruaj për problemet reale të qytetarëve, përditëso artikujt periodikisht dhe lidh çdo udhëzues me një veprim të qartë që e ndihmon përdoruesin të kalojë nga informacioni te zgjidhja.</p>");

        sb.AppendLine("<h3>Përfundim praktik</h3>");
        sb.AppendLine("<p>Nëse dëshiron që shërbimi të kryhet me cilësi, kosto të kontrolluar dhe kohë të arsyeshme, fokusoju te përgatitja paraprake dhe komunikimi i qartë me profesionistin. Mos e shiko procesin si një thirrje të vetme, por si një bashkëpunim ku të dyja palët kanë informacion të mjaftueshëm për të marrë vendime të sakta. Ky model pune rrit kënaqësinë e klientit dhe ul nevojën për ndërhyrje të përsëritura. Në fund, përmbajtja e mirë e kombinuar me shërbim korrekt është formula më e sigurt për SEO të qëndrueshme, reputacion pozitiv dhe rritje të vazhdueshme në tregun shqiptar.</p>");

        return sb.ToString();
    }

    private static string BuildEnglishHtml(int index, string category)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<h2>{category}: A Complete Practical Strategy for Users in Albania</h2>");
        sb.AppendLine($"<p>This article in series {index} is designed for families and businesses in Albania who need reliable guidance on {category.ToLower()}, want to avoid unnecessary costs, and aim to understand a professional workflow from diagnosis to final resolution. The objective is to deliver long-form, actionable content that helps readers make confident decisions and builds long-term trust. When a client faces an urgent issue, clear information at the right time reduces stress and increases first-visit success. In this guide you will find practical steps, checklists, and collaboration rules that make technical service predictable. Content quality is not only useful for users; it is also a strong foundation for sustainable SEO performance and better monetization opportunities.</p>");

        for (var s = 1; s <= 6; s++)
        {
            sb.AppendLine($"<h3>Step {s}: What to validate before choosing a service path</h3>");
            sb.AppendLine("<p>Every successful intervention starts with precise context. Many users request immediate action without documenting symptoms, and this usually leads to delays or higher costs. A better approach is to record when the issue started, how frequently it appears, and which conditions make it worse. This short diagnostic note helps technicians prepare with proper tools and materials, increasing the chance of solving the issue on the first visit. In Albania, service transparency is a competitive advantage: request a clear estimate, a realistic timeline, and warranty terms before work begins. Structured communication prevents misunderstandings, protects both sides, and creates a workflow where quality can be measured. Over time, this discipline lowers repeat incidents and improves customer satisfaction across both residential and business projects.</p>");
        }

        sb.AppendLine("<h3>Local SEO and Google AdSense: why depth of content matters</h3>");
        sb.AppendLine("<p>To rank well in Albanian search results, articles must be comprehensive, well-structured, and written in natural language that reflects real user intent. Thin content no longer performs consistently. High-quality pages keep readers engaged longer, increase relevance signals, and attract qualified organic traffic. That traffic quality directly supports Google AdSense outcomes because better audience fit often improves ad engagement without sacrificing user trust. The winning strategy is straightforward: publish evergreen guides around real technical problems, refresh them periodically, and connect each article to a clear next action. This creates a full conversion path where education, credibility, and service delivery reinforce each other. Long-form educational content remains one of the most efficient assets for sustained growth.</p>");

        sb.AppendLine("<h3>Actionable conclusion</h3>");
        sb.AppendLine("<p>If you want reliable quality, controlled costs, and faster execution, treat the process as guided collaboration rather than a one-time request. Prepare facts, compare options, and ensure every step is documented before implementation. This reduces risk, improves accountability, and makes final outcomes easier to evaluate. In competitive markets, trust is built through consistency: clear communication, predictable execution, and transparent pricing. Combined with strong editorial content, this approach supports long-term SEO authority, stronger brand reputation, and measurable business growth.</p>");

        return sb.ToString();
    }

    private static string EnsureWordRange(string html, string language, int minWords, int maxWords)
    {
        var extraSq = "<p>Për ta mbajtur cilësinë e shërbimit të lartë, rekomandohet që çdo ndërhyrje të dokumentohet me foto para dhe pas punës, me shënime të qarta për materialet e përdorura dhe me afat të dakordësuar për kontrollin pasues. Kjo rrit përgjegjshmërinë, ul konfliktet dhe ndihmon klientin të kuptojë vlerën reale të investimit.</p>";
        var extraEn = "<p>To maintain high service quality, each intervention should be documented with before-and-after photos, concise notes on materials used, and a follow-up check timeline agreed by both sides. This improves accountability, reduces disputes, and helps clients understand the real value of the investment.</p>";

        var result = html;
        var guard = 0;

        while (CountWords(ToPlainText(result)) < minWords && guard < 20)
        {
            result += language == "sq" ? extraSq : extraEn;
            guard++;
        }

        var plain = ToPlainText(result);
        var words = plain.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > maxWords)
        {
            plain = string.Join(' ', words.Take(maxWords));
            result = $"<p>{plain}</p>";
        }

        return result;
    }

    private static int CountWords(string input)
    {
        return input
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }

    private static string ToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var noTags = Regex.Replace(html, "<.*?>", " ");
        return Regex.Replace(noTags, "\\s+", " ").Trim();
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.Length <= max ? text : text[..max] + "...";
    }
}
