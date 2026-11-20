using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CREEOT.WEB.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string UserType { get; set; } = "Client"; // "Client" or "Influencer"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (1-to-1)
        public virtual Brand? BrandProfile { get; set; }
        public virtual Influencer? InfluencerProfile { get; set; }
    }
}
