using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Tourism_Website.Models
{
    public class Tour
    {
       
            public int Id { get; set; }   // PK

            [Required, StringLength(100)]
            public string Title { get; set; }   // e.g., "Paris City Tour"

            [Required, StringLength(100)]
            public string Destination { get; set; }   // City or country

            [DataType(DataType.MultilineText)]
            public string Description { get; set; }   // Details of the tour

            [Range(0, 999999), DataType(DataType.Currency)]
            public decimal Price { get; set; }   // Price for tourists

            [Display(Name = "Duration (days)")]
            [Range(1, 365)]
            public int DurationDays { get; set; }   // Duration in days

            [Display(Name = "Image Path")]
            public string ImagePath { get; set; }   // Store path of an uploaded image

            // (Optional) Who created this tour (Agency/Guide)
            public string CreatedByUserId { get; set; }

            [Display(Name = "Created At")]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    }




