using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Models
{
    public class SendMailViewModel
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;
    }
}
