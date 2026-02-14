using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// ViewModel for displaying individual booking items in the booking history.
    /// 
    /// Usage: Used in /Booking/History view to display user's past and upcoming bookings.
    /// 
    /// Features:
    /// • Contains all information needed to display a booking summary
    /// • Shows movie details (title, poster)
    /// • Shows theatre and screen information
    /// • Shows seat details (count, numbers)
    /// • Shows booking status and dates
    /// • Shows total amount paid
    /// 
    /// Data Flow:
    /// BookingController.History() → Query Bookings → Map to BookingHistoryItemViewModel → History.cshtml
    /// </summary>
    public class BookingHistoryItemViewModel
    {
        /// <summary>
        /// Unique booking identifier from database.
        /// Used to generate confirmation number and for linking to booking details.
        /// </summary>
        [Display(Name = "Booking ID")]
        public int BookingId { get; set; }

        /// <summary>
        /// Human-readable confirmation number (format: BOOK-XXXXXXXX).
        /// Example: BOOK-00000047
        /// 
        /// Generated from: $"BOOK-{BookingId:D8}"
        /// Used for customer reference and support inquiries.
        /// </summary>
        [Display(Name = "Confirmation #")]
        public string ConfirmationNumber { get; set; }

        /// <summary>
        /// Movie title for this booking.
        /// Example: "Inception"
        /// </summary>
        [Display(Name = "Movie")]
        public string MovieTitle { get; set; }

        /// <summary>
        /// URL to movie poster image.
        /// Used in history view to display thumbnail.
        /// 
        /// Example: "/images/movies/inception-poster.jpg"
        /// Falls back to default image if not available.
        /// </summary>
        [Display(Name = "Poster")]
        public string MoviePosterUrl { get; set; }

        /// <summary>
        /// Theatre name where movie is showing.
        /// Example: "Cineplex Downtown"
        /// </summary>
        [Display(Name = "Theatre")]
        public string TheatreName { get; set; }

        /// <summary>
        /// Screen/auditorium name within the theatre.
        /// Example: "Screen 3"
        /// </summary>
        [Display(Name = "Screen")]
        public string ScreenName { get; set; }

        /// <summary>
        /// Date and time of the show.
        /// Example: 2024-01-15 19:30:00
        /// 
        /// Used to determine if booking is past or upcoming:
        /// • ShowDateTime > DateTime.Now → Upcoming (can cancel)
        /// • ShowDateTime <= DateTime.Now → Past (completed)
        /// </summary>
        [Display(Name = "Show Date & Time")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy hh:mm tt}")]
        public DateTime ShowDateTime { get; set; }

        /// <summary>
        /// Number of seats booked.
        /// Example: 3
        /// </summary>
        [Display(Name = "Seats")]
        public int SeatCount { get; set; }

        /// <summary>
        /// Comma-separated list of seat numbers.
        /// Example: "A5, A6, A7"
        /// 
        /// Format: "Letter+Number", sorted by row then number.
        /// Used for easy reference of which seats were booked.
        /// </summary>
        [Display(Name = "Seat Numbers")]
        public string SeatNumbers { get; set; }

        /// <summary>
        /// Total amount paid for this booking.
        /// Example: 1200.00 (INR)
        /// 
        /// Calculation: Base price per seat × Number of seats (before any discounts)
        /// Future: Can add discount information if applicable.
        /// </summary>
        [Display(Name = "Amount Paid")]
        [DisplayFormat(DataFormatString = "{0:₹ #,##0.00}")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Current status of the booking.
        /// Possible values: "Confirmed", "Cancelled", "Pending"
        /// 
        /// Affects UI rendering:
        /// • Confirmed + upcoming show → Show cancel button
        /// • Confirmed + past show → Show only view details
        /// • Cancelled → Show cancellation info
        /// </summary>
        [Display(Name = "Status")]
        public string BookingStatus { get; set; }

        /// <summary>
        /// Date and time when the booking was made.
        /// Example: 2024-01-05 14:30:00
        /// 
        /// Used to verify booking age and audit trail.
        /// </summary>
        [Display(Name = "Booked On")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy hh:mm tt}")]
        public DateTime BookedOn { get; set; }

        /// <summary>
        /// Computed property: Check if this booking is for an upcoming show.
        /// 
        /// True if: ShowDateTime > DateTime.Now
        /// Used in view to determine which actions are available:
        /// • Upcoming → Show cancel option
        /// • Past → Hide cancel option
        /// </summary>
        public bool IsUpcoming => ShowDateTime > DateTime.Now;

        /// <summary>
        /// Computed property: Check if booking can be cancelled.
        /// 
        /// Cancellable if:
        /// • Status is "Confirmed"
        /// • Show is upcoming (not yet started)
        /// • Optional: Show is at least 24 hours away (future enhancement)
        /// 
        /// Used in view to show/hide cancel button.
        /// </summary>
        public bool CanCancel => BookingStatus == "Confirmed" && IsUpcoming;
    }
}
