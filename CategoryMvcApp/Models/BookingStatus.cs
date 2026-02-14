using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// Enum for booking status lifecycle.
    /// Pending → Confirmed (after successful payment) → Refunded/Cancelled (if refund requested)
    /// </summary>
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Refunded = 2,
        Cancelled = 3
    }
}
