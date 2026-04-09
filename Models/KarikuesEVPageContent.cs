using System.Collections.Generic;

namespace TeknoSOS.WebApp.Models
{
    public class KarikuesEVPageContent
    {
        public int Id { get; set; }
        // Hero Section
        public string HeroTitle { get; set; } = string.Empty;
        public string HeroLead { get; set; } = string.Empty;
        public string HeroBadgesJson { get; set; } = string.Empty; // JSON array for car badges
        // Services Section
        public string ServicesTitle { get; set; } = string.Empty;
        public string ServicesDescription { get; set; } = string.Empty;
        public string ServicesJson { get; set; } = string.Empty; // JSON array for service cards
        // Pricing Section
        public string PricingJson { get; set; } = string.Empty; // JSON array for pricing cards
        // Process Section
        public string ProcessTitle { get; set; } = string.Empty;
        public string ProcessDescription { get; set; } = string.Empty;
        public string ProcessStepsJson { get; set; } = string.Empty; // JSON array for steps
        // Why Us Section
        public string WhyUsTitle { get; set; } = string.Empty;
        public string WhyUsJson { get; set; } = string.Empty; // JSON array for why us cards
        // Brands Section
        public string BrandsJson { get; set; } = string.Empty; // JSON array for brands
        // FAQ Section
        public string FaqTitle { get; set; } = string.Empty;
        public string FaqJson { get; set; } = string.Empty; // JSON array for FAQ items
        // Final CTA
        public string FinalCtaTitle { get; set; } = string.Empty;
        public string FinalCtaDescription { get; set; } = string.Empty;
        public string FinalCtaPhone { get; set; } = string.Empty;
    }
}