using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

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
    }
}