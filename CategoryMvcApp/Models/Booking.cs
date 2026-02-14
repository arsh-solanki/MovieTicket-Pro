using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// Booking – Represents a ticket booking/reservation by a user.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique auto-increment identifier
    /// - UserId (Foreign Key): Reference to the user who made the booking
    /// - ShowId (Foreign Key): Reference to the show being booked
    /// - BookingDateTime: When the booking was made
    /// - TotalAmount: Total cost for all seats (decimal, 2 decimals)
    /// - Status: Booking status enum (Pending, Confirmed, Refunded, Cancelled)
    /// - IsActive: Soft delete flag
    /// - Notes: Optional booking notes/special requests (max 500 chars)
    /// - CreatedAt: Creation timestamp
    /// - UpdatedAt: Last modification timestamp
    /// - User: Navigation to user who booked
    /// - Show: Navigation to the show
    /// - BookedSeats: Navigation to individual seat reservations
    /// 
    /// Status flow: Pending → Confirmed (after payment) → Refunded/Cancelled (if refund requested).
    /// BookedSeats collection contains all seat details for this booking.
    /// </summary>
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("Show")]
        public int ShowId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime BookingDateTime { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("ShowId")]
        public virtual Show? Show { get; set; }

        public virtual ICollection<BookedSeat> BookedSeats { get; set; } = new List<BookedSeat>();
    }
}
