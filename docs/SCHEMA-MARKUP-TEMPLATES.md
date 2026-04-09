<!-- ===================================================================
    SCHEMA MARKUP TEMPLATES FOR SEO
    Insert these in appropriate views
    =================================================================== -->

<!-- 1. ORGANIZATION SCHEMA (Homepage / Footer) -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Organization",
  "name": "TeknoSOS",
  "url": "https://teknosos.app",
  "logo": "https://teknosos.app/images/logo.png",
  "sameAs": [
    "https://www.facebook.com/teknosos",
    "https://www.instagram.com/teknosos",
    "https://www.linkedin.com/company/teknosos"
  ],
  "contactPoint": {
    "@type": "ContactPoint",
    "contactType": "Customer Service",
    "telephone": "+355-69-344-6516",
    "email": "support@teknosos.app"
  },
  "areaServed": "AL",
  "address": {
    "@type": "PostalAddress",
    "streetAddress": "Rruga e Kavajës",
    "addressLocality": "Tiranë",
    "addressRegion": "Durrës",
    "postalCode": "1000",
    "addressCountry": "AL"
  }
}
</script>

<!-- ============================================================= -->
<!-- 2. LOCAL BUSINESS SCHEMA (For local SEO) -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "LocalBusiness",
  "name": "TeknoSOS",
  "image": "https://teknosos.app/images/logo.png",
  "description": "Platforma #1 në Shqipëri për shërbime teknike, elektrike, hidraulike dhe rreparime të shtëpisë",
  "telephone": "+355-69-344-6516",
  "email": "support@teknosos.app",
  "url": "https://teknosos.app",
  "address": {
    "@type": "PostalAddress",
    "streetAddress": "Tiranë",
    "addressLocality": "Tiranë",
    "addressCountry": "AL"
  },
  "openingHoursSpecification": [
    {
      "@type": "OpeningHoursSpecification",
      "dayOfWeek": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
      "opens": "00:00",
      "closes": "23:59"
    }
  ],
  "aggregateRating": {
    "@type": "AggregateRating",
    "ratingValue": "4.8",
    "reviewCount": "2000"
  },
  "priceRange": "$$"
}
</script>

<!-- ============================================================= -->
<!-- 3. SERVICE SCHEMA (For service category pages) -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Service",
  "name": "Defekt Elektrik",
  "description": "Shërbim profesional për zgjidhjen e defekteve elektrike në shtëpi dhe biznese",
  "provider": {
    "@type": "Organization",
    "name": "TeknoSOS",
    "url": "https://teknosos.app"
  },
  "areaServed": [
    {
      "@type": "Place",
      "name": "Tiranë"
    },
    {
      "@type": "Place",
      "name": "Durrës"
    },
    {
      "@type": "Place",
      "name": "Vlorë"
    },
    {
      "@type": "Place",
      "name": "Shkodër"
    }
  ],
  "availableLanguage": "sq",
  "serviceType": "Electrical Repair",
  "image": "https://teknosos.app/images/electrical-service.jpg",
  "priceRange": "$$"
}
</script>

<!-- ============================================================= -->
<!-- 4. FAQ SCHEMA (For FAQ pages) -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "FAQPage",
  "mainEntity": [
    {
      "@type": "Question",
      "name": "Si funksionon TeknoSOS?",
      "acceptedAnswer": {
        "@type": "Answer",
        "text": "1. Raportoni defektin, 2. Merrni oferta, 3. Zgjidhni teknikun, 4. Puna përfundohet"
      }
    },
    {
      "@type": "Question",
      "name": "Sa kohe pritet për një teknik?",
      "acceptedAnswer": {
        "@type": "Answer",
        "text": "Mesatarisht brenda 15 minutash merrni përgjigje nga profesionistët në zonen tuaj"
      }
    },
    {
      "@type": "Question",
      "name": "A janë teknikët e verifikuar?",
      "acceptedAnswer": {
        "@type": "Answer",
        "text": "Po, cdo profesionist kalon nje proces verifikimi përpara se të pranohej në platformë"
      }
    }
  ]
}
</script>

<!-- ============================================================= -->
<!-- 5. BREADCRUMB SCHEMA (For navigation) -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "BreadcrumbList",
  "itemListElement": [
    {
      "@type": "ListItem",
      "position": 1,
      "name": "Home",
      "item": "https://teknosos.app/"
    },
    {
      "@type": "ListItem",
      "position": 2,
      "name": "Defekt Elektrik",
      "item": "https://teknosos.app/defekt-elektrik/"
    },
    {
      "@type": "ListItem",
      "position": 3,
      "name": "Defekt Elektrik në Tiranë",
      "item": "https://teknosos.app/defekt-elektrik-tirane/"
    }
  ]
}
</script>

<!-- ============================================================= -->
<!-- 6. RATING/REVIEW SCHEMA (For professional profiles) -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "AggregateRating",
  "ratingValue": "4.8",
  "bestRating": "5",
  "worstRating": "1",
  "ratingCount": "127",
  "reviewCount": "127"
}
</script>

<!-- ============================================================= -->
<!-- 7. PERSON SCHEMA (For professional profiles) -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "Person",
  "name": "[Professional Name]",
  "jobTitle": "Elektricist",
  "image": "[Photo URL]",
  "description": "[Bio]",
  "sameAs": [
    "[Facebook URL]",
    "[Instagram URL]"
  ],
  "address": {
    "@type": "PostalAddress",
    "addressLocality": "Tiranë",
    "addressCountry": "AL"
  },
  "aggregateRating": {
    "@type": "AggregateRating",
    "ratingValue": "4.7",
    "reviewCount": "45"
  }
}
</script>

<!-- ============================================================= -->
<!-- 8. LOCALBUSINESS FOR CITIES -->
<!-- Use this for /defekt-elektrik-[city]/ pages -->
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "LocalBusiness",
  "name": "TeknoSOS - Defekt Elektrik në [City]",
  "image": "https://teknosos.app/images/logo.png",
  "description": "Elektricistë profesionistë në [City]. Defekt elektrik 24/7, koha e përgjigjes 15 minutash",
  "telephone": "+355-69-344-6516",
  "url": "https://teknosos.app/defekt-elektrik-[city]/",
  "address": {
    "@type": "PostalAddress",
    "addressLocality": "[City]",
    "addressRegion": "[Region]",
    "addressCountry": "AL"
  },
  "geo": {
    "@type": "GeoCoordinates",
    "latitude": "[LAT]",
    "longitude": "[LONG]"
  }
}
</script>

<!-- ============================================================= -->
<!-- IMPLEMENTATION NOTES

1. HOMEPAGE: Use schemas 1, 2
2. SERVICE PAGES: Use schemas 3, 5
3. FAQ PAGE: Use schema 4
4. PROFESSIONAL PROFILE: Use schemas 6, 7
5. CITY PAGES: Use schema 8

REPLACE PLACEHOLDERS:
- [City] → Tiranë, Durrës, etc
- [LAT], [LONG] → Google Maps coordinates
- [Professional Name] → Professional's full name
- [Photo URL] → Link to professional photo
- [Bio] → Description text

TESTING:
- Use Google Rich Results Test: https://search.google.com/test/rich-results
- Use Structured Data Testing Tool: https://developers.google.com/web/tools/markup-helper

IMPORTANT: Each schema should be in its own <script type="application/ld+json"> tag
