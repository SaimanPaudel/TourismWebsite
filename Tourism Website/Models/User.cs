using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tourism_Website.Models
{
    public class User
    {
        public int UserId { get; set; }  // PK

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; }

        [Required, StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// The effective role used for authorization and Session["UserRole"].
        /// Allowed values: "Admin", "Agency", "Guide", "Tourist"
        /// Default is "Tourist". Do NOT auto-assign Agency/Guide here
        /// unless an admin has approved.
        /// </summary>
        [StringLength(50)]
        public string Role { get; set; } = "Tourist";

        /// <summary>
        /// If the user requested an elevated role (e.g., "Agency" or "Guide"),
        /// store it here until an admin approves/rejects.
        /// Null if no request is pending or after approval if you clear it.
        /// </summary>
        [StringLength(50)]
        public string RequestedRole { get; set; } // null | "Agency" | "Guide"

        /// <summary>
        /// null  = pending
        /// true  = approved (admin accepted the request)
        /// false = rejected (admin rejected the request)
        /// </summary>
        public bool? IsRoleApproved { get; set; }  // null=pending, true=approved, false=rejected

        // --- Navigation properties ---
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new HashSet<Feedback>();

        // --- Effective role helpers ---
        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        public bool IsAgency => Role.Equals("Agency", StringComparison.OrdinalIgnoreCase);
        public bool IsTourist => Role.Equals("Tourist", StringComparison.OrdinalIgnoreCase);
        public bool IsGuide => Role.Equals("Guide", StringComparison.OrdinalIgnoreCase);

        // --- Requested role helpers ---
        public bool RequestedAgency => RequestedRole != null && RequestedRole.Equals("Agency", StringComparison.OrdinalIgnoreCase);
        public bool RequestedGuide => RequestedRole != null && RequestedRole.Equals("Guide", StringComparison.OrdinalIgnoreCase);

        public bool IsPendingRoleRequest => RequestedRole != null && IsRoleApproved == null;
        public bool IsRejectedRoleRequest => RequestedRole != null && IsRoleApproved == false;
    }
}
