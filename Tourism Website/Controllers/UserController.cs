using System;
using System.Collections.Generic;
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
        private Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // GET: User
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: User/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Include(u => u.Bookings)
                               .Include(u => u.Feedbacks)
                               .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // GET: User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FullName,Email,Password,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(user);
                }

                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: User/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,FullName,Email,Password,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                // Check if email is changed and unique
                if (db.Users.Any(u => u.Email == user.Email && u.UserId != user.UserId))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(user);
                }

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: User/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Include(u => u.Bookings)
                               .Include(u => u.Feedbacks)
                               .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return HttpNotFound();

            // Optional: prevent deletion if user has bookings or feedback
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
            var user = db.Users.Include(u => u.Bookings)
                               .Include(u => u.Feedbacks)
                               .FirstOrDefault(u => u.UserId == id);

            if (user != null && !user.Bookings.Any() && !user.Feedbacks.Any())
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
