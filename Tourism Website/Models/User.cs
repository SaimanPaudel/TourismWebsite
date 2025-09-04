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
            public string Email { get; set; }

            [Required]
            [StringLength(100)]
            public string Password { get; set; } // Consider hashing in production

            // Optional role field if needed
            [StringLength(50)]
            public string Role { get; set; } = "Tourist";

            // Navigation properties
            public virtual ICollection<Booking> Bookings { get; set; }
            public virtual ICollection<Feedback> Feedbacks { get; set; }
        }
    }


    

