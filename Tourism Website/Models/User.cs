using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Tourism_Website.Models
{
    public class User
    {
        public int UserId { get; set; }  // PK

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; } // Consider hashing in production

        [StringLength(50)]
        public string Role { get; set; } = "Tourist";

        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new HashSet<Feedback>();

        // Optional: convenience helper properties
        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        public bool IsAgency => Role.Equals("Agency", StringComparison.OrdinalIgnoreCase);
        public bool IsTourist => Role.Equals("Tourist", StringComparison.OrdinalIgnoreCase);
        public bool IsGuide => Role.Equals("Guide", StringComparison.OrdinalIgnoreCase);
    }
}
