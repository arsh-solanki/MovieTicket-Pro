using System.Collections.Generic;

namespace CategoryMvcApp.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalMovies { get; set; }
        public int TotalShowsToday { get; set; }
        public int TotalBookingsToday { get; set; }
        public decimal TotalRevenueToday { get; set; }
        
        // All-time metrics
        public int TotalBookingsAllTime { get; set; }
        public decimal TotalRevenueAllTime { get; set; }
        
        // Recent data
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        
        // 7-day sales summary
        public List<DailySalesData> DailySales { get; set; } = new List<DailySalesData>();
    }
    
    public class DailySalesData
    {
        public string Date { get; set; }
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
