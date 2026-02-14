using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels.DTOs
{
    /// <summary>
    /// DTO for AJAX POST request to reserve/book seats.
    /// Sent from client (jQuery/AJAX) to Booking/ReserveSeats endpoint.
    /// </summary>
    public class ReserveSeatsRequest
    {
        [Required]
        [Display(Name = "Show ID")]
        public int ShowId { get; set; }

        [Required]
        [Display(Name = "Selected Seat Numbers")]
        [MinLength(1, ErrorMessage = "Please select at least one seat")]
        public List<string> SeatNumbers { get; set; }

        /// <summary>
        /// Usage in AJAX call (jQuery example):
        /// 
        /// var request = {
        ///     showId: @Model.ShowId,
        ///     seatNumbers: ["A1", "A2", "B5"]  // User-selected seats
        /// };
        ///
        /// $.ajax({
        ///     type: "POST",
        ///     url: "/Booking/ReserveSeats",
        ///     contentType: "application/json; charset=utf-8",
        ///     dataType: "json",
        ///     data: JSON.stringify(request),
        ///     success: function(response) {
        ///         if (response.success) {
        ///             window.location.href = "/Booking/Confirmation/" + response.bookingId;
        ///         } else {
        ///             alert("Error: " + response.message);
        ///         }
        ///     },
        ///     error: function(xhr) {
        ///         alert("Request failed: " + xhr.statusText);
        ///     }
        /// });
        /// 
        /// Server-side binding (ASP.NET Core automatically deserializes JSON):
        /// [HttpPost]
        /// public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsRequest request)
        /// {
        ///     // request.ShowId and request.SeatNumbers are automatically populated
        /// }
        /// </summary>
    }
}
