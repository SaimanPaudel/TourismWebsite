using System;
using System.ComponentModel.DataAnnotations;

namespace Tourism_Website.Models
{
    public class Tour
    {
        [Key]   // primary key
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }   // e.g., "Paris City Tour"

        [Required, StringLength(100)]
        public string Destination { get; set; }   // City or country

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Range(0, 999999), DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Duration (days)")]
        [Range(1, 365)]
        public int DurationDays { get; set; }

        [Display(Name = "Image Path")]
        public string ImagePath { get; set; }

        public string CreatedByUserId { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
