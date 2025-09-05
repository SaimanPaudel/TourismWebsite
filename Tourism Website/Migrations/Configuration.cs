namespace Tourism_Website.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Tourism_Website.Data.Tourism_WebsiteContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Tourism_Website.Data.Tourism_WebsiteContext";
        }

        protected override void Seed(Tourism_Website.Data.Tourism_WebsiteContext context)
        {
            // Seed admin user if not exist
            if (!context.Users.Any(u => u.Email == "saimanpaudel@gmail.com"))
            {
                context.Users.AddOrUpdate(u => u.Email, new Models.User
                {
                    FullName = "Saiman Paudel",
                    Email = "saimanpaudel@gmail.com",
                    Password = "Saiman",
                    Role = "Admin"
                });
            }
        }
    }
}

