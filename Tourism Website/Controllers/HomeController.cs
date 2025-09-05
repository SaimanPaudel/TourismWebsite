using System.Web.Mvc;

namespace Tourism_Website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult TestSession()
        {
            var userRole = Session["UserRole"] as string ?? "Not set";
            var userId = Session["UserId"]?.ToString() ?? "Not set";

            ViewBag.UserRole = userRole;
            ViewBag.UserId = userId;

            return View();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        // GET: Contact
        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // POST: Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(string name, string email, string message)
        {
            if (ModelState.IsValid)
            {
                // TODO: You can save the message or send an email here

                TempData["ThankYouMessage"] = "Thank you for your message! We'll get back to you shortly.";
                return RedirectToAction("Contact");
            }

            // If model state is invalid, return the view for user to fix input
            return View();
        }
    }
}


