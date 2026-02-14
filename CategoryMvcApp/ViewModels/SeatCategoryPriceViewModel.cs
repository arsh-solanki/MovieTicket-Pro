using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// Simple ViewModel for seat category and price information.
    /// Used in SeatSelectionViewModel and ShowDetailViewModel.
    /// Maps to SeatPrice entity.
    /// </summary>
    public class SeatCategoryPriceViewModel
    {
        [Display(Name = "Seat Category")]
        [Required]
        [StringLength(50)]
        public string Category { get; set; } // e.g., "Premium", "Regular", "Economy"

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        [Range(0, 9999.99)]
        public decimal Price { get; set; }

        /// <summary>
        /// Usage: Project from SeatPrice entity.
        /// 
        /// Example:
        /// var prices = show.SeatPrices
        ///     .Select(p => new SeatCategoryPriceViewModel
        ///     {
        ///         Category = p.Category,
        ///         Price = p.Price
        ///     })
        ///     .ToList();
        /// </summary>
    }
}
