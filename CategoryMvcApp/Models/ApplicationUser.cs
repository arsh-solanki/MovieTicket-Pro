using Microsoft.AspNetCore.Identity;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// ApplicationUser – Extends IdentityUser for custom fields.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique identifier, inherited from IdentityUser
    /// - UserName, Email, PhoneNumber: Authentication/contact, inherited
    /// - FirstName: User's given name
    /// - LastName: User's family name
    /// - IsActive: Soft delete flag; false = deactivated user
    /// - Bookings: Navigation to user's ticket bookings
    /// 
    /// Used in booking history, user management, admin role checks.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        /// <summary>
        /// Phone number (overrides inherited from IdentityUser for clarity).
        /// </summary>
        public override string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
