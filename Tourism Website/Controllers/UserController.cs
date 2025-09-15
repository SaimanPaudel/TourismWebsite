using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tourism_Website.Data;
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class UserController : Controller
    {
        private readonly Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        private bool IsAdmin() => (Session["UserRole"] as string) == "Admin";

        // GET: User
        // Supports search (q) and role filter (role = All/Tourist/Agency/Guide/Admin/Pending)
        [HttpGet]
        public ActionResult Index(string q, string role = "All")
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            var users = db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                users = users.Where(u => u.FullName.Contains(q) || u.Email.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (role.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    users = users.Where(u => u.RequestedRole != null && u.IsRoleApproved == null);
                }
                else
                {
                    users = users.Where(u => u.Role == role);
                }
            }

            // Consistent ordering (Role then Name)
            var list = users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToList();

            return View(list);
        }

        // GET: User/Details/5
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users
                         .Include(u => u.Bookings)
                         .Include(u => u.Feedbacks)
                         .FirstOrDefault(u => u.UserId == id);

            if (user == null) return HttpNotFound();

            return View(user);
        }

        // GET: User/Create
        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FullName,Email,Password,Role")] User input)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) return View(input);

            if (db.Users.Any(u => u.Email == input.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(input);
            }

            // Admin-created users are final—no requested-role flow here.
            var user = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Password = input.Password,   // TODO: hash in production
                Role = string.IsNullOrWhiteSpace(input.Role) ? "Tourist" : input.Role.Trim(),
                RequestedRole = null,
                IsRoleApproved = null
            };

            db.Users.Add(user);
            db.SaveChanges();

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction("Index");
        }

        // GET: User/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,FullName,Email,Password,Role")] User input)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) return View(input);

            // Email uniqueness check
            if (db.Users.Any(u => u.Email == input.Email && u.UserId != input.UserId))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(input);
            }

            var existing = db.Users.Find(input.UserId);
            if (existing == null) return HttpNotFound();

            // Update only allowed fields (keep RequestedRole/IsRoleApproved intact)
            existing.FullName = input.FullName;
            existing.Email = input.Email;
            existing.Password = input.Password;  // TODO: hash in production
            existing.Role = string.IsNullOrWhiteSpace(input.Role) ? existing.Role : input.Role.Trim();

            db.Entry(existing).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: User/Delete/5
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users
                         .Include(u => u.Bookings)
                         .Include(u => u.Feedbacks)
                         .FirstOrDefault(u => u.UserId == id);

            if (user == null) return HttpNotFound();

            if (user.Bookings.Any() || user.Feedbacks.Any())
            {
                ViewBag.ErrorMessage = "Cannot delete user with existing bookings or feedback.";
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            var user = db.Users
                         .Include(u => u.Bookings)
                         .Include(u => u.Feedbacks)
                         .FirstOrDefault(u => u.UserId == id);

            if (user != null && !user.Bookings.Any() && !user.Feedbacks.Any())
            {
                db.Users.Remove(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "User deleted.";
            }
            else
            {
                TempData["Error"] = "User cannot be deleted (has bookings or feedback).";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
