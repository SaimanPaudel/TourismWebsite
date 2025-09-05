using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Tourism_Website.Data;
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class AccountController : Controller
    {
        private Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // GET: Account/Register
        [HttpGet]
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
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,  // TODO: Hash passwords before saving in production
                    Role = model.Role
                };
                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating user: " + ex.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
            if (user != null)
            {
                // Set auth cookie
                FormsAuthentication.SetAuthCookie(user.Email, false);

                // Set session variables
                Session["UserRole"] = user.Role;
                Session["UserId"] = user.UserId;

                // Redirect to returnUrl if it exists and is local
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect based on role if no returnUrl
                switch (user.Role)
                {
                    case "Admin":
                        return RedirectToAction("AdminDashboard", "Dashboard");
                    case "Agency":
                        return RedirectToAction("AgencyDashboard", "Dashboard");
                    case "Guide":
                        return RedirectToAction("GuideDashboard", "Dashboard");
                    case "Tourist":
                        return RedirectToAction("TouristDashboard", "Dashboard");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
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
