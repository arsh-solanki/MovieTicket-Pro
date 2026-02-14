using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying booking confirmation details.
    /// Used on Booking/Confirmation/{bookingId} page after successful reservation.
    /// </summary>
    public class BookingConfirmationViewModel
    {
        [Display(Name = "Booking ID")]
        public int BookingId { get; set; }

        [Display(Name = "Confirmation Number")]
        public string ConfirmationNumber { get; set; } // Could be formatted as "BOOK-{BookingId:D8}"

        [Display(Name = "Movie Title")]
        [Required]
        [StringLength(200)]
        public string MovieTitle { get; set; }

        [Display(Name = "Theatre")]
        [Required]
        [StringLength(200)]
        public string TheatreName { get; set; }

        [Display(Name = "Screen")]
        [StringLength(100)]
        public string ScreenName { get; set; }

        [Display(Name = "Show Time")]
        [DataType(DataType.DateTime)]
        public DateTime ShowTime { get; set; }

        [Display(Name = "Booked Seats")]
        public List<BookedSeatDetailViewModel> Seats { get; set; } = new List<BookedSeatDetailViewModel>();

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookingDateTime { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } // e.g., "Confirmed", "Pending", etc.

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Usage: In Booking/Confirmation/{bookingId} GET action.
        /// 
        /// Example:
        /// var booking = await _context.Bookings
        ///     .Include(b => b.Show).ThenInclude(s => s.Movie)
        ///     .Include(b => b.Show).ThenInclude(s => s.Screen).ThenInclude(sc => sc.Theatre)
        ///     .Include(b => b.BookedSeats)
        ///     .FirstOrDefaultAsync(b => b.Id == bookingId);
        ///
        /// var vm = new BookingConfirmationViewModel
        /// {
        ///     BookingId = booking.Id,
        ///     ConfirmationNumber = $"BOOK-{booking.Id:D8}",
        ///     MovieTitle = booking.Show.Movie.Title,
        ///     TheatreName = booking.Show.Screen.Theatre.Name,
        ///     ScreenName = booking.Show.Screen.Name,
        ///     ShowTime = booking.Show.StartDateTime,
        ///     Seats = booking.BookedSeats
        ///         .Select(bs => new BookedSeatDetailViewModel
        ///         {
        ///             SeatNumber = bs.SeatNumber,
        ///             Category = bs.Category,
        ///             Price = bs.Price
        ///         })
        ///         .ToList(),
        ///     TotalAmount = booking.TotalAmount,
        ///     BookingDateTime = booking.BookingDateTime,
        ///     Status = booking.Status.ToString(),
        ///     Notes = booking.Notes
        /// };
        /// return View(vm);
        /// </summary>
    }

    /// <summary>
    /// Helper ViewModel for individual booked seat details in BookingConfirmationViewModel.
    /// </summary>
    public class BookedSeatDetailViewModel
    {
        [Display(Name = "Seat Number")]
        [StringLength(10)]
        public string SeatNumber { get; set; } // e.g., "A1", "B12"

        [Display(Name = "Category")]
        [StringLength(50)]
        public string Category { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
    }
}
