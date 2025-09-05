using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tourism_Website.Data;
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class TourController : Controller
    {
        private Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // GET: Tour
        public ActionResult Index()
        {
            var role = Session["UserRole"] as string;
            var userIdString = Session["UserId"]?.ToString() ?? string.Empty;

            if (role == "Agency" && !string.IsNullOrEmpty(userIdString))
            {
                var agencyTours = db.Tours.Where(t => t.CreatedByUserId == userIdString).ToList();
                return View(agencyTours);
            }
            else if (role == "Admin")
            {
                return View(db.Tours.ToList());
            }
            else
            {
                return View(db.Tours.ToList());
            }
        }

        // GET: Tour/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Tour tour = db.Tours.Find(id);
            if (tour == null)
                return HttpNotFound();

            return View(tour);
        }

        // GET: Tour/Create
        public ActionResult Create()
        {
            var role = Session["UserRole"] as string;
            if (role != "Agency" && role != "Admin")
                return new HttpUnauthorizedResult();

            return View();
        }

        // POST: Tour/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Destination,Description,Price,DurationDays,ImagePath")] Tour tour)
        {
            var role = Session["UserRole"] as string;
            var userIdString = Session["UserId"]?.ToString() ?? string.Empty;

            if (role != "Agency" && role != "Admin")
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                tour.CreatedAt = DateTime.UtcNow;
                tour.CreatedByUserId = userIdString; // Logged-in user id as string
                db.Tours.Add(tour);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tour);
        }

        // GET: Tour/Edit/5
        public ActionResult Edit(int? id)
        {
            var role = Session["UserRole"] as string;
            var userIdString = Session["UserId"]?.ToString() ?? string.Empty;

            if (role != "Agency" && role != "Admin")
                return new HttpUnauthorizedResult();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Tour tour = db.Tours.Find(id);
            if (tour == null)
                return HttpNotFound();

            if (role == "Agency" && (string.IsNullOrEmpty(tour.CreatedByUserId) || tour.CreatedByUserId != userIdString))
                return new HttpUnauthorizedResult();

            return View(tour);
        }

        // POST: Tour/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Destination,Description,Price,DurationDays,ImagePath")] Tour tour)
        {
            var role = Session["UserRole"] as string;
            var userIdString = Session["UserId"]?.ToString() ?? string.Empty;

            if (role != "Agency" && role != "Admin")
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                var existingTour = db.Tours.Find(tour.Id);
                if (existingTour == null)
                    return HttpNotFound();

                if (role == "Agency" && (string.IsNullOrEmpty(existingTour.CreatedByUserId) || existingTour.CreatedByUserId != userIdString))
                    return new HttpUnauthorizedResult();

                existingTour.Title = tour.Title;
                existingTour.Destination = tour.Destination;
                existingTour.Description = tour.Description;
                existingTour.Price = tour.Price;
                existingTour.DurationDays = tour.DurationDays;
                existingTour.ImagePath = tour.ImagePath;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tour);
        }

        // GET: Tour/Delete/5
        public ActionResult Delete(int? id)
        {
            var role = Session["UserRole"] as string;
            var userIdString = Session["UserId"]?.ToString() ?? string.Empty;

            if (role != "Agency" && role != "Admin")
                return new HttpUnauthorizedResult();

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Tour tour = db.Tours.Find(id);
            if (tour == null)
                return HttpNotFound();

            if (role == "Agency" && (string.IsNullOrEmpty(tour.CreatedByUserId) || tour.CreatedByUserId != userIdString))
                return new HttpUnauthorizedResult();

            return View(tour);
        }

        // POST: Tour/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var role = Session["UserRole"] as string;
            var userIdString = Session["UserId"]?.ToString() ?? string.Empty;

            if (role != "Agency" && role != "Admin")
                return new HttpUnauthorizedResult();

            var tour = db.Tours.Find(id);
            if (tour == null)
                return HttpNotFound();

            if (role == "Agency" && (string.IsNullOrEmpty(tour.CreatedByUserId) || tour.CreatedByUserId != userIdString))
                return new HttpUnauthorizedResult();

            db.Tours.Remove(tour);
            db.SaveChanges();
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
