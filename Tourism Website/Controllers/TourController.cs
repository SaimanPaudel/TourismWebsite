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
        private readonly Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        private string Role => Session["UserRole"] as string ?? string.Empty;
        private string CurrentUserId => Session["UserId"]?.ToString() ?? string.Empty;
        private bool IsAdmin => Role == "Admin";
        private bool IsAgency => Role == "Agency";
        private bool IsGuide => Role == "Guide";
        private bool CanManage => IsAdmin || IsAgency || IsGuide;

        // Small helper to map string userId -> FullName
        private string GetUserNameByStringId(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid)) return null;
            return db.Users
                     .Where(u => u.UserId.ToString() == sid)
                     .Select(u => u.FullName)
                     .FirstOrDefault();
        }

        // GET: Tour
        public ActionResult Index()
        {
            IQueryable<Tour> query;

            if (IsAdmin)
            {
                query = db.Tours;
            }
            else if (IsAgency && !string.IsNullOrEmpty(CurrentUserId))
            {
                query = db.Tours.Where(t => t.CreatedByUserId == CurrentUserId);
            }
            else if (IsGuide && !string.IsNullOrEmpty(CurrentUserId))
            {
                query = db.Tours.Where(t => t.CreatedByUserId == CurrentUserId || t.GuideId == CurrentUserId);
            }
            else
            {
                // Tourists/anonymous: show all public tours
                query = db.Tours;
            }

            var tours = query.AsNoTracking().ToList();

            // Build a tourId -> host name map (Guide takes precedence, else Creator)
            var users = db.Users
                          .Select(u => new { u.UserId, u.FullName })
                          .ToList()
                          .ToDictionary(x => x.UserId.ToString(), x => x.FullName);

            ViewBag.HostNames = tours.ToDictionary(
                t => t.Id,
                t =>
                    (!string.IsNullOrEmpty(t.GuideId) && users.ContainsKey(t.GuideId))
                        ? users[t.GuideId]
                        : (users.ContainsKey(t.CreatedByUserId) ? users[t.CreatedByUserId] : "—")
            );

            return View(tours);
        }

        // GET: Tour/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var tour = db.Tours.Find(id);
            if (tour == null) return HttpNotFound();

            var creatorName = GetUserNameByStringId(tour.CreatedByUserId);
            var guideName = GetUserNameByStringId(tour.GuideId);

            ViewBag.HostedBy = guideName ?? creatorName;
            ViewBag.CreatorName = creatorName;
            ViewBag.GuideName = guideName;

            return View(tour);
        }

        // GET: Tour/Create
        public ActionResult Create()
        {
            if (!CanManage) return new HttpUnauthorizedResult();
            return View();
        }

        // POST: Tour/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include =
            "Title,Destination,Description,Price,DurationDays,ImagePath")] Tour tour)
        {
            if (!CanManage) return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                tour.CreatedAt = DateTime.UtcNow;
                tour.CreatedByUserId = CurrentUserId; // who created it

                // If a Guide creates it, also mark them as the assigned guide
                if (IsGuide)
                {
                    tour.GuideId = CurrentUserId;
                }

                db.Tours.Add(tour);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tour);
        }

        // GET: Tour/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!CanManage) return new HttpUnauthorizedResult();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var tour = db.Tours.Find(id);
            if (tour == null) return HttpNotFound();

            // Agency must own; Guide must own OR be assigned as GuideId
            if (IsAgency && tour.CreatedByUserId != CurrentUserId) return new HttpUnauthorizedResult();
            if (IsGuide && !(tour.CreatedByUserId == CurrentUserId || tour.GuideId == CurrentUserId))
                return new HttpUnauthorizedResult();

            return View(tour);
        }

        // POST: Tour/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
            "Id,Title,Destination,Description,Price,DurationDays,ImagePath")] Tour tour)
        {
            if (!CanManage) return new HttpUnauthorizedResult();
            if (!ModelState.IsValid) return View(tour);

            var existing = db.Tours.Find(tour.Id);
            if (existing == null) return HttpNotFound();

            // Agency must own; Guide must own OR be assigned
            if (IsAgency && existing.CreatedByUserId != CurrentUserId) return new HttpUnauthorizedResult();
            if (IsGuide && !(existing.CreatedByUserId == CurrentUserId || existing.GuideId == CurrentUserId))
                return new HttpUnauthorizedResult();

            // Update allowed fields only
            existing.Title = tour.Title;
            existing.Destination = tour.Destination;
            existing.Description = tour.Description;
            existing.Price = tour.Price;
            existing.DurationDays = tour.DurationDays;
            existing.ImagePath = tour.ImagePath;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Tour/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!CanManage) return new HttpUnauthorizedResult();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var tour = db.Tours.Find(id);
            if (tour == null) return HttpNotFound();

            if (IsAgency && tour.CreatedByUserId != CurrentUserId) return new HttpUnauthorizedResult();
            if (IsGuide && !(tour.CreatedByUserId == CurrentUserId || tour.GuideId == CurrentUserId))
                return new HttpUnauthorizedResult();

            return View(tour);
        }

        // POST: Tour/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!CanManage) return new HttpUnauthorizedResult();

            var tour = db.Tours.Find(id);
            if (tour == null) return HttpNotFound();

            if (IsAgency && tour.CreatedByUserId != CurrentUserId) return new HttpUnauthorizedResult();
            if (IsGuide && !(tour.CreatedByUserId == CurrentUserId || tour.GuideId == CurrentUserId))
                return new HttpUnauthorizedResult();

            db.Tours.Remove(tour);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
