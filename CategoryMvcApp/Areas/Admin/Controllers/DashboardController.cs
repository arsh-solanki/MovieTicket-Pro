using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoryMvcApp.Areas.Admin.Controllers
{
    /// <summary>
    /// DashboardController – Admin dashboard/home for administrative features.
    /// 
    /// Access: /Admin/Dashboard or /Admin
    /// Restriction: [Authorize(Roles = "Admin")] – Only Admin role can access
    /// 
    /// Purpose:
    /// - Display admin welcome message
    /// - Provide links to admin features: Movies, Theatres, Screens, Shows, Bookings, Reports
    /// - Show dashboard statistics/widgets (future: booking count, revenue, etc.)
    /// 
    /// Security:
    /// - Antiforgery tokens on POST actions (built-in with ASP.NET Core MVC)
    /// - Role-based access control via [Authorize]
    /// - HTTPS in production (enforced by middleware)
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        /// <summary>
        /// GET /Admin/Dashboard or /Admin
        /// Displays admin dashboard home page.
        /// </summary>
        public IActionResult Index()
        {
            // TODO: Add statistics/metrics in future phases
            // Examples:
            // - Total bookings count
            // - Total revenue
            // - Movies in system
            // - Upcoming shows
            
            ViewData["Title"] = "Admin Dashboard";
            return View();
        }
    }
}
