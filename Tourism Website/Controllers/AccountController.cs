using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Tourism_Website.Data;
using Tourism_Website.Models;
using Tourism_Website.Security; // <-- make sure you added Security/PasswordHashing.cs

namespace Tourism_Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly Tourism_WebsiteContext db = new Tourism_WebsiteContext();

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
            if (!ModelState.IsValid)
                return View(model);

            // Unique email check
            if (db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(model);
            }

            // Normalize role input (fallback to Tourist)
            var requested = (model.Role ?? "Tourist").Trim();

            // Create user with HASHED password
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = PasswordHashing.Hash(model.Password), // <-- hashed
                Role = "Tourist",
                RequestedRole = null,
                IsRoleApproved = null
            };

            // If they selected Agency/Guide, record a pending request
            if (requested.Equals("Agency", StringComparison.OrdinalIgnoreCase) ||
                requested.Equals("Guide", StringComparison.OrdinalIgnoreCase))
            {
                user.RequestedRole = requested;    // "Agency" or "Guide"
                user.IsRoleApproved = null;        // pending
            }

            try
            {
                db.Users.Add(user);
                db.SaveChanges();

                if (user.RequestedRole != null)
                {
                    TempData["SuccessMessage"] =
                        $"Thanks for registering! Your request to become {user.RequestedRole} is pending admin approval. " +
                        "You can log in as a Tourist for now.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating user: " + ex.Message);
                return View(model);
            }
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
                return View(model);

            // Look up user by email only
            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Verify password (supports legacy plaintext and new hashed)
            bool ok, needsUpgrade;

            if (PasswordHashing.IsFormattedHash(user.Password))
            {
                ok = PasswordHashing.Verify(model.Password, user.Password, out needsUpgrade);
            }
            else
            {
                // Legacy plaintext row: compare directly once, then upgrade on success
                ok = string.Equals(model.Password, user.Password);
                needsUpgrade = ok; // upgrade immediately if login succeeds
            }

            if (!ok)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Transparent upgrade for legacy/weak hashes
            if (needsUpgrade || !PasswordHashing.IsFormattedHash(user.Password))
            {
                user.Password = PasswordHashing.Hash(model.Password);
                db.SaveChanges();
            }

            // Set auth cookie
            FormsAuthentication.SetAuthCookie(user.Email, false);

            // Set session with the EFFECTIVE role
            Session["UserId"] = user.UserId;
            Session["UserRole"] = user.Role;     // "Tourist" unless approved to Agency/Guide/Admin
            Session["UserName"] = user.FullName; // handy for UI

            // Info banners about request status
            if (user.RequestedRole != null && user.IsRoleApproved == null)
            {
                TempData["Info"] = $"Your request to become {user.RequestedRole} is pending admin approval. You currently have Tourist access.";
            }
            else if (user.RequestedRole != null && user.IsRoleApproved == false)
            {
                TempData["Info"] = $"Your {user.RequestedRole} request was rejected by admin.";
            }

            // Redirect to returnUrl if provided and local
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirect to dashboards by EFFECTIVE role
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
