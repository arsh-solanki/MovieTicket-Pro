using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// Show – Represents a specific movie showing/time slot on a screen.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique auto-increment identifier
    /// - MovieId (Foreign Key): Reference to the movie being shown
    /// - ScreenId (Foreign Key): Reference to the screen/auditorium
    /// - StartDateTime: Show start date and time
    /// - EndDateTime: Show end date and time (calculated or stored)
    /// - IsActive: Availability flag (can disable show without deleting)
    /// - CreatedAt: Creation timestamp
    /// - Movie: Navigation to movie details
    /// - Screen: Navigation to screen details
    /// - SeatPrices: Navigation to seat price categories for this show
    /// - Bookings: Navigation to all bookings for this show
    /// 
    /// A show represents a unique time slot. Multiple shows per movie per day are possible.
    /// Seat prices per category are specific to each show.
    /// </summary>
    public class Show
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Movie")]
        public int MovieId { get; set; }

        [ForeignKey("Screen")]
        public int ScreenId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("MovieId")]
        public virtual Movie? Movie { get; set; }

        [ForeignKey("ScreenId")]
        public virtual Screen? Screen { get; set; }

        public virtual ICollection<SeatPrice> SeatPrices { get; set; } = new List<SeatPrice>();

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public virtual ICollection<BookedSeat> BookedSeats { get; set; } = new List<BookedSeat>();
    }
}
