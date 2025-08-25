using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;  // <-- Needed for Include()
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly TourismDbContext _context = new TourismDbContext();

        // GET: Feedback for a tour
        public ActionResult Index(int tourId)
        {
            // Include User and Tour navigation properties
            var feedbacks = _context.Feedbacks
                                    .Include(f => f.User)
                                    .Include(f => f.Tour)
                                    .Where(f => f.TourId == tourId)
                                    .ToList();
            return View(feedbacks);
        }

        // GET: Leave feedback
        public ActionResult Create(int tourId)
        {
            ViewBag.TourId = tourId;
            return View();
        }

        // POST: Save feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Feedbacks.Add(feedback);
                _context.SaveChanges();
                return RedirectToAction("Index", new { tourId = feedback.TourId });
            }
            return View(feedback);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
