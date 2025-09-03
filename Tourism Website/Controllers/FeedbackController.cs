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
        private Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // GET: Feedback
        public ActionResult Index()
        {
            var feedbacks = db.Feedbacks.Include(f => f.Tour).Include(f => f.User).ToList();
            return View(feedbacks);
        }

        // GET: Feedback/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks.Include(f => f.Tour)
                                       .Include(f => f.User)
                                       .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null)
                return HttpNotFound();

            return View(feedback);
        }

        // GET: Feedback/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName");
            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title"); // Use Id here
            return View();
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,TourId,Rating,Comments")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.DatePosted = DateTime.Now;
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Repopulate dropdowns if validation fails
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", feedback.UserId);
            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", feedback.TourId);
            return View(feedback);
        }

        // GET: Feedback/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks.Find(id);
            if (feedback == null)
                return HttpNotFound();

            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", feedback.UserId);
            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", feedback.TourId);
            return View(feedback);
        }

        // POST: Feedback/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FeedbackId,UserId,TourId,Rating,Comments")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Feedbacks.Find(feedback.FeedbackId);
                if (existing != null)
                {
                    existing.UserId = feedback.UserId;
                    existing.TourId = feedback.TourId;
                    existing.Rating = feedback.Rating;
                    existing.Comments = feedback.Comments;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", feedback.UserId);
            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", feedback.TourId);
            return View(feedback);
        }

        // GET: Feedback/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks.Include(f => f.Tour)
                                       .Include(f => f.User)
                                       .FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null)
                return HttpNotFound();

            return View(feedback);
        }

        // POST: Feedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var feedback = db.Feedbacks.Find(id);
            if (feedback != null)
            {
                db.Feedbacks.Remove(feedback);
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
