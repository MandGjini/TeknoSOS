# TeknoSOS - SEO Strategy & Implementation Guide

**Qëllim**: Renditet #1 në Google për fjalëkërcime teknike në Shqipëri (defekt elektrik, hidraulik, teknik, etj.)

---

## 📋 PËRMBAJTJE

1. [Strategjia e Fjalëkërcimeve](#1-strategjia-e-fjalekercimeve)
2. [Optimizimi On-Page](#2-optimizimi-on-page)
3. [Optimizimi Teknik](#3-optimizimi-teknik)
4. [Local SEO för Shqipëri](#4-local-seo)
5. [Backlink Strategy](#5-backlink-strategy)
6. [Content Marketing](#6-content-marketing)
7. [Monitoring & Analytics](#7-monitoring--analytics)

---

## 1. STRATEGJIA E FJALËKËRCIMEVE

### 1.1 Primary Keywords (Kampione)
```
KATEGORIA ELEKTRIKE:
- defekt elektrik shqipëri
- elektricist tiranë
- riparimi i elektrikës
- probleme rrjeti elektrik
- karikues makina elektrike
- elektricistë në shqipëri

KATEGORIA HIDRAULIKE:
- defekt ujit shqipëri
- hidraulik tiranë
- reparim tubash
- probleme sanitare
- reparim radiatori
- teknikë hidraulike

KATEGORIA PËRGJITHSHME:
- shërbim teknik shqipëri
- teknik shtëpie tiranë
- reparim të shpejtë
- teknisan profesionist
- platforma teknikë shqipëri
- defektimi urgjenco
```

### 1.2 Long-Tail Keywords
```
- "defekt elektrik mbrëmje në tiranë"
- "ku të gjej elektricistin e mirë në durrës"
- "sa kushton riparimi i menjëhershëm"
- "elektricistë në lagjen e madhe"
- "hidraulik i mirë në vlorë"
- "teknik për pajisje shtëpie"
```

### 1.3 Geographic Keywords
```
- defekt [città]: elektrik tiranë, durrës, vlorë, shkodër, fier, elbasan
- teknik [region]: teknik në jug, teknik në veri
- zona: [lagje], [rrugë], [qark]
```

---

## 2. OPTIMIZIMI ON-PAGE

### 2.1 Meta Tags Strategy
```html
<!-- HOME PAGE -->
<title>TeknoSOS - Defekt Elektrik, Hidraulik, Teknik në Shqipëri</title>
<meta name="description" content="Gjeni elektricistë, teknikë dhe hidraulike profesionistë në Shqipëri. Përgjigje brenda 15 minutash, çmime transparente, vlerësime reale. Shërbim 24/7.">

<!-- CATEGORY PAGES -->
<title>Defekt Elektrik në Shqipëri - Elektricistë Profesionistë | TeknoSOS</title>
<description>Elektricistë të licencuar në Tiranë, Durrës, Vlorë. Riparim i menjëhershëm i defekteve elektrike, instalime, ndricim. Koha e përgjigjes 15 min.

<!-- CITY PAGES -->
<title>Elektricistë në Tiranë - Defekt Elektrik 24/7 | TeknoSOS</title>
<description>Gjeni elektricistin më të mirë në Tiranë. Defekt elektrik urgjent, riparim paneli, instalim ndriçimi. Vlerësime kliente reale.
```

### 2.2 Heading Structure (H1-H6)

```html
<h1>Defekt Elektrik në Tiranë - Elektricistë Profesionistë 24/7</h1>

<h2>Pse Të Zgjidhni TeknoSOS?</h2>
  <h3>Elektricistë të Verifikuar</h3>
  <h3>Përgjigje të Shpejta</h3>
  <h3>Çmime Transparente</h3>

<h2>Llojet e Defekteve Elektrike</h2>
  <h3>Qark i Shkurtër</h3>
  <h3>Probleme Me Automatët</h3>
  <h3>Defekt Prizash</h3>

<h2>Elektricistët Më të Mirë në Tiranë</h2>
<h2>Si Punon Platforma</h2>
<h2>Pyetje të Shpeshta</h2>
```

### 2.3 URL Structure
```
✅ MIRË:
/defekt-elektrik/
/defekt-elektrik-tirane/
/elektricistë-durrës/
/hidraulik-shkoder/
/kategori/kategoria-emri/

❌ KEQË:
/defect.php?id=123
/category_elektrik
/page?name=xxx
```

### 2.4 Internal Linking Strategy
```
Home → Kategoria → Qyteti → Teknikë
Home → FAQ → Pyetja specifike → Kategoria

Shembull:
- Home page → "Defekt Elektrik" → "Defekt Elektrik në Tiranë" → Profili i elektricistit
```

### 2.5 Schema Markup (Structured Data)

```html
<!-- ORGANIZATION SCHEMA -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Organization",
  "name": "TeknoSOS",
  "url": "https://teknosos.app",
  "logo": "https://teknosos.app/images/logo.png",
  "sameAs": ["https://facebook.com/teknosos", "https://instagram.com/teknosos"],
  "areaServed": "AL",
  "address": {
    "@type": "PostalAddress",
    "addressCountry": "AL",
    "addressLocality": "Tiranë",
    "postalCode": "1000"
  }
}
</script>

<!-- SERVICE SCHEMA -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Service",
  "name": "Defekt Elektrik",
  "provider": {"@type": "Organization", "name": "TeknoSOS"},
  "areaServed": ["Tiranë", "Durrës", "Vlorë"],
  "serviceType": "Electrical Repair",
  "image": "https://teknosos.app/images/electrical-service.jpg"
}
</script>

<!-- LOCALSERVICE SCHEMA -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "LocalService",
  "name": "TeknoSOS Defekt Elektrik",
  "areaServed": "AL",
  "availableLanguage": ["sq"],
  "image": "https://teknosos.app/images/service.jpg",
  "telephone": "+355693446516"
}
</script>

<!-- AGGREGATE RATING SCHEMA -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "LocalBusiness",
  "name": "TeknoSOS",
  "aggregateRating": {
    "@type": "AggregateRating",
    "ratingValue": "4.8",
    "reviewCount": "2000"
  }
}
</script>
```

---

## 3. OPTIMIZIMI TEKNIK

### 3.1 Performance Optimization
```
✅ KRITIK:
- Page Speed < 2 sekonda (target 1.5s)
- Mobile First Design (100% responsive)
- Gzip/Brotli compression
- Image optimization (WebP format)
- CDN for static assets
- Cache strategy:
  * Static: 30 days
  * Dynamic: 5 min
  * API: no cache

Toolet:
- Google PageSpeed Insights: target 90+
- GTmetrix: < 2s
- Lighthouse: 90+
```

### 3.2 Mobile Optimization
```
- Viewport meta tag: ✅ (në _Layout.cshtml)
- Touch-friendly buttons: min 44x44px
- Mobile-first CSS approach
- No horizontal scroll
- Fast tap response
```

### 3.3 Sitemap & Robots

**robots.txt:**
```
User-agent: *
Allow: /
Disallow: /admin/
Disallow: /api/
Crawl-delay: 1

Sitemap: https://teknosos.app/sitemap.xml
```

**sitemap.xml:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <url>
    <loc>https://teknosos.app/</loc>
    <lastmod>2026-03-14</lastmod>
    <changefreq>daily</changefreq>
    <priority>1.0</priority>
  </url>
  <url>
    <loc>https://teknosos.app/defekt-elektrik/</loc>
    <lastmod>2026-03-14</lastmod>
    <changefreq>weekly</changefreq>
    <priority>0.9</priority>
  </url>
  <url>
    <loc>https://teknosos.app/defekt-elektrik-tirane/</loc>
    <lastmod>2026-03-14</lastmod>
    <changefreq>weekly</changefreq>
    <priority>0.8</priority>
  </url>
</urlset>
```

### 3.4 Core Web Vitals
```
LCP (Largest Contentful Paint): < 2.5s ✅
FID (First Input Delay): < 100ms ✅
CLS (Cumulative Layout Shift): < 0.1 ✅

Monitorim: Google Search Console → Core Web Vitals
```

---

## 4. LOCAL SEO

### 4.1 Google Business Profile
```
✅ AKTIV:
- Google Business Profile për TeknoSOS
- Kategoria: "Service"
- Zona: Shqipëri (all cities)
- Telefon: +355 69 344 6516
- Adresa: Tiranë, AL
- Orari: 24/7
- Foto: 10+ foto cilësie të lartë
- Videa: 3-5 videa shërbimesh
- Faqe rrjetesh sociale të lidhur
```

### 4.2 Local Citations
```
Nënshrirje në:
1. Albanian Business Directory
2. Local.Al
3. Shqiperia.Al
4. Pinarusti
5. Info4Business
6. Rrugica.Al
7. GoDaddy Local

Të gjitha me:
- NAP të njëjtë: TeknoSOS, Tiranë, Shqipëri
- Kategoria: Service, Electrical, Repair
- Linkje drejt homepage: https://teknosos.app
```

### 4.3 Local Keywords Targeting
```
Faqe për secilin qytet:
- /defekt-elektrik-tirane/
- /defekt-elektrik-durres/
- /defekt-elektrik-vlore/
- /elektricistë-shkoder/
- /elektricistë-fier/
- /elektricistë-elbasan/

Secilit me:
- Title me emrin e qytetit
- Meta description me qytetit
- H1 me "në [qyteti]"
- Lokale e rrezja 50-100km
- Numrat telefonik lokal
```

---

## 5. BACKLINK STRATEGY

### 5.1 High-Authority Link Building

**Premium Directories (Priority):**
```
1. Alo.Al (Albanian Yellow Pages)
   - Category: Services → Electrical
   - Link: teknosos.app

2. Biznesi.Org (Business Directory)
   - Name: TeknoSOS
   - Description: Platform for technical services
   
3. InfoBusiness.AL
   - Premium listing
   - Verified business

4. Pinarusti.Com
   - Business network
   - Local authority
```

**Government & Authority Sites:**
```
1. INSTAT.GOV.AL (Albanian Statistics)
   - Business registration mention
   - Data: 500+ technicians registered

2. Chamber of Commerce
   - Member listing (if applicable)
   - Authority boost

3. AKKIE (Consumer Protection)
   - Verified business
   - Trust signal
```

**Relevant Industry Sites:**
```
1. ConstructionAL.Com
   - Guest post: "How to Find the Right Technician"
   - Link: https://teknosos.app

2. RenovationDiaries.AL
   - Blog post: "Home Repair Services in Albania"
   
3. TechServeAL.Com
   - Industry news coverage
   - Press release publication
```

### 5.2 Local Partnership Backlinks

```
1. Real Estate Agencies
   - Realtors linking to TeknoSOS
   - Anchor: "home repair services"

2. Insurance Companies
   - Claims assistance links
   - "get technician quickly"

3. Property Management Companies
   - Service recommendations
   - Internal linking

4. Hotels/Tourism Sites
   - Emergency repair services
   - Maintenance links
```

### 5.3 Content-Based Backlink Generation

**Strategy:**
```
1. Create high-value content (Blog posts, Guides)
   - "How to Fix Common Electrical Problems"
   - "Home Maintenance Guide 2026"
   - "Finding Trusted Technicians in Albania"

2. Outreach to relevant sites
   - Tech publications
   - Albanian news sites
   - Home improvement blogs
   - Forum discussions

3. Anchor text diversity:
   ✅ "defekt elektrik shqipëri"
   ✅ "teknosas.app"
   ✅ "find technician albania"
   ✅ "home repair platform"
   ✅ "click here"
```

### 5.4 Social Signals (Not Direct Ranking Factor, but Traffic Driver)

```
Social Media Strategy:
- Facebook: Daily posts + community engagement
- Instagram: Photos of work + stories
- TikTok: Quick repair tips (younger audience)
- LinkedIn: Professional content

Content topics:
- Safety tips
- Maintenance reminders
- Customer testimonials
- Team highlights
- Industry news
```

### 5.5 Backlink Monitoring

```
Tools:
1. Ahrefs (best)
2. SEMrush
3. Moz
4. Google Search Console

Monitor:
- New backlinks (daily)
- Lost backlinks (weekly)
- Referring domain count
- Top referring pages
- Anchor text distribution
```

---

## 6. CONTENT MARKETING

### 6.1 Pillar Pages (Main Content Hubs)

```
1. /defekt-elektrik/ — "Everything About Electrical Problems"
   - Internal links to sub-pages
   - 3000+ words
   - Comprehensive guide

2. /defekt-ujit/ — "Water & Plumbing Issues"
   - Detailed troubleshooting
   - Local references

3. /si-funksionon/ — "How TeknoSOS Works"
   - Process explanation
   - Video tutorial
   - FAQ section

4. /pyetje-te-shpeshta/ — "Frequently Asked Questions"
   - 50+ Q&A
   - Long-form answers
   - Related links
```

### 6.2 Blog Content Strategy

**Post Frequency:** 2-3 posts/week

**Content Calendar:**
```
JANUAR:
- "New Year Home Checklist"
- "Winter Electrical Safety"
- "Common Heating Problems"

SHKURT:
- "Valentine's Day Home Projects"
- "Spring Preparation Guide"

MARS-ZEZË (WARM MONTHS):
- "Summer AC Maintenance"
- "Outdoor Electrical Safety"

TETOR-DHJETOR (COLD):
- "Winter Preparation"
- "Heating System Check"
```

**Content Types:**
```
1. How-to Guides (500-800 words)
   - SEO-optimized steps
   - Images/video
   - Links to service pages

2. Problem Diagnosis (600-1000 words)
   - Symptoms → Solution
   - When to call professional
   - Safety warnings

3. Local Guides (1000+ words)
   - "Electrical Services in Tirana"
   - "Best Hydro Technicians in Durres"
   - Regional recommendations

4. Industry News (300-500 words)
   - Commentary
   - Educational angle
   - Link to relevant service

5. Customer Testimonials (300-400 words)
   - Real stories
   - Before/After
   - Video testimonials
```

### 6.3 Video Content (YouTube Channel)

```
Video Strategy:
- Upload 1-2 videos/month
- Target: "how to", "what if", emergency situations
- Include channel links in all

Popular Topics:
1. "How to Fix a Tripped Breaker"
2. "Electrical Safety Tips"
3. "Water Leak Emergency Guide"
4. "How to Find the Right Technician"
5. "Common Home Problems - Quick Fixes"
6. "Interview: Professional Technician Tips"

Optimization:
- Title with keywords
- Description with links
- Tags: category, city, problem
- Playlist: By category
- Embed on website
```

---

## 7. MONITORING & ANALYTICS

### 7.1 Key Metrics to Track

```
VISIBILITY METRICS:
- Organic traffic (% change month/month)
- Keyword rankings (top 100 keywords)
- Search impressions (Google Search Console)
- Search CTR (Click-through rate)
- Average position in SERPs

ENGAGEMENT METRICS:
- Bounce rate target: < 50%
- Avg. session duration target: > 2 min
- Pages per session target: > 2.5
- Conversion rate: requests submitted

TECHNICAL METRICS:
- Page speed (average)
- Mobile usability score
- Core Web Vitals status
- Crawl errors

BACKLINK METRICS:
- Total backlinks
- Referring domains
- Link velocity (new links/week)
- Top referring pages
- Anchor text distribution
```

### 7.2 Tools Setup

```
1. Google Search Console
   - Performance tracking
   - Keyword analysis
   - Coverage issues
   - Manual actions

2. Google Analytics 4
   - Traffic sources
   - User behavior
   - Conversions
   - Retention

3. SEMrush or Ahrefs
   - Competitor analysis
   - Rank tracking
   - Backlink monitoring
   - Technical audit

4. Screaming Frog
   - Website crawling
   - Duplicate content check
   - Broken links

5. Lighthouse CI
   - Automated performance testing
```

### 7.3 Reporting Schedule

```
DAILY:
- Check ranking for top 10 keywords
- Monitor Google Search Console

WEEKLY:
- Organic traffic review
- New backlinks check
- Content performance

MONTHLY:
- Full performance report
- Competitor comparison
- Strategy adjustment
- Executive summary
```

---

## 8. IMPLEMENTATION ROADMAP

### Phase 1: Foundation (Week 1-2)
- [ ] Install robots.txt
- [ ] Create sitemap.xml
- [ ] Add schema markup to pages
- [ ] Optimize meta descriptions
- [ ] Setup Google Search Console
- [ ] Setup Google Analytics 4
- [ ] Setup Google Business Profile

### Phase 2: On-Page (Week 3-4)
- [ ] Optimize all category pages
- [ ] Create city-specific pages
- [ ] Internal linking structure
- [ ] Add FAQ schema
- [ ] Image optimization
- [ ] SSL certificate (HTTPS) ✅ (Already done)

### Phase 3: Content (Week 5-8)
- [ ] Create 5 pillar pages
- [ ] Publish 8 blog posts
- [ ] Create video content
- [ ] FAQ page expansion

### Phase 4: Backlinks (Week 9-16)
- [ ] Directory submissions (5-10)
- [ ] Local citations (10+)
- [ ] Partnership outreach (10)
- [ ] Press release submission
- [ ] Guest blogging (3-5)

### Phase 5: Ongoing
- [ ] Weekly blog posts
- [ ] Monthly backlink targets
- [ ] Quarterly strategy reviews
- [ ] Continuous optimization

---

## 9. QUICK ACTION ITEMS (THIS WEEK)

```
HIGH PRIORITY:
1. ✅ Add robots.txt (Immediate)
2. ✅ Create sitemap.xml (Immediate)
3. Meta descriptions audit (Today)
4. Schema markup check (Tomorrow)
5. Google Search Console setup (verify ownership)
6. Create first pillar page on defect-elektrik

MEDIUM PRIORITY:
7. Create 3 city-specific pages
8. Optimize images (WebP)
9. Setup Google Business Profile
10. Create backlink prospect list
```

---

## 10. KOMPETITOR ANALIZA

### Këto website-a janë sfiduese në Shqipëri:
```
1. alo.al (Yellow Pages - huge authority)
2. biznesi.org
3. Local Facebook pages (informal)
4. Personal networks (traditional way)

Strateg vs. ato:
- Targeted local content
- Better UX + site speed
- Professional reviews system
- Mobile-first design
- Active social media
```

---

## 📊 EXPECTED RESULTS

```
TIMELINE:
3 months:
- Increased impressions in Google (2-3x)
- Top 20 rankings for main keywords
- 20-30% traffic increase

6 months:
- Top 3 rankings for 10+ keywords
- 100%+ traffic increase
- Multiple #1 rankings

12 months:
- Dominant rankings in target categories
- 300%+ traffic increase
- Brand authority established
```

---

**Përaxo:** Këtij planit me disiplinë. SEO nuk është overnight - duhen 3-6 muaj për rezultate të dukshme. Konsistenca është çelësi!

**Kontakt për pyetje SEO:** Punoni me SEO specialist nëse keni buxhet.
