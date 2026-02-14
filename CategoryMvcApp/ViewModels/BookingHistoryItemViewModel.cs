using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying user's booking history.
    /// Used on user profile/booking history page to list all user's past and upcoming bookings.
    /// </summary>
    public class BookingHistoryItemViewModel
    {
        [Display(Name = "Booking ID")]
        public int BookingId { get; set; }

        [Display(Name = "Movie Title")]
        [Required]
        [StringLength(200)]
        public string MovieTitle { get; set; }

        [Display(Name = "Show Time")]
        [DataType(DataType.DateTime)]
        public DateTime ShowTime { get; set; }

        [Display(Name = "Theatre")]
        [StringLength(200)]
        public string TheatreName { get; set; }

        [Display(Name = "Number of Seats")]
        public int SeatCount { get; set; }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Booking Status")]
        public string Status { get; set; } // "Confirmed", "Pending", "Refunded", "Cancelled"

        [Display(Name = "Booked On")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDateTime { get; set; }

        [Display(Name = "Is Upcoming")]
        public bool IsUpcoming { get; set; } // True if ShowTime is in future

        /// <summary>
        /// Usage: In UserProfile/BookingHistory GET action.
        /// 
        /// Example:
        /// var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        /// var bookings = await _context.Bookings
        ///     .Where(b => b.UserId == userId)
        ///     .Include(b => b.Show).ThenInclude(s => s.Movie)
        ///     .Include(b => b.Show).ThenInclude(s => s.Screen).ThenInclude(sc => sc.Theatre)
        ///     .Include(b => b.BookedSeats)
        ///     .OrderByDescending(b => b.BookingDateTime)
        ///     .ToListAsync();
        ///
        /// var vm = bookings.Select(b => new BookingHistoryItemViewModel
        /// {
        ///     BookingId = b.Id,
        ///     MovieTitle = b.Show.Movie.Title,
        ///     ShowTime = b.Show.StartDateTime,
        ///     TheatreName = b.Show.Screen.Theatre.Name,
        ///     SeatCount = b.BookedSeats.Count,
        ///     TotalAmount = b.TotalAmount,
        ///     Status = b.Status.ToString(),
        ///     BookingDateTime = b.BookingDateTime,
        ///     IsUpcoming = b.Show.StartDateTime > DateTime.UtcNow
        /// }).ToList();
        /// </summary>
    }
}
