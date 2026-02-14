using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// BookedSeat – Represents a single reserved/booked seat for a show.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique auto-increment identifier
    /// - BookingId (Foreign Key): Reference to the parent booking
    /// - ShowId (Foreign Key): Reference to the show (denormalized for query performance)
    /// - SeatNumber: Seat identifier, e.g. "A12", "C05" (max 10 chars, alphanumeric)
    /// - Category: Seat category at booking time, e.g. "Silver", "Gold" (max 50 chars)
    /// - Price: Price paid for this seat (decimal, 2 decimals)
    /// - IsActive: Soft delete / cancellation flag
    /// - CreatedAt: Creation timestamp
    /// - UpdatedAt: Last modification timestamp
    /// - Booking: Navigation to parent booking
    /// - Show: Navigation to show
    /// 
    /// CRITICAL: Unique constraint on (ShowId, SeatNumber) prevents double-booking.
    /// This table uses a database-level unique index to enforce seat uniqueness per show.
    /// Category and Price are stored for audit trail (prices may change).
    /// </summary>
    public class BookedSeat
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Show ID is required.")]
        [ForeignKey("Show")]
        public int ShowId { get; set; }

        [Required(ErrorMessage = "Seat number is required.")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Seat number must be between 1 and 10 characters.")]
        [RegularExpression(@"^[A-Za-z]+\d+$", ErrorMessage = "Seat number format: Letter(s) + Number (e.g., A12, C05).")]
        public string SeatNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Category must be between 1 and 50 characters.")]
        public string Category { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        [ForeignKey("ShowId")]
        public virtual Show? Show { get; set; }
    }
}
