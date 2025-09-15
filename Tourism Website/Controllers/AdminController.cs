// Controllers/AdminController.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Tourism_Website.Data;
using Tourism_Website.Models;

public class AdminController : Controller
{
    private readonly Tourism_WebsiteContext db = new Tourism_WebsiteContext();

    // Simple gate
    private bool IsAdmin()
        => (Session["UserRole"]?.ToString() ?? "") == "Admin";

    // GET: /Admin/PendingRoleRequests
    public ActionResult PendingRoleRequests()
    {
        if (!IsAdmin()) return new HttpUnauthorizedResult();

        var pending = db.Users
            .Where(u => u.RequestedRole != null && u.IsRoleApproved == null)
            .OrderBy(u => u.FullName)
            .ToList();

        return View(pending);
    }

    // POST: /Admin/ApproveRole/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ApproveRole(int id)
    {
        if (!IsAdmin()) return new HttpUnauthorizedResult();

        var user = db.Users.Find(id);
        if (user == null) return HttpNotFound();

        if (string.IsNullOrEmpty(user.RequestedRole))
        {
            TempData["Error"] = "This user has no pending role request.";
            return RedirectToAction("PendingRoleRequests");
        }

        // Approve: set Role to RequestedRole
        user.Role = user.RequestedRole;
        user.IsRoleApproved = true;
        user.RequestedRole = null;

        db.SaveChanges();
        TempData["Success"] = "User upgraded successfully.";
        return RedirectToAction("PendingRoleRequests");
    }

    // POST: /Admin/RejectRole/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult RejectRole(int id)
    {
        if (!IsAdmin()) return new HttpUnauthorizedResult();

        var user = db.Users.Find(id);
        if (user == null) return HttpNotFound();

        if (string.IsNullOrEmpty(user.RequestedRole))
        {
            TempData["Error"] = "This user has no pending role request.";
            return RedirectToAction("PendingRoleRequests");
        }

        // Reject: keep current Role as-is (likely Tourist), mark rejected
        user.IsRoleApproved = false;
        // Optionally clear request so they must re-apply later:
        // user.RequestedRole = null;

        db.SaveChanges();
        TempData["Success"] = "User request rejected.";
        return RedirectToAction("PendingRoleRequests");
    }
}
