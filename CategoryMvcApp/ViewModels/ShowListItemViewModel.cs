using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a show/showtime in listings.
    /// Lightweight model used in MovieDetailViewModel.ShowTimes and search results.
    /// </summary>
    public class ShowListItemViewModel
    {
        [Display(Name = "Show ID")]
        public int ShowId { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        [Display(Name = "Screen")]
        [StringLength(100)]
        public string ScreenName { get; set; }

        [Display(Name = "Theatre")]
        [StringLength(200)]
        public string TheatreName { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string City { get; set; }

        [Display(Name = "Price Range")]
        public string PriceRange { get; set; } // e.g., "₹150 – ₹500"

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Usage: Project from Show entity with related Theatre and SeatPrices.
        /// 
        /// Example projection:
        /// new ShowListItemViewModel
        /// {
        ///     ShowId = s.Id,
        ///     StartDateTime = s.StartDateTime,
        ///     EndDateTime = s.EndDateTime,
        ///     ScreenName = s.Screen.Name,
        ///     TheatreName = s.Screen.Theatre.Name,
        ///     City = s.Screen.Theatre.City,
        ///     PriceRange = $"₹{s.SeatPrices.Min(p => p.Price)} – ₹{s.SeatPrices.Max(p => p.Price)}",
        ///     IsActive = s.IsActive
        /// }
        /// </summary>
    }
}
