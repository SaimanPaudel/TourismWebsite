using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Tourism_Website.Data;
using Tourism_Website.Models;

namespace Tourism_Website.Controllers
{
    public class BookingController : Controller
    {
        private Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // GET: Booking
        public ActionResult Index()
        {
            var role = Session["UserRole"]?.ToString();
            var userId = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Booking> bookings = db.Bookings.Include(b => b.Tour).Include(b => b.Tourist);

            switch (role)
            {
                case "Admin":
                    bookings = bookings.OrderByDescending(b => b.BookingDate);
                    break;
                case "Agency":
                    var agencyTourIds = db.Tours.Where(t => t.CreatedByUserId == userId).Select(t => t.Id);
                    bookings = bookings.Where(b => agencyTourIds.Contains(b.TourId)).OrderByDescending(b => b.BookingDate);
                    break;
                case "Guide":
                    var guideTourIds = db.Tours.Where(t => t.GuideId == userId).Select(t => t.Id);
                    bookings = bookings.Where(b => guideTourIds.Contains(b.TourId)).OrderByDescending(b => b.BookingDate);
                    break;
                case "Tourist":
                    if (int.TryParse(userId, out int touristId))
                    {
                        bookings = bookings.Where(b => b.TouristId == touristId).OrderByDescending(b => b.BookingDate);
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    break;
                default:
                    return RedirectToAction("Login", "Account");
            }

            return View(bookings.ToList());
        }

        // GET: Booking/Create
        public ActionResult Create(int? tourId)
        {
            var role = Session["UserRole"]?.ToString();
            var userId = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = new Booking
            {
                BookingDate = DateTime.Now,
                Status = "Pending",
                NumberOfPeople = 1
            };

            if (tourId != null)
            {
                booking.TourId = tourId.Value;
                var tour = db.Tours.Find(tourId);
                if (tour != null)
                {
                    booking.TotalPrice = tour.Price;
                }
            }

            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", booking.TourId);
            ViewBag.AllTours = db.Tours.ToList();

            return View(booking);
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TourId,TouristId,BookingDate,NumberOfPeople,TotalPrice,Status,SpecialRequests")] Booking booking)
        {
            var role = Session["UserRole"]?.ToString();
            var userId = Session["UserId"]?.ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var tour = db.Tours.Find(booking.TourId);
                if (tour != null)
                    booking.TotalPrice = tour.Price * booking.NumberOfPeople;

                booking.TouristId = int.Parse(userId);
                db.Bookings.Add(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", booking.TourId);
            ViewBag.AllTours = db.Tours.ToList();
            return View(booking);
        }

        // Dispose method omitted for brevity...

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
