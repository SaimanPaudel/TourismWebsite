using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tourism_Website.Data;
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class BookingController : Controller
    {
        private readonly Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // Session helpers
        private string Role => Session["UserRole"]?.ToString() ?? "";
        private int? UserIdInt => int.TryParse(Session["UserId"]?.ToString(), out var x) ? (int?)x : null;
        private bool IsLoggedIn => !string.IsNullOrEmpty(Role) && UserIdInt.HasValue;
        private bool CanManage => Role == "Admin" || Role == "Agency" || Role == "Guide";

        // Only populate the Tours dropdown; Tourist is locked/read-only
        private void PopulateEditSelects(Booking booking)
        {
            ViewBag.TourId = new SelectList(
                db.Tours.OrderBy(t => t.Title).ToList(),
                "Id", "Title", booking?.TourId);
        }

        // GET: Booking
        public ActionResult Index()
        {
            if (!IsLoggedIn) return RedirectToAction("Login", "Account");

            var bookings = db.Bookings
                .AsNoTracking()
                .Include(b => b.Tour)
                .Include(b => b.Tourist)
                .AsQueryable();

            switch (Role)
            {
                case "Admin":
                    bookings = bookings.OrderByDescending(b => b.BookingDate);
                    break;

                case "Agency":
                    {
                        var uid = Session["UserId"]?.ToString();
                        var agencyTourIds = db.Tours
                            .Where(t => t.CreatedByUserId == uid)
                            .Select(t => t.Id);
                        bookings = bookings.Where(b => agencyTourIds.Contains(b.TourId))
                                           .OrderByDescending(b => b.BookingDate);
                        break;
                    }

                case "Guide":
                    {
                        var uid = Session["UserId"]?.ToString();
                        var guideTourIds = db.Tours
                            .Where(t => t.GuideId == uid)
                            .Select(t => t.Id);
                        bookings = bookings.Where(b => guideTourIds.Contains(b.TourId))
                                           .OrderByDescending(b => b.BookingDate);
                        break;
                    }

                case "Tourist":
                    {
                        if (!UserIdInt.HasValue) return RedirectToAction("Login", "Account");
                        var touristId = UserIdInt.Value;
                        bookings = bookings.Where(b => b.TouristId == touristId)
                                           .OrderByDescending(b => b.BookingDate);
                        break;
                    }

                default:
                    return RedirectToAction("Login", "Account");
            }

            return View(bookings.ToList());
        }

        // GET: Booking/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.Bookings
                .Include(b => b.Tour)
                .Include(b => b.Tourist)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null) return HttpNotFound();

            // Fallback: hydrate Tourist if nav is not populated
            if (booking.Tourist == null && booking.TouristId > 0)
                booking.Tourist = db.Users.Find(booking.TouristId);

            if (CanManage || (UserIdInt.HasValue && booking.TouristId == UserIdInt.Value))
                return View(booking);

            return new HttpUnauthorizedResult();
        }

        // GET: Booking/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!CanManage) return new HttpUnauthorizedResult();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.Bookings
                .Include(b => b.Tour)
                .Include(b => b.Tourist)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null) return HttpNotFound();

            // Fallback hydrate
            if (booking.Tourist == null && booking.TouristId > 0)
                booking.Tourist = db.Users.Find(booking.TouristId);

            // Scope Agency/Guide to only their tours
            if (Role == "Agency")
            {
                var uid = Session["UserId"]?.ToString();
                bool ownsTour = db.Tours.Any(t => t.Id == booking.TourId && t.CreatedByUserId == uid);
                if (!ownsTour) return new HttpUnauthorizedResult();
            }
            if (Role == "Guide")
            {
                var uid = Session["UserId"]?.ToString();
                bool guidesTour = db.Tours.Any(t => t.Id == booking.TourId && t.GuideId == uid);
                if (!guidesTour) return new HttpUnauthorizedResult();
            }

            PopulateEditSelects(booking);
            return View(booking);
        }

        // POST: Booking/Edit/5
        // TouristId deliberately NOT bound: the tourist is locked and cannot be changed.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TourId,BookingDate,NumberOfPeople,Status,SpecialRequests")] Booking posted)
        {
            if (!CanManage) return new HttpUnauthorizedResult();

            var existing = db.Bookings
                .Include(b => b.Tour)
                .FirstOrDefault(b => b.Id == posted.Id);
            if (existing == null) return HttpNotFound();

            // Scope Agency/Guide to only their tours on the EXISTING record
            if (Role == "Agency")
            {
                var uid = Session["UserId"]?.ToString();
                bool ownsTour = db.Tours.Any(t => t.Id == existing.TourId && t.CreatedByUserId == uid);
                if (!ownsTour) return new HttpUnauthorizedResult();
            }
            if (Role == "Guide")
            {
                var uid = Session["UserId"]?.ToString();
                bool guidesTour = db.Tours.Any(t => t.Id == existing.TourId && t.GuideId == uid);
                if (!guidesTour) return new HttpUnauthorizedResult();
            }

            if (ModelState.IsValid)
            {
                // Update allowed fields ONLY (TouristId is intentionally NOT touched)
                existing.TourId = posted.TourId;
                existing.BookingDate = posted.BookingDate;
                existing.NumberOfPeople = posted.NumberOfPeople;
                existing.Status = posted.Status;
                existing.SpecialRequests = posted.SpecialRequests;

                // Recalculate TotalPrice from the selected Tour
                var tour = db.Tours.Find(existing.TourId);
                if (tour != null)
                    existing.TotalPrice = tour.Price * existing.NumberOfPeople;

                db.Entry(existing).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Booking updated.";
                return RedirectToAction("Index");
            }

            PopulateEditSelects(posted);
            return View(existing);
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!CanManage) return new HttpUnauthorizedResult();
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.Bookings
                .Include(b => b.Tour)
                .Include(b => b.Tourist)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null) return HttpNotFound();

            // Fallback hydrate
            if (booking.Tourist == null && booking.TouristId > 0)
                booking.Tourist = db.Users.Find(booking.TouristId);

            if (Role == "Agency")
            {
                var uid = Session["UserId"]?.ToString();
                bool ownsTour = db.Tours.Any(t => t.Id == booking.TourId && t.CreatedByUserId == uid);
                if (!ownsTour) return new HttpUnauthorizedResult();
            }
            if (Role == "Guide")
            {
                var uid = Session["UserId"]?.ToString();
                bool guidesTour = db.Tours.Any(t => t.Id == booking.TourId && t.GuideId == uid);
                if (!guidesTour) return new HttpUnauthorizedResult();
            }

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!CanManage) return new HttpUnauthorizedResult();

            var booking = db.Bookings.Find(id);
            if (booking == null) return HttpNotFound();

            if (Role == "Agency")
            {
                var uid = Session["UserId"]?.ToString();
                bool ownsTour = db.Tours.Any(t => t.Id == booking.TourId && t.CreatedByUserId == uid);
                if (!ownsTour) return new HttpUnauthorizedResult();
            }
            if (Role == "Guide")
            {
                var uid = Session["UserId"]?.ToString();
                bool guidesTour = db.Tours.Any(t => t.Id == booking.TourId && t.GuideId == uid);
                if (!guidesTour) return new HttpUnauthorizedResult();
            }

            db.Bookings.Remove(booking);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Booking deleted.";
            return RedirectToAction("Index");
        }

        // ===== Create actions =====

        // GET: Booking/Create
        public ActionResult Create(int? tourId)
        {
            if (!IsLoggedIn) return RedirectToAction("Login", "Account");

            var booking = new Booking
            {
                BookingDate = DateTime.Now,
                Status = "Pending",
                NumberOfPeople = 1
            };

            if (tourId != null)
            {
                booking.TourId = tourId.Value;
                var tour = db.Tours.Find(tourId.Value);
                if (tour != null)
                {
                    booking.TotalPrice = tour.Price; // final total recalculated on POST
                }
            }

            ViewBag.TourId = new SelectList(db.Tours.OrderBy(t => t.Title), "Id", "Title", booking.TourId);
            ViewBag.AllTours = db.Tours.ToList();

            return View(booking);
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TourId,BookingDate,NumberOfPeople,Status,SpecialRequests")] Booking booking)
        {
            if (!IsLoggedIn) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var tour = db.Tours.Find(booking.TourId);
                if (tour != null)
                    booking.TotalPrice = tour.Price * booking.NumberOfPeople;

                // Always set to the logged-in user
                booking.TouristId = UserIdInt.Value;

                db.Bookings.Add(booking);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Booking created.";
                return RedirectToAction("Index");
            }

            ViewBag.TourId = new SelectList(db.Tours.OrderBy(t => t.Title), "Id", "Title", booking.TourId);
            ViewBag.AllTours = db.Tours.ToList();
            return View(booking);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
