using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Tourism_Website.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }   // Primary Key

        [Required]
        public int UserId { get; set; }       // Tourist

        [Required]
        public int TourId { get; set; }       // Which tour/package the feedback is about

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }       // Stars (1–5)

        [StringLength(500)]
        public string Comments { get; set; }  // Feedback text

        public DateTime DatePosted { get; set; } = DateTime.Now;

        // ✅ Navigation Properties (needed for Include)
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("TourId")]
        public virtual Tour Tour { get; set; }
    }
}