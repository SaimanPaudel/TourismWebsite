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
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            IQueryable<Feedback> feedbacks = db.Feedbacks.Include(f => f.Tour).Include(f => f.User);

            if (role == "Tourist" && int.TryParse(userIdStr, out int uid))
            {
                feedbacks = feedbacks.Where(f => f.UserId == uid);
            }

            return View(feedbacks.ToList());
        }

        // GET: Feedback/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks.Include(f => f.Tour).Include(f => f.User).FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null) return HttpNotFound();

            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            if (role == "Admin" || role == "Agency" || role == "Guide" ||
                (role == "Tourist" && feedback.UserId.ToString() == userIdStr))
            {
                return View(feedback);
            }

            return new HttpUnauthorizedResult();
        }

        // GET: Feedback/Create
        public ActionResult Create()
        {
            if (Session["UserRole"] == null)
                return RedirectToAction("Login", "Account");

            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title");
            return View();
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TourId,Rating,Comments")] Feedback feedback)
        {
            if (Session["UserRole"] == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                feedback.DatePosted = DateTime.Now;
                feedback.UserId = (int)Session["UserId"];
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", feedback.TourId);
            return View(feedback);
        }

        // GET: Feedback/Edit/5
        public ActionResult Edit(int? id)
        {
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks.Find(id);
            if (feedback == null)
                return HttpNotFound();

            if (role == "Admin" || role == "Agency" || role == "Guide" ||
                (role == "Tourist" && feedback.UserId.ToString() == userIdStr))
            {
                ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", feedback.UserId);
                ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", feedback.TourId);
                return View(feedback);
            }

            return new HttpUnauthorizedResult();
        }

        // POST: Feedback/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FeedbackId,UserId,TourId,Rating,Comments,DatePosted")] Feedback feedback)
        {
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            var existing = db.Feedbacks.Find(feedback.FeedbackId);
            if (existing == null)
                return HttpNotFound();

            if (role != "Admin" && role != "Agency" && role != "Guide" &&
                !(role == "Tourist" && existing.UserId.ToString() == userIdStr))
            {
                return new HttpUnauthorizedResult();
            }

            if (ModelState.IsValid)
            {
                existing.UserId = feedback.UserId;
                existing.TourId = feedback.TourId;
                existing.Rating = feedback.Rating;
                existing.Comments = feedback.Comments;
                existing.DatePosted = feedback.DatePosted;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "UserId", "FullName", feedback.UserId);
            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", feedback.TourId);
            return View(feedback);
        }

        // GET: Feedback/Delete/5
        public ActionResult Delete(int? id)
        {
            var role = Session["UserRole"]?.ToString();
            var userIdStr = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var feedback = db.Feedbacks.Include(f => f.Tour).Include(f => f.User).FirstOrDefault(f => f.FeedbackId == id);
            if (feedback == null)
                return HttpNotFound();

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
            if (feedback == null)
                return HttpNotFound();

            if (role == "Admin" || role == "Agency" || role == "Guide" ||
                (role == "Tourist" && feedback.UserId.ToString() == userIdStr))
            {
                db.Feedbacks.Remove(feedback);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return new HttpUnauthorizedResult();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
