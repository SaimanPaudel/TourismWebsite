using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tourism_Website.Data;
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // Helper to fill dropdowns the Edit/Create views may expect
        private void PopulateSelects(Feedback model)
        {
            ViewBag.TourId = new SelectList(
                db.Tours.OrderBy(t => t.Title).ToList(),
                "Id", "Title", model?.TourId
            );

            // Safe to provide even if your view doesn't show it
            ViewBag.UserId = new SelectList(
                db.Users.OrderBy(u => u.FullName).ToList(),
                "UserId", "FullName", model?.UserId
            );
        }

        // GET: Feedback
        // Everyone can see feedback. Optional ?tourId= filters to a single tour.
        public ActionResult Index(int? tourId)
        {
            var feedbacks = db.Feedbacks
                .AsNoTracking()
                .Include(f => f.Tour)
                .Include(f => f.User);

            if (tourId.HasValue)
                feedbacks = feedbacks.Where(f => f.TourId == tourId.Value);

            return View(feedbacks
                .OrderByDescending(f => f.DatePosted)
                .ToList());
        }

        // GET: Feedback/Details/5
        // Everyone can view a single feedback item.
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks
                .Include(f => f.Tour)
                .Include(f => f.User)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null) return HttpNotFound();

            return View(feedback);
        }

        // GET: Feedback/Create
        public ActionResult Create(int? tourId)
        {
            if (Session["UserRole"] == null || Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            var model = new Feedback { TourId = tourId ?? 0 };
            PopulateSelects(model);
            return View(model);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TourId,Rating,Comments")] Feedback feedback)
        {
            if (Session["UserRole"] == null || Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            if (!int.TryParse(Session["UserId"].ToString(), out int uid))
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                feedback.UserId = uid;                 // author is the logged-in user
                feedback.DatePosted = DateTime.Now;

                db.Feedbacks.Add(feedback);
                db.SaveChanges();

                return RedirectToAction("Index", new { tourId = feedback.TourId });
            }

            PopulateSelects(feedback);
            return View(feedback);
        }

        // GET: Feedback/Edit/5
        public ActionResult Edit(int? id)
        {
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks
                .Include(f => f.Tour)
                .Include(f => f.User)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null) return HttpNotFound();

            // Admin/Agency/Guide can edit; Tourist can edit only own feedback
            if (role == "Admin" || role == "Agency" || role == "Guide" ||
                (role == "Tourist" && feedback.UserId.ToString() == userIdStr))
            {
                PopulateSelects(feedback); // <<< ensures ViewBag.UserId & ViewBag.TourId are SelectLists
                return View(feedback);
            }

            return new HttpUnauthorizedResult();
        }

        // POST: Feedback/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FeedbackId,TourId,UserId,Rating,Comments")] Feedback posted)
        {
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            var existing = db.Feedbacks.Find(posted.FeedbackId);
            if (existing == null) return HttpNotFound();

            // Authorization: same as GET
            var isTouristOwner = (role == "Tourist" && existing.UserId.ToString() == userIdStr);
            if (role != "Admin" && role != "Agency" && role != "Guide" && !isTouristOwner)
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                // Update allowed fields
                existing.TourId = posted.TourId;
                existing.Rating = posted.Rating;
                existing.Comments = posted.Comments;

                // Only staff may reassign the author; tourists cannot change it
                if (role == "Admin" || role == "Agency" || role == "Guide")
                {
                    // If your Edit view doesn't post UserId, this line just keeps the old value
                    if (posted.UserId != 0) existing.UserId = posted.UserId;
                }

                // Keep original DatePosted (or uncomment next line to mark as edited "now")
                // existing.DatePosted = DateTime.Now;

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", new { tourId = existing.TourId });
            }

            // Repopulate dropdowns when returning with errors
            PopulateSelects(posted);
            return View(existing);
        }

        // GET: Feedback/Delete/5
        public ActionResult Delete(int? id)
        {
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks
                .Include(f => f.Tour)
                .Include(f => f.User)
                .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null) return HttpNotFound();

            if (role == "Admin" || role == "Agency" || role == "Guide" ||
                (role == "Tourist" && feedback.UserId.ToString() == userIdStr))
            {
                return View(feedback);
            }

            return new HttpUnauthorizedResult();
        }

        // POST: Feedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            var feedback = db.Feedbacks.Find(id);
            if (feedback == null) return HttpNotFound();

            if (role == "Admin" || role == "Agency" || role == "Guide" ||
                (role == "Tourist" && feedback.UserId.ToString() == userIdStr))
            {
                var tourId = feedback.TourId;
                db.Feedbacks.Remove(feedback);
                db.SaveChanges();
                return RedirectToAction("Index", new { tourId });
            }

            return new HttpUnauthorizedResult();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
