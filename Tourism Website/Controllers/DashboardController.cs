using System.Web.Mvc;

namespace Tourism_Website.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult AdminDashboard()
        {
            // Simple role check to secure page
            var role = Session["UserRole"] as string;
            if (role != "Admin")
                return new HttpUnauthorizedResult();

            return View();
        }

        public ActionResult AgencyDashboard()
        {
            var role = Session["UserRole"] as string;
            if (role != "Agency")
                return new HttpUnauthorizedResult();

            return View();
        }

        public ActionResult GuideDashboard()
        {
            var role = Session["UserRole"] as string;
            if (role != "Guide")
                return new HttpUnauthorizedResult();

            return View();
        }

        public ActionResult TouristDashboard()
        {
            var role = Session["UserRole"] as string;
            if (role != "Tourist")
                return new HttpUnauthorizedResult();

            return View();
        }
    }
}
