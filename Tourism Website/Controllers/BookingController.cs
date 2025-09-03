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
        private Tourism_WebsiteContext db = new Tourism_WebsiteContext();

        // GET: Booking
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.Tour).Include(b => b.Tourist);
            return View(bookings.ToList());
        }

        // GET: Booking/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.Bookings.Include(b => b.Tour)
                                     .Include(b => b.Tourist)
                                     .FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return HttpNotFound();

            return View(booking);
        }

        // GET: Booking/Create
        public ActionResult Create()
        {
            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title");
            ViewBag.TouristId = new SelectList(db.Users, "Id", "FullName"); // replace with your User model properties
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TourId,TouristId,BookingDate,NumberOfPeople,TotalPrice,Status,SpecialRequests")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", booking.TourId);
            ViewBag.TouristId = new SelectList(db.Users, "Id", "FullName", booking.TouristId);
            return View(booking);
        }

        // GET: Booking/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.Bookings.Find(id);
            if (booking == null)
                return HttpNotFound();

            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", booking.TourId);
            ViewBag.TouristId = new SelectList(db.Users, "Id", "FullName", booking.TouristId);
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TourId,TouristId,BookingDate,NumberOfPeople,TotalPrice,Status,SpecialRequests")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TourId = new SelectList(db.Tours, "Id", "Title", booking.TourId);
            ViewBag.TouristId = new SelectList(db.Users, "Id", "FullName", booking.TouristId);
            return View(booking);
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.Bookings.Include(b => b.Tour)
                                     .Include(b => b.Tourist)
                                     .FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return HttpNotFound();

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
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
