using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Blog
{
    public class BlogIndexModel : PageModel
    {
        private readonly IBlogService _blogService;

        public BlogIndexModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public List<BlogArticle> Articles { get; set; } = new();

        public async Task OnGetAsync()
        {
            var dbPosts = await _blogService.GetAllActiveAsync();
            if (dbPosts.Any())
            {
                Articles = dbPosts.Select(p => new BlogArticle
                {
                    Slug = p.Slug,
                    Title = p.Title,
                    MetaDescription = p.MetaDescription ?? "",
                    Excerpt = p.Excerpt ?? "",
                    Category = p.Category,
                    CategoryIcon = p.CategoryIcon,
                    Author = p.Author,
                    PublishedDate = p.PublishedDate,
                    ModifiedDate = p.ModifiedDate,
                    ReadTime = p.ReadTime,
                    ImageIcon = p.ImageIcon,
                    Tags = (p.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Content = p.Content
                }).ToList();
            }
            else
            {
                // Fallback to static data if DB is empty
                Articles = BlogData.GetAllArticles();
            }
        }
    }

    public class BlogArticle
    {
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public string MetaDescription { get; set; } = "";
        public string Excerpt { get; set; } = "";
        public string Category { get; set; } = "";
        public string CategoryIcon { get; set; } = "bi-tag";
        public string Author { get; set; } = "TeknoSOS";
        public DateTime PublishedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ReadTime { get; set; } = "3 min";
        public string ImageIcon { get; set; } = "bi-file-text";
        public List<string> Tags { get; set; } = new();
        public string Content { get; set; } = "";
    }

    public static class BlogData
    {
        public static List<BlogArticle> GetAllArticles()
        {
            return new List<BlogArticle>
            {
                // Article 1
                new BlogArticle
                {
                    Slug = "5-shenjat-qe-keni-nevoje-per-elektricist",
                    Title = "5 Shenjat Që Tregojnë Se Keni Nevojë për Elektricist",
                    MetaDescription = "Mësoni 5 shenjat kryesore që tregojnë se sistemi juaj elektrik ka nevojë për ndërhyrjen e një elektricisti profesionist. Mos i neglizhoni këto sinjale rreziku.",
                    Excerpt = "A dini kur duhet të telefononi një elektricist? Këto 5 shenja të qarta ju ndihmojnë të kuptoni nëse keni defekt elektrik në shtëpi.",
                    Category = "Siguria Elektrike",
                    CategoryIcon = "bi-shield-check",
                    PublishedDate = new DateTime(2026, 3, 10),
                    ReadTime = "4 min",
                    ImageIcon = "bi-lightning-charge",
                    Tags = new List<string> { "elektricist", "defekt elektrik", "siguri", "shtëpi" },
                    Content = @"
<h2>Pse Është e Rëndësishme të Njohësh Shenjat e Defekteve Elektrike?</h2>
<p>Sistemi elektrik i shtëpisë tuaj është njëra nga pjesët më të rëndësishme të infrastrukturës së banesës. Kur diçka nuk funksionon siç duhet, pasojat mund të jenë serioze — nga dëmtimi i pajisjeve deri tek rreziku i zjarrit. Njohja e shenjave të para ju ndihmon të veproni shpejt dhe të shmangni problemet e mëdha.</p>

<h3>1. Siguresa Bien Shpesh</h3>
<p>Nëse siguresa (siguresat automatike) bien në mënyrë të përsëritur, kjo tregon se ka mbingarkesë në qark ose një defekt serioz. Mos i rivendosni thjesht — telefononi një <strong>elektricist profesionist</strong> që të kontrollojë instalimin.</p>

<h3>2. Prizat ose Çelësat Janë të Nxehtë</h3>
<p>Nëse prekni një prizë ose çelës drrite dhe ndiheni nxehtësi, është shenjë alarmi. Kjo mund të shkaktohet nga lidhje të dobëta, tela të dëmtuara ose mbingarkesë. Një <strong>defekt i tillë elektrik</strong> kërkon kontroll të menjëhershëm.</p>

<h3>3. Erë Djegie ose Tym</h3>
<p>Nëse ndieni erë djegie pranë prizave, panelit elektrik ose pajisjeve, fikni menjëherë rrymën dhe telefononi emergjencën elektrike. Kjo është një nga <strong>shenjat më serioze të defektit elektrik</strong> dhe nuk duhet neglizhuar asnjëherë.</p>

<h3>4. Drrita Vezulluese ose të Dobëta</h3>
<p>Nëse drritat në shtëpi vezullojnë ose janë më të dobëta se zakonisht, kjo mund të tregojë probleme me tensionin ose lidhje të paqëndrueshme. Një <strong>elektricist i certifikuar</strong> mund të diagnostikojë çështjen shpejt.</p>

<h3>5. Instalim i Vjetër (Mbi 20 Vjet)</h3>
<p>Nëse banesa juaj ka më shumë se 20 vjet pa asnjë rinovim të instalimit elektrik, ka ardhur koha për një kontroll të plotë. Instalimet e vjetra nuk përballojnë ngarkesën e pajisjeve moderne dhe shtojnë rrezikun e defekteve.</p>

<h2>Çfarë Duhet të Bëni?</h2>
<p>Nëse keni vërejtur ndonjë nga këto shenja, mos prisni derisa problemi të përkeqësohet. Në <strong>TeknoSOS</strong>, ju mund të gjeni elektricistë të verifikuar pranë jush me vetëm disa klikime. Raportoni defektin tuaj dhe merrni ndihmën që meritoni — shpejt, sigurt dhe profesionalisht.</p>

<div class='blog-cta'>
    <p><strong>Keni një defekt elektrik?</strong> <a href='/ReportDefect'>Raportoni tani në TeknoSOS</a> dhe gjeni elektricistin më të afërt!</p>
</div>"
                },

                // Article 2
                new BlogArticle
                {
                    Slug = "si-te-zgjidhni-elektricistin-e-duhur",
                    Title = "Si të Zgjidhni Elektricistin e Duhur: Guida e Plotë",
                    MetaDescription = "Zbuloni si të zgjidhni elektricistin më të mirë për punën tuaj. Këshilla praktike për të gjetur profesionistin e duhur në Shqipëri.",
                    Excerpt = "Zgjedhja e elektricistit të duhur mund të jetë e vështirë. Ja si mund ta bëni këtë vendim me besim duke ndjekur disa hapa të thjeshtë.",
                    Category = "Këshilla Praktike",
                    CategoryIcon = "bi-lightbulb",
                    PublishedDate = new DateTime(2026, 3, 8),
                    ReadTime = "5 min",
                    ImageIcon = "bi-person-check",
                    Tags = new List<string> { "elektricist", "zgjedhje", "këshilla", "profesionist" },
                    Content = @"
<h2>Pse Ka Rëndësi Zgjedhja e Elektricistit të Duhur?</h2>
<p>Kur bëhet fjalë për punë elektrike, cilësia e punës ndikon drejtpërdrejt në sigurinë e familjes suaj. Një elektricist i pakualifikuar mund të krijojë probleme edhe më të mëdha se ato që kishit fillimisht. Prandaj, është thelbësore të zgjidhni dikë me përvojë dhe besueshmëri.</p>

<h3>Kontrolloni Kualifikimet dhe Licencën</h3>
<p>Para se të punësoni një elektricist, sigurohuni që ai ka <strong>licencën e duhur profesionale</strong>. Në Shqipëri, elektricistët duhet të kenë certifikata që vërtetojnë aftësinë e tyre. Mos hezitoni të pyesni për dokumentacionin — një profesionist i vërtetë do t'jua tregojë me kënaqësi.</p>

<h3>Lexoni Vlerësimet e Klientëve të Mëparshëm</h3>
<p>Opinionet e njerëzve që kanë përdorur shërbimet e një elektricisti janë të çmuara. Në platformën <strong>TeknoSOS</strong>, çdo teknik ka profil me <strong>vlerësime dhe komente</strong> nga klientë të vërtetë. Kjo ju ndihmon të merrni vendimin e duhur para se të telefononi.</p>

<h3>Krahasoni Çmimet — Por Jo Vetëm Çmimin</h3>
<p>Çmimi më i ulët nuk do të thotë gjithmonë punë më e mirë. Krahasoni ofertat duke marrë parasysh përvojën, materialet që përdoren dhe garancinë që ofrojnë. Një <strong>elektricist i mirë në Tiranë</strong> ose çdo qytet tjetër do t'ju japë një preventiv të qartë para se të fillojë punën.</p>

<h3>Verifikoni Disponueshmërinë për Emergjenca</h3>
<p>Defektet elektrike nuk presin orarit zyrtar. Sigurohuni që elektricisti juaj ofron <strong>shërbim emergjence</strong> ose ka disponueshmëri jashtë orarit. Kjo është veçanërisht e rëndësishme për familje me fëmijë të vegjël ose të moshuar.</p>

<h2>Gjeni Elektricistin Ideal në TeknoSOS</h2>
<p>Me <strong>TeknoSOS</strong>, gjetja e elektricistit të duhur është e thjeshtë. Platforma jonë ju lidhur me teknikë të verifikuar, me profil të plotë, vlerësime reale dhe mundësi komunikimi direkt. Mos humbni kohë duke kërkuar — le te teknologjia ju ndihmojë.</p>

<div class='blog-cta'>
    <p><strong>Gati për të gjetur elektricistin tuaj?</strong> <a href='/Technicians'>Shikoni teknikët e verifikuar në TeknoSOS</a></p>
</div>"
                },

                // Article 3
                new BlogArticle
                {
                    Slug = "defekt-elektrik-urgjent-cfare-duhet-bere",
                    Title = "Defekt Elektrik Urgjent? Ja Çfarë Duhet të Bëni Menjëherë",
                    MetaDescription = "Udhëzues i shpejtë se çfarë duhet të bëni kur keni defekt elektrik urgjent në shtëpi. Hapa të sigurt për të mbrojtur familjen dhe pronën tuaj.",
                    Excerpt = "Kur ndodh një defekt elektrik urgjent, çdo sekondë ka rëndësi. Mësoni hapat e sakta për të vepruar sigurt dhe profesionalisht.",
                    Category = "Emergjenca",
                    CategoryIcon = "bi-exclamation-triangle",
                    PublishedDate = new DateTime(2026, 3, 12),
                    ReadTime = "3 min",
                    ImageIcon = "bi-exclamation-octagon",
                    Tags = new List<string> { "emergjencë", "defekt elektrik", "siguri", "ndihmë e shpejtë" },
                    Content = @"
<h2>Hapi i Parë: Mos u Panikosni</h2>
<p>Kur ndodh një <strong>defekt elektrik urgjent</strong> — si një shkëndijë nga priza, erë djegie ose humbje totale e rrymës — gjëja më e rëndësishme është të qëndroni të qetë. Paniku çon në vendime të gabuara, ndërsa qetësia ju ndihmon të mbroni veten dhe familjen.</p>

<h3>Fikni Rrymën nga Paneli Kryesor</h3>
<p>Nëse ka tym, erë djegie ose shkëndija, hapi i parë është të <strong>fikni çelësin kryesor</strong> të panelet elektrik (tablosë). Kjo ndërpret furnizimin me rrymë dhe ul rrezikun e zjarrit ose goditjes elektrike. Sigurohuni që e dini ku ndodhet tabloja — është informacion që çdo anëtar i familjes duhet ta dijë.</p>

<h3>Largohuni nga Zona e Rrezikut</h3>
<p>Nëse ka ujë në dysheme pranë zonës elektrike, ose nëse shihni tela të ekspozuara, <strong>mos u afroni</strong>. Uji përçon rrymën dhe rreziku i goditjes elektrike është i lartë. Evakuoni fëmijët dhe kafshët shtëpiake menjëherë.</p>

<h3>Telefononi një Elektricist Profesionist</h3>
<p>Mos u mundoni ta riparoni vetë defektin — kjo mund të jetë e rrezikshme. Telefononi një <strong>elektricist të licencuar</strong> që ka përvojë me emergjenca. Në TeknoSOS, mund të gjeni teknikë që ofrojnë <strong>shërbim urgjent 24/7</strong> pranë vendndodhjes suaj.</p>

<h3>Dokumentoni Problemin</h3>
<p>Ndërsa prisni elektricistin, bëni shënime ose foto të zonës problematike (nga distanca e sigurt). Kjo ndihmon teknikun të kuptojë situatën para se të arrijë dhe të përgatisë vegëlat e duhura.</p>

<h2>Parandalimi Është Më i Mirë se Kurimi</h2>
<p>Shumica e emergjencave elektrike mund të parandaloheshin me <strong>kontroll periodik të instalimit</strong>. Rekomandohet një inspektim profesional çdo 3-5 vjet. Me TeknoSOS, organizimi i këtij kontrolli është i thjeshtë — zgjidhni teknikun, caktoni takimin dhe siguroni shtëpinë tuaj.</p>

<div class='blog-cta'>
    <p><strong>Keni emergjencë elektrike tani?</strong> <a href='/ReportDefect'>Raportoni defektin menjëherë</a> dhe merrni ndihmë brenda minutave!</p>
</div>"
                },

                // Article 4
                new BlogArticle
                {
                    Slug = "kurseni-energji-elektrike-ne-shtepi",
                    Title = "10 Mënyra të Thjeshta për të Kursyer Energji Elektrike në Shtëpi",
                    MetaDescription = "Zbuloni 10 këshilla praktike për të ulur faturën e rrymës elektrike. Metoda të thjeshta që çdokush mund t'i zbatojë sot.",
                    Excerpt = "Fatura e rrymës po rritet? Këto 10 këshilla të thjeshta ju ndihmojnë të kurseni energji dhe para — pa sakrifikuar komoditetin.",
                    Category = "Kursim Energjie",
                    CategoryIcon = "bi-battery-charging",
                    PublishedDate = new DateTime(2026, 3, 6),
                    ReadTime = "5 min",
                    ImageIcon = "bi-lightning",
                    Tags = new List<string> { "kursim energjie", "rrymë", "faturë", "shtëpi", "këshilla" },
                    Content = @"
<h2>Pse Duhet të Kursejmë Energji?</h2>
<p>Kursimi i energjisë elektrike nuk është vetëm çështje ekonomike — është edhe përgjegjësi ndaj mjedisit. Në Shqipëri, çmimet e energjisë kanë pësuar rritje, dhe çdo familje kërkon mënyra për të ulur faturat. Lajmi i mirë? Ka shumë gjëra të thjeshta që mund të bëni sot.</p>

<h3>1. Kaloni në Llamba LED</h3>
<p>Llambat LED konsumojnë <strong>deri në 80% më pak energji</strong> se llambat tradicionale dhe zgjasin 25 herë më shumë. Investimi fillestar kthehet brenda disa muajsh përmes faturave më të ulëta.</p>

<h3>2. Fikni Pajisjet në Standby</h3>
<p>Televizori, kompjuteri dhe karikuesit e telefonit konsumojnë rrymë edhe kur janë në <strong>modalitetin standby</strong>. Përdorni priza me çelës ose hiqni spinën kur nuk i përdorni — kjo mund të kursejë 5-10% të faturës mujore.</p>

<h3>3. Përdorni Pajisje me Efikasitet të Lartë Energjitik</h3>
<p>Kur blini pajisje të reja shtëpiake, kontrolloni <strong>etiketën energjitike</strong>. Pajisjet me klasë A+++ konsumojnë shumë më pak rrymë se ato me klasë më të ulët.</p>

<h3>4. Izoloni Shtëpinë Mirë</h3>
<p>Izolimi i mirë termik ul nevojën për ngrohje dhe ftohje, që janë konsumuesit më të mëdhenj të energjisë. Kontrolloni dritaret, dyert dhe çatinë për humbje termike.</p>

<h3>5. Kontrolloni Instalimin Elektrik</h3>
<p>Një <strong>instalim i vjetër ose i dëmtuar</strong> mund të shkaktojë humbje energjie. Një kontroll profesional nga një elektricist i TeknoSOS mund të zbulojë probleme të fshehura që ju kushtojnë para çdo muaj.</p>

<h3>6-10: Më Shumë Këshilla të Shpejta</h3>
<ul>
    <li><strong>6.</strong> Përdorni termostat të programueshëm për ngrohjen.</li>
    <li><strong>7.</strong> Lani rrobat me ujë të ftohtë sa herë të jetë e mundur.</li>
    <li><strong>8.</strong> Mos hapni derën e frigoiferit pa nevojë.</li>
    <li><strong>9.</strong> Shfrytëzoni drritën natyrale gjatë ditës.</li>
    <li><strong>10.</strong> Vendosni panele diellore nëse keni mundësi — investim afatgjatë i shkëlqyer.</li>
</ul>

<h2>Filloni me Hapin e Parë</h2>
<p>Nuk ka nevojë t'i zbatoni të gjitha menjëherë. Filloni me 2-3 ndryshime të vogla dhe shikoni diferencën në faturën e ardhshme. Dhe nëse dyshoni se instalimi juaj ka probleme, <strong>TeknoSOS</strong> ju lidhur me elektricistë profesionistë që mund ta zgjidhin shpejt.</p>

<div class='blog-cta'>
    <p><strong>Doni të kontrolloni instalimin tuaj?</strong> <a href='/Technicians'>Gjeni një elektricist pranë jush në TeknoSOS</a></p>
</div>"
                },

                // Article 5
                new BlogArticle
                {
                    Slug = "pse-duhet-kontroll-periodik-instalimit-elektrik",
                    Title = "Pse Duhet të Bëni Kontroll Periodik të Instalimit Elektrik",
                    MetaDescription = "Mësoni pse kontrolli periodik i instalimit elektrik është thelbësor për sigurinë e shtëpisë suaj dhe si mund t'ju ndihmojë TeknoSOS.",
                    Excerpt = "Kontrolli periodik i instalimit elektrik mund t'ju mbrojë nga zjarret, goditjet elektrike dhe faturat e papritura. Ja pse nuk duhet neglizhuar.",
                    Category = "Mirëmbajtje",
                    CategoryIcon = "bi-tools",
                    PublishedDate = new DateTime(2026, 3, 4),
                    ReadTime = "4 min",
                    ImageIcon = "bi-clipboard-check",
                    Tags = new List<string> { "kontroll", "instalim", "mirëmbajtje", "siguri" },
                    Content = @"
<h2>Sa Shpesh Duhet Kontrolluar Instalimi Elektrik?</h2>
<p>Ekspertët rekomandojnë që instalimi elektrik i një banese të kontrollohet <strong>të paktën çdo 5 vjet</strong>. Për shtëpi të vjetra (mbi 20 vjet), kontrolli duhet bërë çdo 3 vjet. Nëse keni bërë rinovime ose keni shtuar pajisje të reja me konsum të lartë (si kondicionerë ose karikues EV), kontroll shtesë rekomandohet.</p>

<h3>Çfarë Përfshin një Kontroll Profesional?</h3>
<p>Një <strong>elektricist i certifikuar</strong> do të kontrollojë:</p>
<ul>
    <li>Panelin e shpërndarjes (tablosë) dhe siguresat</li>
    <li>Gjendjen e telave dhe kabllove</li>
    <li>Lidhjet në priza, çelësa dhe kuti shpërndarëse</li>
    <li>Sistemin e tokëzimit</li>
    <li>Mbingarkesën në qarqe</li>
    <li>Përputhjen me standardet aktuale</li>
</ul>

<h3>Rreziqet e Neglizhimit</h3>
<p>Instalimi i pakontrolluar mund të çojë në:</p>
<ul>
    <li><strong>Zjarr elektrik</strong> — shkaku kryesor i zjarreve në banese në Shqipëri</li>
    <li><strong>Goditje elektrike</strong> — veçanërisht e rrezikshme për fëmijët</li>
    <li><strong>Dëmtim pajisjesh</strong> — tensioni i paqëndrueshëm dëmton pajisjet elektronike</li>
    <li><strong>Fatura më të larta</strong> — humbjet e energjisë nga lidhje të dobëta</li>
</ul>

<h3>Si Mund t'Ju Ndihmojë TeknoSOS?</h3>
<p>Me platformën <strong>TeknoSOS</strong>, caktimi i një kontrolli periodik është i thjeshtë. Zgjidhni qytetin tuaj, shikoni profilet e <strong>elektricistëve të verifikuar</strong>, lexoni vlerësimet e klientëve të tjerë dhe caktoni takimin — gjithçka online. Mund të krahasoni çmimet, të komunikoni drejtpërdrejt me teknikun dhe të merrni garancinë e punës.</p>

<h2>Mos Prisni Derisa të Jetë Vonë</h2>
<p>Parandalimi është gjithmonë më i lirë dhe më i sigurt se riparimi pas aksidentit. Një kontroll i zakonshëm zgjat vetëm 1-2 orë dhe mund t'ju kursejë mijëra euro në dëme. Bëjeni sot — familja juaj e meriton.</p>

<div class='blog-cta'>
    <p><strong>Caktoni kontrollin tuaj të parë!</strong> <a href='/Technicians'>Zgjidhni elektricistin në TeknoSOS</a> dhe siguroni shtëpinë tuaj.</p>
</div>"
                },

                // Article 6
                new BlogArticle
                {
                    Slug = "karikues-makinash-elektrike-ne-shtepi",
                    Title = "Karikues Makinash Elektrike në Shtëpi: Gjithçka Që Duhet të Dini",
                    MetaDescription = "Udhëzues i plotë për instalimin e karikuesit të makinës elektrike në shtëpi. Llojet, kostot dhe si të gjeni elektricistin e duhur në Shqipëri.",
                    Excerpt = "Makinat elektrike po bëhen gjithnjë e më popullore në Shqipëri. Ja gjithçka që duhet të dini për instalimin e karikuesit në shtëpi.",
                    Category = "Teknologji",
                    CategoryIcon = "bi-ev-station",
                    PublishedDate = new DateTime(2026, 3, 14),
                    ReadTime = "5 min",
                    ImageIcon = "bi-ev-front",
                    Tags = new List<string> { "karikues EV", "makinë elektrike", "instalim", "teknologji" },
                    Content = @"
<h2>Pse të Instaloni Karikues në Shtëpi?</h2>
<p>Me rritjen e numrit të makinave elektrike (EV) në Shqipëri, pasja e një <strong>karikuesi në shtëpi</strong> është bërë një nevojë reale. Karikimi në shtëpi është më i përshtatshëm, më i lirë dhe eliminon nevojën për vizita të shpeshta në stacionet publike. Por si funksionon instalimi dhe çfarë duhet të dini?</p>

<h3>Llojet e Karikuesve për Shtëpi</h3>
<p>Ka dy lloje kryesore karikuesish për përdorim shtëpiak:</p>
<ul>
    <li><strong>Niveli 1 (Prizë Standarde)</strong> — Përdor prizën normale 220V. I ngadaltë (6-12 orë për karikim të plotë), por nuk kërkon instalim të veçantë.</li>
    <li><strong>Niveli 2 (Wallbox)</strong> — Kërkon instalim special me qark të dedikuar. Shumë më i shpejtë (3-6 orë) dhe i rekomanduar për përdorim të përditshëm.</li>
</ul>

<h3>Çfarë Nevojitet për Instalimin?</h3>
<p>Para se të instaloni një karikues Niveli 2, duhet të siguroheni që:</p>
<ul>
    <li>Paneli elektrik ka <strong>kapacitet të mjaftueshëm</strong> (zakonisht 32A shtesë)</li>
    <li>Kablloja nga paneli te garazhi/parkingu është e dimensionuar mirë</li>
    <li>Ka <strong>mbrojtje diferenciale (RCD)</strong> të dedikuar</li>
    <li>Instalimi respekton standardet e sigurrisë</li>
</ul>
<p>Këto janë punë që duhet bërë vetëm nga një <strong>elektricist i licencuar</strong> — mos u mundoni vetë, pasi rreziku elektrik është i lartë.</p>

<h3>Sa Kushton Instalimi?</h3>
<p>Kostoja e instalimit varion sipas kompleksitetit:</p>
<ul>
    <li><strong>Karikuesi (Wallbox):</strong> 300-800 EUR</li>
    <li><strong>Instalimi elektrik:</strong> 200-500 EUR</li>
    <li><strong>Totali mesatar:</strong> 500-1,300 EUR</li>
</ul>
<p>Çmimi perfundimtar varet nga distanca e panelit, gjendja e instalimit ekzistues dhe marka e karikuesit. Një elektricist profesionist në <strong>TeknoSOS</strong> mund t'ju japë preventiv të saktë pas inspektimit.</p>

<h3>Përfitimet Afatgjata</h3>
<p>Karikimi në shtëpi kushton mesatarisht <strong>70-80% më pak</strong> se karburanti për makina me benzinë. Për një familje Shqiptare që udhëton 15,000 km/vit, kjo do të thotë kursim i konsiderueshëm. Plus, vlera e pronës tuaj rritet me prani të pikës së karikimit.</p>

<h2>Filloni me TeknoSOS</h2>
<p>Nëse jeni gati të instaloni karikuesin tuaj të parë shtëpiak, <strong>TeknoSOS</strong> ju lidhur me elektricistë të specializuar në instalimet EV. Shikoni profilet, krahasoni vlerësimet dhe caktoni takimin — e tëra online.</p>

<div class='blog-cta'>
    <p><strong>Gati për karikuesin tuaj EV?</strong> <a href='/KarikuesEV'>Zbuloni shërbimin e instalimit të karikuesve në TeknoSOS</a></p>
</div>"
                },

                // Article 7 — Hidrauliku
                new BlogArticle
                {
                    Slug = "probleme-hidraulike-ne-shtepi-si-ti-zgjidhni",
                    Title = "Problemet Hidraulike Më të Zakonshme në Shtëpi dhe Si t'i Zgjidhni",
                    MetaDescription = "Zbuloni problemet hidraulike më të shpeshta në shtëpi — rrjedhje uji, tuba të bllokuara, bojler me defekt — dhe si të gjeni hidraulikun e duhur në Shqipëri.",
                    Excerpt = "Rrjedhje uji, tuba të bllokuara apo bojler që nuk ngroh? Mësoni si t'i njihni problemet hidraulike dhe kur duhet të telefononi një profesionist.",
                    Category = "Hidraulikë",
                    CategoryIcon = "bi-droplet",
                    PublishedDate = new DateTime(2026, 3, 13),
                    ReadTime = "4 min",
                    ImageIcon = "bi-droplet-half",
                    Tags = new List<string> { "hidraulik", "rrjedhje uji", "tuba", "bojler", "mirëmbajtje" },
                    Content = @"
<h2>Pse Problemet Hidraulike Nuk Duhen Neglizhuar?</h2>
<p>Sistemi hidraulik i shtëpisë përfshin të gjitha tubacionet që furnizojnë ujin e pastër dhe largojnë ujërat e ndotura. Kur ky sistem ka defekt, pasojat janë të menjëhershme: dëme nga uji, lagështi, myku, dhe fatura shumë të larta uji. Sa më shpejt të veproni, aq më pak dëme do të keni.</p>

<h3>Rrjedhjet e Ujit — Armiku i Heshtur</h3>
<p>Një <strong>rrjedhje e vogël uji</strong> mund të duket e parëndësishme, por me kalimin e kohës shkakton dëme serioze. Rrjedhjet mund të ndodhin në tubat nën lavaman, në lidhjet e banjës, ose edhe brenda mureve ku nuk shihen. Nëse vëreni njolla lagështie në mure ose tavan, telefononi menjëherë një <strong>hidraulik profesionist</strong>.</p>

<h3>Tuba të Bllokuara — Problem i Shpeshtë</h3>
<p>Bllokimi i tubave është njëri nga problemet më të zakonshme hidraulike. Shkaqet kryesore janë:</p>
<ul>
    <li><strong>Grumbullimi i flokëve dhe sapunit</strong> në tubat e dushit</li>
    <li><strong>Yndyra dhe mbetje ushqimore</strong> në lavaman e kuzhinës</li>
    <li><strong>Objekte të huaja</strong> që futen në tualet</li>
</ul>
<p>Mos përdorni kimikate agresive pa konsultuar një specialist — ato mund të dëmtojnë tubat. Një <strong>hidraulik i TeknoSOS</strong> ka vegëlat profesionale për pastrimin e sigurt.</p>

<h3>Bojleri Nuk Ngroh Mirë</h3>
<p>Nëse <strong>bojleri nuk ngroh ujin</strong> ose merr shumë kohë, problemet mund të jenë: rezistenca e djegur, termostati i defektuar, ose gëlqerja e akumuluar. Kontrolli periodik i bojlerit çdo 12 muaj nga një profesionist zgjat jetëgjatësinë e pajisjes dhe ul faturat.</p>

<h3>Kur Duhet të Telefononi Hidraulikun?</h3>
<p>Telefononi menjëherë nëse:</p>
<ul>
    <li>Ka rrjedhje uji që nuk ndalet</li>
    <li>Uji del me ngjyrë të ndryshme (kafe ose i verdhë)</li>
    <li>Presioni i ujit ka rënë papritur</li>
    <li>Ndieni erë të keqe nga tubat</li>
    <li>Bojleri bën zhurma të çuditshme</li>
</ul>

<h2>Gjeni Hidraulikun e Duhur me TeknoSOS</h2>
<p>Me <strong>TeknoSOS</strong>, gjetja e një hidrauliku të verifikuar është e thjeshtë. Shikoni profilet, lexoni vlerësimet e klientëve dhe caktoni takimin — gjithçka online, pa stres.</p>

<div class='blog-cta'>
    <p><strong>Keni problem hidraulik?</strong> <a href='/Technicians'>Gjeni hidraulikun më të afërt në TeknoSOS</a> dhe zgjidheni sot!</p>
</div>"
                },

                // Article 8 — Ngrohje-Ftohje (HVAC)
                new BlogArticle
                {
                    Slug = "sistemi-ngrohje-ftohje-guida-e-plote",
                    Title = "Sistemi i Ngrohjes dhe Ftohjes: Guida e Plotë për Shtëpinë Tuaj",
                    MetaDescription = "Gjithçka rreth sistemeve të ngrohjes dhe ftohjes — kondicionerë, kalldaja, pompa nxehtësie. Si të zgjidhni, mirëmbani dhe kurseni energji në Shqipëri.",
                    Excerpt = "Kondicioneri nuk ftoh mirë? Kalldaja konsumon shumë? Mësoni si të mirëmbani sistemin e ngrohje-ftohjes dhe kur t'i drejtoheni një tekniku.",
                    Category = "Ngrohje & Ftohje",
                    CategoryIcon = "bi-thermometer-half",
                    PublishedDate = new DateTime(2026, 3, 11),
                    ReadTime = "5 min",
                    ImageIcon = "bi-thermometer-sun",
                    Tags = new List<string> { "ngrohje", "ftohje", "kondicioner", "kalldajë", "HVAC" },
                    Content = @"
<h2>Pse Është i Rëndësishëm Sistemi i Ngrohje-Ftohjes?</h2>
<p>Në Shqipëri, temperaturat variojnë nga -5°C në dimër deri në 40°C në verë. Një <strong>sistem i mirë ngrohje-ftohje</strong> nuk është luks — është domosdoshmëri për komoditetin dhe shëndetin e familjes. Por sistemi duhet mirëmbajtur rregullisht për të funksionuar me efikasitet.</p>

<h3>Kondicionerët: Mirëmbajtja Që Shumica Harrojnë</h3>
<p>Kondicioneri kërkon <strong>pastrim të filtrave çdo 2-4 javë</strong> gjatë përdorimit aktiv. Filtrat e ndotur ulin kapacitetin ftohës deri 15% dhe rrisin konsumin e rrymës. Përveç kësaj, njësia e jashtme duhet pastruar nga pluhuri dhe gjethet të paktën dy herë në vit.</p>
<p>Shenjat që kondicioneri ka nevojë për servis profesional:</p>
<ul>
    <li>Ftoh ose ngroh më dobët se zakonisht</li>
    <li>Bën zhurma të pazakonta</li>
    <li>Ka erë të pakëndshme kur ndizet</li>
    <li>Rrjedh ujë brenda shtëpisë</li>
    <li>Kompresori ndizet dhe fiket shpesh</li>
</ul>

<h3>Kalldajat dhe Kthina Qendrore</h3>
<p>Për <strong>ngrohje qendrore</strong>, kalldajat me gaz ose me rrymë janë zgjedhja më e zakonshme. Kontrolli vjetor përfshin pastrimin e djegsisë, verifikimin e presionit, kontrollin e pompës qarkulluese dhe ajrosjen e radiatorëve. Një kalldajë e mirëmbajtur konsumon <strong>deri 20% më pak energji</strong>.</p>

<h3>Pompat e Nxehtësisë — E Ardhmja e Ngrohjes</h3>
<p>Pompat e nxehtësisë (heat pumps) po bëhen zgjedhja e preferuar në Shqipëri. Ato ngrohin në dimër dhe ftohin në verë me një pajisje të vetme, duke konsumuar <strong>3-4 herë më pak energji</strong> se ngrohëset elektrike tradicionale. Instalimi kërkon teknik të specializuar.</p>

<h2>Siguroni Komoditetin me TeknoSOS</h2>
<p>Pavarësisht nëse keni nevojë për servis kondicioneri, kontroll kalldaje ose instalim të sistemit të ri, <strong>TeknoSOS</strong> ju lidhur me teknikë të certifikuar për ngrohje-ftohjen. Caktoni takimin online dhe përfitoni shërbim profesional.</p>

<div class='blog-cta'>
    <p><strong>Kondicioneri nuk punon mirë?</strong> <a href='/Technicians'>Gjeni teknikun e duhur në TeknoSOS</a> për servis të shpejtë!</p>
</div>"
                },

                // Article 9 — Sistemi i Kamerave
                new BlogArticle
                {
                    Slug = "sistemi-kamerave-te-sigurise-per-shtepi-biznes",
                    Title = "Sistemi i Kamerave të Sigurisë: Guida për Shtëpi dhe Biznes",
                    MetaDescription = "Si të zgjidhni dhe instaloni sistemin e kamerave të sigurisë për shtëpinë ose biznesin tuaj. Llojet, kostot dhe këshilla nga ekspertët në Shqipëri.",
                    Excerpt = "Kamerat e sigurisë mbrojnë pronën dhe familjen tuaj 24/7. Mësoni llojet e kamerave, ku t'i vendosni dhe si të gjeni instaluesin e duhur.",
                    Category = "Siguri",
                    CategoryIcon = "bi-camera-video",
                    PublishedDate = new DateTime(2026, 3, 9),
                    ReadTime = "5 min",
                    ImageIcon = "bi-webcam",
                    Tags = new List<string> { "kamera sigurie", "CCTV", "siguri", "instalim", "biznes" },
                    Content = @"
<h2>Pse Janë të Domosdoshme Kamerat e Sigurisë?</h2>
<p>Statistikat tregojnë se <strong>pronat me kamera sigurie</strong> kanë deri 60% më pak mundësi për tu bërë objektiv i vjedhjeve. Kamerat jo vetëm parandalojnë krimin, por ofrojnë prova të vlefshme në rast incidenti dhe ju lejojnë të monitoroni pronën nga çdo vendë përmes telefonit.</p>

<h3>Llojet e Kamerave të Sigurisë</h3>
<p>Tregu ofron disa lloje kamerash, secila me avantazhet e veta:</p>
<ul>
    <li><strong>Kamera IP (me rrjet)</strong> — Cilësi e lartë imazhi (2K-4K), shikim nga telefoni, ruajtje në cloud ose NVR</li>
    <li><strong>Kamera Analoge (CCTV)</strong> — Më të lira, ideale për sipërfaqe të mëdha, ruajtje në DVR</li>
    <li><strong>Kamera Wi-Fi</strong> — Instalim i thjeshtë pa kabllo, perfekte për shtëpi</li>
    <li><strong>Kamera PTZ</strong> — Lëvizëse me telekomandë, mbulojnë sipërfaqe të gjera</li>
</ul>

<h3>Ku Duhet Vendosur Kamerat?</h3>
<p>Vendosja strategjike e kamerave është po aq e rëndësishme sa cilësia e tyre:</p>
<ul>
    <li><strong>Hyrja kryesore</strong> — Pika më e rëndësishme e monitorimit</li>
    <li><strong>Hyrjet anësore dhe garazhi</strong> — Pikat e dobëta të sigurisë</li>
    <li><strong>Perimetri i shtëpisë</strong> — Kamerat e jashtme me ndriçim infrared (IR)</li>
    <li><strong>Ambienti i brendshëm</strong> — Korridoret, shkallët dhe zonat kryesore</li>
</ul>

<h3>Sa Kushton Sistemi i Plotë?</h3>
<p>Kostoja varet nga numri i kamerave dhe cilësia:</p>
<ul>
    <li><strong>Sistemi bazë (4 kamera):</strong> 200-400 EUR</li>
    <li><strong>Sistemi mesatar (8 kamera + NVR):</strong> 400-800 EUR</li>
    <li><strong>Sistemi profesional (16+ kamera):</strong> 800-2,000+ EUR</li>
    <li><strong>Instalimi profesional:</strong> 150-400 EUR</li>
</ul>

<h2>Instalim Profesional me TeknoSOS</h2>
<p>Instalimi i kamerave kërkon njohuri teknike — pozicionimi, kabllimi, konfigurimi i rrjetit dhe aplikacionit. Një <strong>teknik i TeknoSOS</strong> ju ndihmon të zgjidhni sistemin e duhur dhe ta instalojë saktë, me garancinë e punës.</p>

<div class='blog-cta'>
    <p><strong>Doni të instaloni kamera sigurie?</strong> <a href='/Technicians'>Gjeni instaluesin profesionist në TeknoSOS</a></p>
</div>"
                },

                // Article 10 — Alarmi i Zjarrit
                new BlogArticle
                {
                    Slug = "alarmi-zjarrit-mbrojtja-e-shtepise-biznesit",
                    Title = "Alarmi i Zjarrit: Si të Mbroni Shtëpinë dhe Biznesin nga Zjarri",
                    MetaDescription = "Gjithçka rreth sistemit të alarmit të zjarrit — llojet e detektorëve, instalimi, mirëmbajtja dhe detyrimet ligjore në Shqipëri.",
                    Excerpt = "Zjarri mund të ndodhë në çdo moment. Mësoni si alarmi i zjarrit mund t'ju shpëtojë jetën dhe pronën — dhe si ta instaloni saktë.",
                    Category = "Siguri nga Zjarri",
                    CategoryIcon = "bi-fire",
                    PublishedDate = new DateTime(2026, 3, 7),
                    ReadTime = "4 min",
                    ImageIcon = "bi-bell",
                    Tags = new List<string> { "alarm zjarri", "detektor tymi", "siguri", "mbrojtje", "zjarr" },
                    Content = @"
<h2>Pse Është Jetik Alarmi i Zjarrit?</h2>
<p>Çdo vit në Shqipëri ndodhin qindra zjarre në banese dhe biznese. Shumica e viktimave nuk vdesin nga zjarri vetë, por nga <strong>tymi dhe gazrat toksike</strong> që çlirohen. Një sistem alarmi i zjarrit ju zgjjon para se situata të bëhet e pakontrollueshme — duke ju dhënë kohën e çmuar për evakuim.</p>

<h3>Llojet e Detektorëve</h3>
<ul>
    <li><strong>Detektorë tymi (fotoelektrik)</strong> — Ideale për dhoma gjumi dhe korridore. Reagojnë ndaj tymit të dendur</li>
    <li><strong>Detektorë tymi (jonizues)</strong> — Reagojnë më shpejt ndaj zjarreve me flakë të hapur</li>
    <li><strong>Detektorë nxehtësie</strong> — Perfektë për kuzhinat ku tymi i gatimit mund të shkaktojë alarme false</li>
    <li><strong>Detektorë gazi (CO)</strong> — Zbulojnë monoksidin e karbonit, gazin vdekjeprurës pa erë</li>
</ul>

<h3>Ku Duhen Vendosur Detektorët?</h3>
<p>Për mbrojtje optimale:</p>
<ul>
    <li>Një detektor tymi në <strong>çdo dhomë gjumi</strong></li>
    <li>Në korridor jashtë dhomave të gjumit</li>
    <li>Në çdo kat të shtëpisë, përfshirë bodruminë</li>
    <li>Detektor nxehtësie në kuzhinë</li>
    <li>Detektor CO pranë kalldajës ose oxhakut</li>
</ul>

<h3>Mirëmbajtja — Mos e Harroni!</h3>
<p>Sistemi i alarmit kërkon kontroll të rregullt:</p>
<ul>
    <li>Testoni alarmin <strong>çdo muaj</strong> duke shtypur butonin e testit</li>
    <li>Ndërroni bateritë <strong>çdo vit</strong> (ose kur dëgjoni sinjalin e baterisë së ulët)</li>
    <li>Pastroni detektorët nga pluhuri çdo 6 muaj</li>
    <li>Zëvendësoni detektorët çdo <strong>10 vjet</strong></li>
</ul>

<h2>Instalim i Sigurt me TeknoSOS</h2>
<p>Instalimi profesional i alarmit të zjarrit siguron që çdo zonë e pronës tuaj të jetë e mbuluar. Teknikët e <strong>TeknoSOS</strong> ju ndihmojnë me zgjedhjen e sistemit, instalimin dhe konfigurimin — për shtëpi ose biznes.</p>

<div class='blog-cta'>
    <p><strong>Mbroni familjen tuaj sot!</strong> <a href='/Technicians'>Gjeni teknikun e alarmit të zjarrit në TeknoSOS</a></p>
</div>"
                },

                // Article 11 — Mbrojtja nga Rrufetë
                new BlogArticle
                {
                    Slug = "mbrojtja-nga-rrufete-rrufepritesi",
                    Title = "Mbrojtja nga Rrufetë: Pse Çdo Ndërtesë Ka Nevojë për Rrufepritës",
                    MetaDescription = "Mësoni pse rrufepritësi është i domosdoshëm për mbrojtjen e ndërtesës, si funksionon dhe sa kushton instalimi i sistemit kundër rrufeve në Shqipëri.",
                    Excerpt = "Rrufetë mund të shkaktojnë dëme katastrofike në ndërtesa dhe pajisje elektronike. Njihuni me sistemin e rrufepritësit dhe mbrojtjen e duhur.",
                    Category = "Mbrojtje Elektrike",
                    CategoryIcon = "bi-cloud-lightning",
                    PublishedDate = new DateTime(2026, 3, 5),
                    ReadTime = "4 min",
                    ImageIcon = "bi-cloud-lightning-rain",
                    Tags = new List<string> { "rrufepritës", "mbrojtje", "rrufë", "tokëzim", "ndërtesë" },
                    Content = @"
<h2>Çfarë Dëmesh Shkaktojnë Rrufetë?</h2>
<p>Një goditje rrufeje mbart <strong>deri 300 milion volt</strong> dhe temperatura deri 30,000°C. Kur godet një ndërtesë pa mbrojtje, mund të shkaktojë zjarr, shkatërrim të instalimit elektrik, dëmtim të të gjitha pajisjeve elektronike dhe — në rastet më të rënda — humbje jete. Në Shqipëri, me stinën e stuhive gjatë vjeshtës dhe pranverës, mbrojtja nga rrufetë është thelbësore.</p>

<h3>Si Funksionon Rrufepritësi?</h3>
<p>Sistemi i rrufepritësit përbëhet nga tre komponentë kryesorë:</p>
<ul>
    <li><strong>Shufra kapëse (rrufepritësi)</strong> — Vendoset në pikën më të lartë të ndërtesës dhe tërheq rrufenë</li>
    <li><strong>Përçuesi zbrites</strong> — Kabllo e veçantë bakri ose alumini që drejton rrymën poshtë</li>
    <li><strong>Sistemi i tokëzimit</strong> — Shpërndan energjinë e rrufesë në tokë në mënyrë të sigurt</li>
</ul>
<p>Ky sistem krijon një rrugë me rezistencë të ulët për rrymën e rrufesë, duke e devijuar larg strukturës së ndërtesës dhe banorëve.</p>

<h3>Kush Ka Nevojë për Rrufepritës?</h3>
<ul>
    <li>Ndërtesat e larta (mbi 3 kate)</li>
    <li>Shtëpitë në zona malore ose të hapura</li>
    <li>Bizneset me pajisje të kushtueshme elektronike</li>
    <li>Ndërtesat me çati metalike</li>
    <li>Monumentet dhe ndërtesat publike</li>
</ul>

<h3>Mbrojtja Shtesë: SPD (Mbrojtësi i Mbingarkesave)</h3>
<p>Përveç rrufepritësit, rekomandohet fuqimisht instalimi i <strong>SPD (Surge Protection Device)</strong> në panelin elektrik. SPD-ja mbron pajisjet elektronike nga mbingarkessat që vijnë përmes linjave elektrike gjatë stuhive, edhe kur rrufeja nuk godet drejtpërdrejt ndërtesën tuaj.</p>

<h2>Instalim nga Profesionistët e TeknoSOS</h2>
<p>Instalimi i rrufepritësit kërkon njohuri të thelluara dhe respektim të standardeve. Me <strong>TeknoSOS</strong>, gjeni teknikë të specializuar që bëjnë projektimin, instalimin dhe testimin e sistemit të mbrojtjes nga rrufetë.</p>

<div class='blog-cta'>
    <p><strong>Mbroni ndërtesën tuaj nga rrufetë!</strong> <a href='/Technicians'>Gjeni specialistin në TeknoSOS</a></p>
</div>"
                },

                // Article 12 — Duralumini
                new BlogArticle
                {
                    Slug = "duralumini-dyer-dritare-moderne",
                    Title = "Duralumini: Dyer dhe Dritare Moderne për Shtëpinë Tuaj",
                    MetaDescription = "Gjithçka rreth dyerve dhe dritareve prej duralumini — përfitimet, llojet, izolimi termik dhe si të gjeni instaluesin e duhur në Shqipëri.",
                    Excerpt = "Duralumini ka revolucionarizuar ndërtimtarinë moderne. Zbuloni avantazhet, llojet dhe koston e dyerve dhe dritareve prej duralumini.",
                    Category = "Ndërtimtari",
                    CategoryIcon = "bi-door-open",
                    PublishedDate = new DateTime(2026, 3, 3),
                    ReadTime = "4 min",
                    ImageIcon = "bi-grid-1x2",
                    Tags = new List<string> { "duralumini", "dyer", "dritare", "izolim", "ndërtim" },
                    Content = @"
<h2>Pse Duralumini Është Zgjedhja Numër Një?</h2>
<p>Dritaret dhe dyert e <strong>duraluminit</strong> janë bërë standardi i ndërtimtarisë moderne në Shqipëri. Me kombinimin e bukurisë, qëndrueshmërisë dhe izolimit termik, duralumini ofron avantazhe që materialet tradicionale nuk mund t'i arrijnë.</p>

<h3>Avantazhet Kryesore të Duraluminit</h3>
<ul>
    <li><strong>Izolim termik i shkëlqyer</strong> — Profili me thyerje termike ul humbjet e nxehtësisë deri 60%</li>
    <li><strong>Izolim akustik</strong> — Xhami dyshtresor ose treshtresor ul zhurmën deri 42 dB</li>
    <li><strong>Qëndrueshmëri</strong> — Nuk ndryshkon, nuk gërryet, zgjat 30-50+ vjet</li>
    <li><strong>Mirëmbajtje minimale</strong> — Pastrohet lehtësisht, nuk kërkon lyerje</li>
    <li><strong>Dizajn modern</strong> — Mundësi të pakufizuara ngjyrash dhe formash</li>
    <li><strong>Siguri</strong> — Profil i fortë me sisteme shumëpikëshe kyçjeje</li>
</ul>

<h3>Llojet e Dritareve dhe Dyerve</h3>
<ul>
    <li><strong>Dritare me hapje anësore</strong> — Klasike dhe praktike</li>
    <li><strong>Dritare me hapje rrëshqitëse</strong> — Ideale për ballkone dhe veranda</li>
    <li><strong>Dritare me hapje oscilobatante</strong> — Hapje nga lart për ajrosje të sigurt</li>
    <li><strong>Dyer rrëshqitëse (sliding)</strong> — Perfekte për tarracat dhe kopshtet</li>
    <li><strong>Dyer harmonikë (folding)</strong> — Hapje e gjerë, elegancë maksimale</li>
</ul>

<h3>Sa Kushton Duralumini?</h3>
<p>Çmimet variojnë sipas cilësisë dhe madhësisë:</p>
<ul>
    <li><strong>Dritare standarde (120x120 cm):</strong> 80-200 EUR</li>
    <li><strong>Derë ballkoni rrëshqitëse:</strong> 300-800 EUR</li>
    <li><strong>Shtëpi e plotë (10-15 njësi):</strong> 3,000-8,000 EUR</li>
</ul>
<p>Investimi kthehet përmes kursimeve në energji brenda 3-5 vjetësh.</p>

<h2>Gjeni Instaluesin e Duhur me TeknoSOS</h2>
<p>Cilësia e instalimit është po aq e rëndësishme sa cilësia e materialit. Një <strong>specialist duralumini i TeknoSOS</strong> mat saktësisht hapësirat, prodhon dhe instalon me precizion, duke garantuar izolim perfekt.</p>

<div class='blog-cta'>
    <p><strong>Doni dritare të reja?</strong> <a href='/Technicians'>Gjeni specialistin e duraluminit në TeknoSOS</a></p>
</div>"
                },

                // Article 13 — Arkitektët
                new BlogArticle
                {
                    Slug = "arkitekti-pse-keni-nevoje-per-arkitekt",
                    Title = "Pse Keni Nevojë për Arkitekt: Roli Thelbësor në Çdo Projekt Ndërtimi",
                    MetaDescription = "Zbuloni pse arkitekti është i domosdoshëm për projektin tuaj — nga projektimi i shtëpisë deri te lejet e ndërtimit. Si të gjeni arkitektin e duhur në Shqipëri.",
                    Excerpt = "Doni të ndërtoni ose rinovoni? Arkitekti nuk është luks — është investimi më i mençur. Mësoni rolin e arkitektit dhe si t'ju ndihmojë.",
                    Category = "Projektim",
                    CategoryIcon = "bi-pencil-square",
                    PublishedDate = new DateTime(2026, 3, 2),
                    ReadTime = "5 min",
                    ImageIcon = "bi-building-gear",
                    Tags = new List<string> { "arkitekt", "projektim", "ndërtim", "leje ndërtimi", "dizajn" },
                    Content = @"
<h2>Çfarë Bën Saktësisht një Arkitekt?</h2>
<p>Arkitekti nuk është thjesht dikush që vizaton plane — është profesionisti që transformon nevojat dhe ëndrrat tuaja në hapësira funksionale, të bukura dhe të sigurta. Nga <strong>projektimi fillestar</strong> deri te <strong>mbikëqyrja e ndërtimit</strong>, arkitekti është partneri juaj i besuar gjatë gjithë procesit.</p>

<h3>Shërbimet Kryesore të Arkitektit</h3>
<ul>
    <li><strong>Projektim arkitektonik</strong> — Planimetri, fasada, prerje, detaje konstruktive</li>
    <li><strong>Dizajn interior</strong> — Organizimi i hapësirës së brendshme dhe estetika</li>
    <li><strong>Leje ndërtimi</strong> — Përgatitja e dosjes teknike për bashkinë</li>
    <li><strong>Studim fizibiliteti</strong> — Analiza e mundësisë së projektit në truallin tuaj</li>
    <li><strong>Mbikëqyrje ndërtimi</strong> — Kontrolli që puna bëhet sipas projektit</li>
    <li><strong>Vlerësim energjitik</strong> — Projektim për efikasitet maksimal energjitik</li>
</ul>

<h3>Kur Keni Nevojë për Arkitekt?</h3>
<p>Arkitekti është i nevojshëm kur:</p>
<ul>
    <li>Ndërtoni shtëpi ose ndërtesë të re</li>
    <li>Bëni rinovim ose zgjerim të madh</li>
    <li>Ndryshoni destinacionin e një objekti</li>
    <li>Keni nevojë për leje ndërtimi nga bashkia</li>
    <li>Doni optimizim të hapësirës ekzistuese</li>
</ul>

<h3>Si të Zgjidhni Arkitektin e Duhur?</h3>
<p>Kur zgjidhni arkitektin, merrni parasysh:</p>
<ul>
    <li><strong>Portofolio</strong> — Shikoni projektet e mëparshme. A përputhen me stilin tuaj?</li>
    <li><strong>Licenca</strong> — Sigurohuni që ka licencë aktive nga Urdhri i Arkitektit</li>
    <li><strong>Komunikim</strong> — A e kupton vizionin tuaj? A është i disponueshëm?</li>
    <li><strong>Çmimi</strong> — Krahasoni ofertat. Zakonisht 3-8% e kostos së ndërtimit</li>
</ul>

<h2>Gjeni Arkitektin Tuaj në TeknoSOS</h2>
<p>Me <strong>TeknoSOS</strong>, gjeni arkitektë të licencuar me përvojë në projekte të ndryshme — nga shtëpi individuale te komplekse banimi. Shikoni portofoliot, lexoni vlerësimet dhe filloni projektin tuaj sot.</p>

<div class='blog-cta'>
    <p><strong>Gati të filloni projektin?</strong> <a href='/Technicians'>Gjeni arkitektin ideal në TeknoSOS</a></p>
</div>"
                },

                // Article 14 — Inxhinierët
                new BlogArticle
                {
                    Slug = "inxhinieri-ndertimit-roli-ne-projekt",
                    Title = "Inxhinieri i Ndërtimit: Roli i Tij në Sigurinë dhe Cilësinë e Projektit",
                    MetaDescription = "Mësoni pse inxhinieri i ndërtimit është thelbësor për çdo projekt — nga llogaritjet strukturore te mbikëqyrja e punimeve. Gjeni inxhinierin e duhur në Shqipëri.",
                    Excerpt = "Inxhinieri siguron që ndërtesa juaj të jetë e sigurt, e qëndrueshme dhe në përputhje me standardet. Ja çfarë bën dhe pse keni nevojë për të.",
                    Category = "Inxhinieri",
                    CategoryIcon = "bi-gear",
                    PublishedDate = new DateTime(2026, 2, 28),
                    ReadTime = "5 min",
                    ImageIcon = "bi-calculator",
                    Tags = new List<string> { "inxhinier", "ndërtim", "strukturë", "projekt", "siguri" },
                    Content = @"
<h2>Çfarë Është Inxhinieri i Ndërtimit?</h2>
<p><strong>Inxhinieri i ndërtimit</strong> (civil engineer) është profesionisti që siguron se një ndërtesë qëndron në këmbë — fjalë për fjalë. Ndërsa arkitekti projekton formën dhe funksionalitetin, inxhinieri llogarit forcat, materialet dhe dimensionet e strukurës që mbajnë gjithçka të sigurt.</p>

<h3>Specialitetet e Inxhinierisë në Ndërtim</h3>
<ul>
    <li><strong>Inxhinier strukturist</strong> — Projekton skeletin e ndërtesës (themele, kolona, trarë, soletë)</li>
    <li><strong>Inxhinier hidroteknik</strong> — Projekton sistemin e ujësjellësit dhe kanalizimeve</li>
    <li><strong>Inxhinier elektrik</strong> — Projekton instalimin elektrik dhe sistemet e fuqisë</li>
    <li><strong>Inxhinier gjeoteknik</strong> — Studion truallin para ndërtimit të themeleve</li>
    <li><strong>Inxhinier i mjedisit</strong> — Siguron përputhjen me standardet mjedisore</li>
</ul>

<h3>Kur Keni Nevojë për Inxhinier?</h3>
<p>Nuk mjafton vetëm arkitekti — inxhinieri është i domosdoshëm kur:</p>
<ul>
    <li>Ndërtoni çdo lloj strukture (shtëpi, ndërtesë, urë)</li>
    <li>Bëni shtesa ose modifikime strukturore</li>
    <li>Doni të kontrolloni gjendjen e ndërtesës ekzistuese</li>
    <li>Keni nevojë për <strong>raport ekspertize</strong> pas dëmtimeve (si tërmeti)</li>
    <li>Projektoni sisteme komplekse (hidraulike, elektrike, HVAC)</li>
</ul>

<h3>Inxhinieri dhe Siguria Sizmike në Shqipëri</h3>
<p>Shqipëria ndodhet në zonë sizmike aktive. Pas tërmetit të nëntorit 2019, rëndësia e <strong>inxhinierisë strukturore</strong> u kuptua thellë. Çdo ndërtesë e re duhet projektuar sipas Eurokodit 8 (EC8) për rezistencë ndaj tërmeteve. Inxhinieri strukturist garanton që ndërtesa juaj ti përballojë forcat sizmike.</p>

<h2>Gjeni Inxhinierin e Duhur me TeknoSOS</h2>
<p><strong>TeknoSOS</strong> ju lidhur me inxhinierë të licencuar dhe me përvojë në projekte të ndryshme ndërtimi. Nga llogaritjet strukturore te mbikëqyrja e punimeve — gjeni profesionistin e duhur online.</p>

<div class='blog-cta'>
    <p><strong>Keni nevojë për inxhinier?</strong> <a href='/Technicians'>Gjeni inxhinierin e duhur në TeknoSOS</a></p>
</div>"
                },

                // Article 15 — Real Estate
                new BlogArticle
                {
                    Slug = "tregu-imobiliar-ne-shqiperi-guida",
                    Title = "Tregu Imobiliar në Shqipëri: Gjithçka Që Duhet të Dini Para Se të Blini ose Shisni",
                    MetaDescription = "Guida e plotë për tregun imobiliar në Shqipëri — çmimet, zonat më të kërkuara, këshilla blerje-shitje dhe si të gjeni agjentin e duhur.",
                    Excerpt = "Doni të blini ose shisni pronë në Shqipëri? Mësoni tendencat e tregut, zonat më të mira dhe gabimet që duhet të shmangni.",
                    Category = "Real Estate",
                    CategoryIcon = "bi-house-heart",
                    PublishedDate = new DateTime(2026, 2, 25),
                    ReadTime = "5 min",
                    ImageIcon = "bi-houses",
                    Tags = new List<string> { "real estate", "pronë", "blerje", "shitje", "Shqipëri" },
                    Content = @"
<h2>Si Është Gjendja e Tregut Imobiliar në Shqipëri?</h2>
<p>Tregu imobiliar në Shqipëri ka njohur rritje të vazhdueshme, veçanërisht në <strong>Tiranë, Durrës, Vlorë dhe Sarandë</strong>. Kërkesa për apartamente të reja, vila pranë detit dhe prona komerciale vazhdon të rritet, e nxitur nga investimet e diasporës, turizmi dhe urbanizimi.</p>

<h3>Zonat Më të Kërkuara për Investim</h3>
<ul>
    <li><strong>Tiranë (Blloku, Kompleksi Dinamo, Liqeni)</strong> — Çmimet 1,200-2,500 EUR/m². Kërkesë e lartë për apartamente luksoze</li>
    <li><strong>Durrës (Plazhi, Currila)</strong> — Çmimet 600-1,200 EUR/m². Popullore për pushime dhe investim</li>
    <li><strong>Vlorë (Uji i Ftohtë, Radhimë)</strong> — Çmimet 800-1,800 EUR/m². Rritje e shpejtë</li>
    <li><strong>Sarandë</strong> — Çmimet 1,000-2,000 EUR/m². Fuqimisht turistike</li>
    <li><strong>Shkodër, Korçë, Berat</strong> — Çmimet 400-800 EUR/m². Potencial i pashfrytëzuar</li>
</ul>

<h3>Këshilla për Blerësit</h3>
<ul>
    <li><strong>Verifikoni pronësinë</strong> — Kontrolloni certifikatën e pronësisë në ASHK (Kadastrën)</li>
    <li><strong>Kontrolloni lejen e ndërtimit</strong> — Sigurohuni që ndërtesa ka leje të rregullt</li>
    <li><strong>Inspektoni gjendjen teknike</strong> — Kontrolloni instalimin elektrik, hidraulik dhe strukturën</li>
    <li><strong>Krahasoni çmimet</strong> — Studioni tregun para se të vendosni</li>
    <li><strong>Punoni me profesionistë</strong> — Agjent imobiliar, noter dhe avokat</li>
</ul>

<h3>Këshilla për Shitësit</h3>
<ul>
    <li><strong>Vlerësimi profesional</strong> — Merrni vlerësim nga një ekspert për çmimin e duhur</li>
    <li><strong>Rinovoni para shitjes</strong> — Investime të vogla në pamjen e pronës kthejnë shumë</li>
    <li><strong>Dokumentacioni</strong> — Përgatitni gjithë dokumentet para se ta listoni</li>
</ul>

<h2>TeknoSOS — Partneri Juaj Imobiliar</h2>
<p>Para se të blini ose shisni, sigurohuni që prona është në gjendje të mirë teknike. Me <strong>TeknoSOS</strong>, gjeni profesionistë për <strong>inspektim elektrik, hidraulik dhe strukturor</strong> — gjithçka që ju nevojitet para vendimit të madh.</p>

<div class='blog-cta'>
    <p><strong>Po blini pronë? Kontrollojeni parë!</strong> <a href='/Technicians'>Gjeni inspektorin teknik në TeknoSOS</a></p>
</div>"
                },

                // Article 16 — Vlerësuesit e Pasurisë
                new BlogArticle
                {
                    Slug = "vleresimi-pasurise-pse-eshte-i-rendesishem",
                    Title = "Vlerësimi i Pasurisë: Pse Është i Rëndësishëm dhe Si Bëhet",
                    MetaDescription = "Mësoni pse vlerësimi profesional i pasurisë është i domosdoshëm — për blerje, shitje, kredi bankare ose trashëgimi. Si funksionon në Shqipëri.",
                    Excerpt = "A e dini vlerën reale të pronës tuaj? Vlerësimi profesional ju mbron nga gabimet e kushtueshme. Ja si funksionon procesi.",
                    Category = "Vlerësim Prone",
                    CategoryIcon = "bi-graph-up-arrow",
                    PublishedDate = new DateTime(2026, 2, 22),
                    ReadTime = "4 min",
                    ImageIcon = "bi-clipboard-data",
                    Tags = new List<string> { "vlerësim", "pasuri", "pronë", "bankë", "ekspert" },
                    Content = @"
<h2>Çfarë Është Vlerësimi i Pasurisë?</h2>
<p><strong>Vlerësimi i pasurisë</strong> është procesi profesional i përcaktimit të vlerës së tregut për një pronë — banesë, tokë, ndërtesë komerciale ose çdo lloj pasurie të paluajtshme. Vlerësimi bëhet nga <strong>ekspertë të licencuar</strong> që analizojnë faktorë të shumtë për të ardhur në një çmim të drejtë dhe objektiv.</p>

<h3>Kur Keni Nevojë për Vlerësim?</h3>
<p>Vlerësimi profesional kërkohet në disa raste:</p>
<ul>
    <li><strong>Blerje me kredi bankare</strong> — Banka kërkon vlerësim zyrtarë para se të aprovojë kredinë hipotekore</li>
    <li><strong>Shitje prone</strong> — Për të vendosur çmimin e duhur të tregut</li>
    <li><strong>Trashëgimi</strong> — Për ndarjen e drejtë të pasurisë midis trashëgimtarëve</li>
    <li><strong>Sigurim</strong> — Për përcaktimin e vlerës së sigurimit</li>
    <li><strong>Kontabilitet biznesi</strong> — Vlerësimi i aseteve të paluajtshme të kompanisë</li>
    <li><strong>Procese gjyqësore</strong> — Si provë e vlerës në çështje pronësore</li>
</ul>

<h3>Si Bëhet Vlerësimi?</h3>
<p>Procesi i vlerësimit përfshin disa hapa:</p>
<ul>
    <li><strong>Inspektimi fizik</strong> — Vlerësuesi viziton pronën, mat sipërfaqen, kontrollon gjendjen</li>
    <li><strong>Analiza e tregut</strong> — Krahason me prona të ngjashme të shitura së fundmi</li>
    <li><strong>Metoda e vlerësimit</strong> — Përdor metodën krahasuese, metodën e të ardhurave ose metodën e kostos</li>
    <li><strong>Raporti i vlerësimit</strong> — Dokumenti zyrtarë me fotografi, analiza dhe përfundimin</li>
</ul>

<h3>Faktorët Që Ndikojnë Vlerën</h3>
<ul>
    <li><strong>Vendndodhja</strong> — Faktori numër një (zona, lagja, pamja)</li>
    <li><strong>Sipërfaqja</strong> — Metrat katrore të ndërtimit dhe truallit</li>
    <li><strong>Gjendja teknike</strong> — Instalimi elektrik, hidraulik, izolimi, materialet</li>
    <li><strong>Viti i ndërtimit</strong> — Ndërtesat e reja vlerësohen më lart</li>
    <li><strong>Dokumentacioni</strong> — Pronësia e rregullt, leje ndërtimi</li>
</ul>

<h2>Përgatituni me TeknoSOS</h2>
<p>Para vlerësimit, sigurohuni që prona të jetë në gjendjen më të mirë teknike. Me <strong>TeknoSOS</strong>, gjeni profesionistë për çdo riparim ose mirëmbajtje — nga elektrika te hidraulika — dhe rritni vlerën e pronës tuaj.</p>

<div class='blog-cta'>
    <p><strong>Rritni vlerën e pronës suaj!</strong> <a href='/Technicians'>Gjeni profesionistë në TeknoSOS</a> për mirëmbajtje para vlerësimit.</p>
</div>"
                },
            };
        }

        public static BlogArticle? GetBySlug(string slug)
        {
            return GetAllArticles().FirstOrDefault(a => a.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        }
    }
}
