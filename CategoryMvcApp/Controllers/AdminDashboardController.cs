using CategoryMvcApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CategoryMvcApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Dashboard")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);

            // Today's metrics
            var totalMovies = await _context.Movies.CountAsync(m => m.IsActive);
            var totalShowsToday = await _context.Shows.CountAsync(s => s.StartDateTime.Date == today && s.IsActive);
            var totalBookingsToday = await _context.Bookings.CountAsync(b => b.CreatedOn.Date == today);
            var totalRevenueToday = await _context.Bookings
                .Where(b => b.CreatedOn.Date == today)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0M;

            // All-time metrics
            var totalBookingsAllTime = await _context.Bookings.CountAsync();
            var totalRevenueAllTime = await _context.Bookings
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0M;

            // Recent 10 bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Show)
                    .ThenInclude(s => s.Movie)
                .OrderByDescending(b => b.CreatedOn)
                .Take(10)
                .ToListAsync();

            // 7-day sales summary (last 7 days including today)
            var dailySalesData = new List<DailySalesData>();
            for (int i = 7; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var bookingCount = await _context.Bookings.CountAsync(b => b.CreatedOn.Date == date);
                var revenue = await _context.Bookings
                    .Where(b => b.CreatedOn.Date == date)
                    .SumAsync(b => (decimal?)b.TotalAmount) ?? 0M;

                dailySalesData.Add(new DailySalesData
                {
                    Date = date.ToString("MMM dd"),
                    BookingCount = bookingCount,
                    TotalRevenue = revenue
                });
            }

            var model = new AdminDashboardViewModel
            {
                TotalMovies = totalMovies,
                TotalShowsToday = totalShowsToday,
                TotalBookingsToday = totalBookingsToday,
                TotalRevenueToday = totalRevenueToday,
                TotalBookingsAllTime = totalBookingsAllTime,
                TotalRevenueAllTime = totalRevenueAllTime,
                RecentBookings = recentBookings,
                DailySales = dailySalesData
            };

            return View("~/Views/Admin/Dashboard/Index.cshtml", model);
        }
    }
}
