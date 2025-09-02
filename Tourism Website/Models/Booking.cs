using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tourism_Website.Models
{
    public class Booking
    {
   
            public int BookingId { get; set; }
            public int TourId { get; set; }
            public int UserId { get; set; }
        }
    }

