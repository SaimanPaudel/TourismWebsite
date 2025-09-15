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

        private string Role => Session["UserRole"]?.ToString() ?? "";
        private string UserIdStr => Session["UserId"]?.ToString() ?? "";

        private void PopulateSelects(Feedback model)
        {
            ViewBag.TourId = new SelectList(
                db.Tours.OrderBy(t => t.Title).ToList(), "Id", "Title", model?.TourId);

            // present but not used (author not editable)
            ViewBag.UserId = new SelectList(
                db.Users.OrderBy(u => u.FullName).ToList(), "UserId", "FullName", model?.UserId);
        }

        // ===== View (everyone) ===============================================================

        public ActionResult Index(int? tourId)
        {
            var q = db.Feedbacks.AsNoTracking()
                                .Include(f => f.Tour)
                                .Include(f => f.User);

            if (tourId.HasValue) q = q.Where(f => f.TourId == tourId.Value);

            return View(q.OrderByDescending(f => f.DatePosted).ToList());
        }

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

        // ===== Create (Tourist only) =========================================================

        [HttpGet]
        public ActionResult Create(int? tourId)
        {
            if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(UserIdStr))
                return RedirectToAction("Login", "Account");

            // Only Tourists may create feedback (Agency/Guide view-only; Admin typically doesn’t create)
            if (Role != "Tourist") return new HttpUnauthorizedResult();

            var model = new Feedback { TourId = tourId ?? 0 };
            PopulateSelects(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TourId,Rating,Comments")] Feedback feedback)
        {
            if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(UserIdStr))
                return RedirectToAction("Login", "Account");

            if (Role != "Tourist") return new HttpUnauthorizedResult();

            if (!int.TryParse(UserIdStr, out var uid))
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                feedback.UserId = uid;              // author = current tourist
                feedback.DatePosted = DateTime.Now;

                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                return RedirectToAction("Index", new { tourId = feedback.TourId });
            }

            PopulateSelects(feedback);
            return View(feedback);
        }

        // ===== Edit (Admin or Tourist owner) ================================================

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(UserIdStr))
                return RedirectToAction("Login", "Account");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var fb = db.Feedbacks.Include(f => f.Tour)
                                 .Include(f => f.User)
                                 .FirstOrDefault(f => f.FeedbackId == id);
            if (fb == null) return HttpNotFound();

            var isTouristOwner = Role == "Tourist" && fb.UserId.ToString() == UserIdStr;
            if (!(Role == "Admin" || isTouristOwner)) return new HttpUnauthorizedResult();

            PopulateSelects(fb);
            return View(fb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FeedbackId,TourId,Rating,Comments")] Feedback posted)
        {
            if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(UserIdStr))
                return RedirectToAction("Login", "Account");

            var existing = db.Feedbacks.Find(posted.FeedbackId);
            if (existing == null) return HttpNotFound();

            var isTouristOwner = Role == "Tourist" && existing.UserId.ToString() == UserIdStr;
            if (!(Role == "Admin" || isTouristOwner)) return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                existing.TourId = posted.TourId;
                existing.Rating = posted.Rating;
                existing.Comments = posted.Comments;
                // keep existing.UserId and existing.DatePosted

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { tourId = existing.TourId });
            }

            PopulateSelects(posted);
            return View(existing);
        }

        // ===== Delete (Admin or Tourist owner) ===============================================

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(UserIdStr))
                return RedirectToAction("Login", "Account");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var fb = db.Feedbacks.Include(f => f.Tour)
                                 .Include(f => f.User)
                                 .FirstOrDefault(f => f.FeedbackId == id);
            if (fb == null) return HttpNotFound();

            var isTouristOwner = Role == "Tourist" && fb.UserId.ToString() == UserIdStr;
            if (!(Role == "Admin" || isTouristOwner)) return new HttpUnauthorizedResult();

            return View(fb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var fb = db.Feedbacks.Find(id);
            if (fb == null) return HttpNotFound();

            var isTouristOwner = Role == "Tourist" && fb.UserId.ToString() == UserIdStr;
            if (!(Role == "Admin" || isTouristOwner)) return new HttpUnauthorizedResult();

            var tourId = fb.TourId;
            db.Feedbacks.Remove(fb);
            db.SaveChanges();
            return RedirectToAction("Index", new { tourId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
