using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels.DTOs
{
    /// <summary>
    /// DTO for AJAX JSON response from ReserveSeats endpoint.
    /// Returned as JSON from Booking/ReserveSeats POST action.
    /// </summary>
    public class ReserveSeatsResponse
    {
        [Display(Name = "Success")]
        public bool Success { get; set; }

        [Display(Name = "Message")]
        [StringLength(500)]
        public string Message { get; set; }

        [Display(Name = "Booking ID")]
        public int? BookingId { get; set; } // Null if booking failed

        [Display(Name = "Error Details")]
        public string ErrorDetails { get; set; } // Null if success

        /// <summary>
        /// Usage in server action:
        /// 
        /// [HttpPost]
        /// [Authorize] // Only logged-in users can book
        /// public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsRequest request)
        /// {
        ///     try
        ///     {
        ///         // Validate show exists
        ///         var show = await _context.Shows.FindAsync(request.ShowId);
        ///         if (show == null)
        ///         {
        ///             return Json(new ReserveSeatsResponse 
        ///             { 
        ///                 Success = false, 
        ///                 Message = "Show not found",
        ///                 ErrorDetails = $"Show ID {request.ShowId} does not exist"
        ///             });
        ///         }
        ///
        ///         // Check seats availability
        ///         var alreadyBooked = await _context.BookedSeats
        ///             .Where(bs => bs.ShowId == request.ShowId && request.SeatNumbers.Contains(bs.SeatNumber))
        ///             .CountAsync();
        ///
        ///         if (alreadyBooked > 0)
        ///         {
        ///             return Json(new ReserveSeatsResponse
        ///             {
        ///                 Success = false,
        ///                 Message = "Some seats are already booked",
        ///                 ErrorDetails = "Please select different seats"
        ///             });
        ///         }
        ///
        ///         // Create booking (wrapped in transaction for atomicity)
        ///         using (var transaction = _context.Database.BeginTransaction())
        ///         {
        ///             var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        ///             var booking = new Booking
        ///             {
        ///                 UserId = userId,
        ///                 ShowId = request.ShowId,
        ///                 BookingDateTime = DateTime.UtcNow,
        ///                 Status = BookingStatus.Confirmed,
        ///                 IsActive = true
        ///             };
        ///
        ///             _context.Bookings.Add(booking);
        ///             await _context.SaveChangesAsync();
        ///
        ///             // Add booked seats and calculate total
        ///             decimal totalAmount = 0;
        ///             var seatPrices = await _context.SeatPrices.Where(p => p.ShowId == request.ShowId).ToListAsync();
        ///
        ///             foreach (var seatNumber in request.SeatNumbers)
        ///             {
        ///                 var category = DetermineSeatCategory(seatNumber); // Your logic to determine category
        ///                 var price = seatPrices.FirstOrDefault(p => p.Category == category)?.Price ?? 0;
        ///                 totalAmount += price;
        ///
        ///                 var bookedSeat = new BookedSeat
        ///                 {
        ///                     BookingId = booking.Id,
        ///                     ShowId = request.ShowId,
        ///                     SeatNumber = seatNumber,
        ///                     Category = category,
        ///                     Price = price,
        ///                     IsActive = true
        ///                 };
        ///                 _context.BookedSeats.Add(bookedSeat);
        ///             }
        ///
        ///             booking.TotalAmount = totalAmount;
        ///             await _context.SaveChangesAsync();
        ///             await transaction.CommitAsync();
        ///
        ///             return Json(new ReserveSeatsResponse
        ///             {
        ///                 Success = true,
        ///                 Message = "Booking confirmed successfully",
        ///                 BookingId = booking.Id
        ///             });
        ///         }
        ///     }
        ///     catch (Exception ex)
        ///     {
        ///         return Json(new ReserveSeatsResponse
        ///         {
        ///             Success = false,
        ///             Message = "Booking failed",
        ///             ErrorDetails = ex.Message
        ///         });
        ///     }
        /// }
        /// </summary>

        /// <summary>
        /// Usage in client AJAX (jQuery):
        /// 
        /// success: function(response) {
        ///     if (response.success) {
        ///         // Redirect to confirmation page
        ///         window.location.href = "/Booking/Confirmation/" + response.bookingId;
        ///     } else {
        ///         // Show error message
        ///         $("#errorMessage").text(response.message).show();
        ///         console.log(response.errorDetails);
        ///     }
        /// }
        /// </summary>
    }
}
