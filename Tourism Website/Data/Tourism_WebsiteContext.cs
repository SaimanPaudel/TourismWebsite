using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Tourism_Website.Data
{
    public class Tourism_WebsiteContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public Tourism_WebsiteContext() : base("name=Tourism_WebsiteContext")
        {
        }

        public System.Data.Entity.DbSet<Tourism_Website.Models.Feedback> Feedbacks { get; set; }

        public System.Data.Entity.DbSet<Tourism_Website.Models.Tour> Tours { get; set; }

        public System.Data.Entity.DbSet<Tourism_Website.Models.User> Users { get; set; }

        public System.Data.Entity.DbSet<Tourism_Website.Models.Booking> Bookings { get; set; }

       
    }
}
