using System.Text.RegularExpressions;

namespace TeknoSOS.WebApp.Services
{
    /// <summary>
    /// Service to detect and mask contact information in messages to prevent off-platform communication.
    /// This ensures all communication happens within the TeknoSOS ecosystem.
    /// </summary>
    public interface IContactMaskingService
    {
        /// <summary>
        /// Masks all contact information (phone numbers, emails, addresses) in the given text
        /// </summary>
        string MaskContactInfo(string text);

        /// <summary>
        /// Checks if the text contains any contact information
        /// </summary>
        bool ContainsContactInfo(string text);

        /// <summary>
        /// Gets a warning message explaining why contact sharing is not allowed
        /// </summary>
        string GetContactWarningMessage(string language = "sq");
    }

    public class ContactMaskingService : IContactMaskingService
    {
        // Timeout for regex operations to prevent ReDoS attacks
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
        
        private readonly ILogger<ContactMaskingService>? _logger;

        public ContactMaskingService(ILogger<ContactMaskingService>? logger = null)
        {
            _logger = logger;
        }

        // Phone number patterns (Albanian format: 06x xxx xxxx, +355, 00355, etc.)
        private static readonly Regex PhonePatterns = new Regex(
            @"(\+355|00355|0)?[\s.-]?(6[6-9]|4[2-8])[\s.-]?\d{1}[\s.-]?\d{3}[\s.-]?\d{3,4}|" +  // Albanian mobile/landline
            @"\b\d{3}[\s.-]?\d{3}[\s.-]?\d{4}\b|" +  // Generic 10-digit
            @"\b\d{2,4}[\s.-]?\d{2,4}[\s.-]?\d{2,4}\b|" +  // Various formats
            @"\+\d{1,3}[\s.-]?\d{6,14}|" +  // International format
            @"\b0\d{2,3}[\s.-]?\d{6,8}\b",  // Landline format
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);

        // Email patterns
        private static readonly Regex EmailPattern = new Regex(
            @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}|" +
            @"[a-zA-Z0-9._%+-]+\s*[@＠]\s*[a-zA-Z0-9.-]+\s*[.．]\s*[a-zA-Z]{2,}|" +  // Obfuscated with spaces
            @"[a-zA-Z0-9._%+-]+\s*\[at\]\s*[a-zA-Z0-9.-]+\s*\[dot\]\s*[a-zA-Z]{2,}",  // [at] [dot] format
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);

        // Social media handles and URLs
        private static readonly Regex SocialMediaPattern = new Regex(
            @"(?:facebook|fb|instagram|ig|twitter|whatsapp|viber|telegram|tiktok|linkedin)[\s.:/@]*[\w.-]+|" +
            @"(?:wa\.me|t\.me|m\.me)/[\w.-]+|" +
            @"@[\w.-]{3,}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);

        // Website URLs
        private static readonly Regex UrlPattern = new Regex(
            @"(?:https?://)?(?:www\.)?[a-zA-Z0-9][a-zA-Z0-9-]*\.[a-zA-Z]{2,}(?:/\S*)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);

        // Address patterns (street names, building numbers)
        private static readonly Regex AddressPattern = new Regex(
            @"(?:rruga|rr\.?|blv\.?|bulevardi|sheshi|lagjja|lgj\.?)[\s]+[\w\s,.-]+(?:\d+)?|" +  // Albanian addresses
            @"\b(?:street|st\.?|avenue|ave\.?|road|rd\.?|boulevard|blvd\.?)[\s]+[\w\s,.-]+(?:\d+)?",  // English
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);

        // Number sequences that look like phone attempts (6+ consecutive digits) - simplified pattern
        private static readonly Regex SuspiciousNumbers = new Regex(
            @"\d{6,}",
            RegexOptions.Compiled, RegexTimeout);

        // Common obfuscation attempts (zero as o, spaces between digits, etc.)
        private static readonly Regex ObfuscatedPhone = new Regex(
            @"[0oO]\s*[6-9]\s*[6-9]\s*\d{6,}|" +
            @"zero\s*six|" +
            @"zer[oO0]\s+\d|" +
            @"gjashtë|" +
            @"shtatë|" +
            @"tetë|" +
            @"nëntë",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);

        private const string MaskedText = "[***]";
        private const string MaskedPhone = "[telefon i fshehur]";
        private const string MaskedEmail = "[email i fshehur]";
        private const string MaskedSocial = "[kontakt i fshehur]";
        private const string MaskedUrl = "[link i fshehur]";
        private const string MaskedAddress = "[adresë e fshehur]";

        public string MaskContactInfo(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            try
            {
                var result = text;

                // Mask in order of specificity (most specific first)
                result = EmailPattern.Replace(result, MaskedEmail);
                result = SocialMediaPattern.Replace(result, MaskedSocial);
                result = UrlPattern.Replace(result, MaskedUrl);
                result = PhonePatterns.Replace(result, MaskedPhone);
                result = ObfuscatedPhone.Replace(result, MaskedPhone);
                result = AddressPattern.Replace(result, MaskedAddress);
                
                // Check for suspicious number sequences (potential phone attempts)
                result = SuspiciousNumbers.Replace(result, match =>
                {
                    // Only mask if it looks like a phone number (7+ digits)
                    var digitsOnly = new string(match.Value.Where(char.IsDigit).ToArray());
                    return digitsOnly.Length >= 7 ? MaskedPhone : match.Value;
                });

                return result;
            }
            catch (RegexMatchTimeoutException ex)
            {
                _logger?.LogWarning(ex, "Regex timeout while masking contact info - possible malicious input");
                return text; // Return original if timeout occurs
            }
        }

        public bool ContainsContactInfo(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            try
            {
                return EmailPattern.IsMatch(text) ||
                       PhonePatterns.IsMatch(text) ||
                       SocialMediaPattern.IsMatch(text) ||
                       UrlPattern.IsMatch(text) ||
                       AddressPattern.IsMatch(text) ||
                       ObfuscatedPhone.IsMatch(text) ||
                       HasSuspiciousNumberSequence(text);
            }
            catch (RegexMatchTimeoutException ex)
            {
                _logger?.LogWarning(ex, "Regex timeout while checking contact info");
                return false;
            }
        }

        private bool HasSuspiciousNumberSequence(string text)
        {
            try
            {
                var match = SuspiciousNumbers.Match(text);
                if (!match.Success) return false;
            
                var digitsOnly = new string(match.Value.Where(char.IsDigit).ToArray());
                return digitsOnly.Length >= 7;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public string GetContactWarningMessage(string language = "sq")
        {
            return language switch
            {
                "sq" => @"⚠️ Për sigurinë dhe cilësi shërbimi, të gjitha komunikimet duhet të bëhen brenda platformës TeknoSOS.

🔒 Pse nuk lejohet shkëmbimi i kontakteve:
• Mbrojtja e të dhënave personale
• Garanci pagese e sigurt përmes platformës
• Mundësi ankese dhe zgjidhje në rast problemesh
• Vlerësime të besueshme për teknikët dhe bizneset
• Ndjekje e historikut të punëve dhe komunikimit

Ju lutemi përdorni chat-in e brendshëm për të gjitha komunikimet.",

                "en" => @"⚠️ For security and service quality, all communications must be done within the TeknoSOS platform.

🔒 Why contact sharing is not allowed:
• Protection of personal data
• Secure payment guarantee through the platform
• Ability to file complaints and resolve issues
• Reliable ratings for technicians and businesses
• Tracking of work history and communications

Please use the internal chat for all communications.",

                _ => GetContactWarningMessage("sq")
            };
        }
    }
}
