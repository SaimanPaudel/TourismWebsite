using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tourism_Website.Data;
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class AccountController : Controller
    {
        private Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Please enter email and password");
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                // Log the user in
                FormsAuthentication.SetAuthCookie(user.Email, false);
                return RedirectToAction("Index", "Home"); // redirect after login
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already registered");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password, // In production, hash this
                    Role = "Tourist"
                };

                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
