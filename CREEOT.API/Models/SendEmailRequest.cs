using System.ComponentModel.DataAnnotations;

namespace CREEOT.API.Models
{
    public class SendEmailRequest
    {
        [Required, EmailAddress]
        public string ToEmail { get; set; }=string.Empty;

        public string? ToName { get; set; }
        
        [Required]
        public string Subject { get; set; }=string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool isHtml { get; set; } = true;
    }
}
