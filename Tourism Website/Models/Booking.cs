using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Tourism_Website.Models
{
    public class Booking
    {

      
            public int Id { get; set; }

            [Required]
            public int TourId { get; set; }          // Link directly to Tour

            [Required]
            public int TouristId { get; set; }    // Link to User

            public DateTime BookingDate { get; set; } = DateTime.Now;
            public int NumberOfPeople { get; set; }
            public decimal TotalPrice { get; set; }
            public string Status { get; set; }       // Pending, Confirmed, Cancelled
            public string SpecialRequests { get; set; }

            // Navigation properties
            public virtual Tour Tour { get; set; }
            public virtual User Tourist { get; set; }
        
    }
    }


