/*
 * ═════════════════════════════════════════════════════════════════════════════════
 * PHASE 2 – KEY CODE SNIPPETS & EXAMPLES
 * ═════════════════════════════════════════════════════════════════════════════════
 */

// ─────────────────────────────────────────────────────────────────────────────────
// 1. DOUBLE-BOOKING PREVENTION (BookedSeat Unique Index)
// ─────────────────────────────────────────────────────────────────────────────────

// In DbContext.OnModelCreating:
builder.Entity<BookedSeat>(entity =>
{
    // *** UNIQUE INDEX: Prevents inserting same (ShowId, SeatNumber) twice ***
    entity.HasIndex(e => new { e.ShowId, e.SeatNumber })
        .IsUnique()
        .HasName("UX_BookedSeat_ShowId_SeatNumber");
});

// In Controller (Phase 5 – Booking):
public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsRequest request)
{
    try
    {
        using (var transaction = _db.Database.BeginTransaction())
        {
            // 1. Create Booking record
            var booking = new Booking
            {
                UserId = userId,
                ShowId = request.ShowId,
                BookingDateTime = DateTime.UtcNow,
                TotalAmount = request.TotalAmount,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            // 2. Add BookedSeats (if any duplicate, constraint fails)
            foreach (var seatNumber in request.SelectedSeats)
            {
                var bookedSeat = new BookedSeat
                {
                    BookingId = booking.Id,
                    ShowId = request.ShowId,
                    SeatNumber = seatNumber,
                    Category = "Silver",  // lookup from SeatPrice
                    Price = 300,
                    CreatedAt = DateTime.UtcNow
                };
                _db.BookedSeats.Add(bookedSeat);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { bookingId = booking.Id, status = "Confirmed" });
        }
    }
    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UX_BookedSeat") == true)
    {
        // Unique constraint violated: seat already booked
        return Conflict(new { error = "One or more seats were already booked. Please refresh and try again." });
    }
}

// ─────────────────────────────────────────────────────────────────────────────────
// 2. BOOKING STATUS ENUM (Type-Safe Status Management)
// ─────────────────────────────────────────────────────────────────────────────────

// BookingStatus.cs
public enum BookingStatus
{
    Pending = 0,     // Initial state after user completes booking
    Confirmed = 1,   // After successful payment (Phase not in scope yet)
    Refunded = 2,    // After refund processed
    Cancelled = 3    // After cancellation
}

// In DbContext:
builder.Entity<Booking>(entity =>
{
    entity.Property(e => e.Status)
        .HasConversion<int>()  // Store as int in database
        .HasDefaultValue(BookingStatus.Pending);
});

// In Booking.cs:
public BookingStatus Status { get; set; } = BookingStatus.Pending;

// Usage:
var pendingBookings = await _db.Bookings
    .Where(b => b.Status == BookingStatus.Pending)
    .ToListAsync();

// ─────────────────────────────────────────────────────────────────────────────────
// 3. SOFT DELETE PATTERN (IsActive Flag)
// ─────────────────────────────────────────────────────────────────────────────────

// All entities have: public bool IsActive { get; set; } = true;

// CORRECT: Always filter in queries
var activeMovies = await _db.Movies
    .Where(m => m.IsActive)
    .ToListAsync();

var activeShows = await _db.Shows
    .Where(s => s.IsActive && s.MovieId == movieId)
    .Include(s => s.SeatPrices.Where(sp => /* optional filter */))
    .ToListAsync();

// DEACTIVATE (soft delete):
var movie = await _db.Movies.FindAsync(movieId);
movie.IsActive = false;
movie.UpdatedAt = DateTime.UtcNow;
await _db.SaveChangesAsync();

// FUTURE: Global Query Filter to auto-filter all queries
builder.Entity<Movie>()
    .HasQueryFilter(m => m.IsActive);

// ─────────────────────────────────────────────────────────────────────────────────
// 4. AUDIT TRAIL (CreatedAt & UpdatedAt)
// ─────────────────────────────────────────────────────────────────────────────────

// All main entities have:
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime? UpdatedAt { get; set; }

// In DbContext:
builder.Entity<Movie>(entity =>
{
    entity.Property(e => e.CreatedAt)
        .HasDefaultValueSql("GETUTCDATE()")  // Set by SQL Server on insert
        .ValueGeneratedOnAdd();
});

// When updating:
var movie = await _db.Movies.FindAsync(movieId);
movie.Title = "New Title";
movie.UpdatedAt = DateTime.UtcNow;
await _db.SaveChangesAsync();

// Query sorted by creation date:
var recentBookings = await _db.Bookings
    .Where(b => b.UserId == userId && b.IsActive)
    .OrderByDescending(b => b.CreatedAt)
    .ToListAsync();

// ─────────────────────────────────────────────────────────────────────────────────
// 5. UNIQUE CONSTRAINTS (Data Integrity)
// ─────────────────────────────────────────────────────────────────────────────────

// BookedSeat: (ShowId, SeatNumber) – CRITICAL
builder.Entity<BookedSeat>()
    .HasIndex(e => new { e.ShowId, e.SeatNumber })
    .IsUnique()
    .HasName("UX_BookedSeat_ShowId_SeatNumber");

// SeatPrice: (ShowId, Category)
builder.Entity<SeatPrice>()
    .HasIndex(e => new { e.ShowId, e.Category })
    .IsUnique();

// Screen: (TheatreId, Name)
builder.Entity<Screen>()
    .HasIndex(e => new { e.TheatreId, e.Name })
    .IsUnique();

// Handling constraint violations:
try
{
    await _db.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    if (ex.InnerException?.Message.Contains("UX_") == true)
    {
        // Handle unique constraint violation
        return BadRequest(new { error = "Duplicate entry: this record already exists." });
    }
    throw;
}

// ─────────────────────────────────────────────────────────────────────────────────
// 6. RELATIONSHIPS & NAVIGATION (Eager Loading)
// ─────────────────────────────────────────────────────────────────────────────────

// Get show with movie and all seat prices
var show = await _db.Shows
    .Where(s => s.Id == showId && s.IsActive)
    .Include(s => s.Movie)
    .Include(s => s.SeatPrices)
    .FirstOrDefaultAsync();

// Get booking with user and all booked seats
var booking = await _db.Bookings
    .Where(b => b.Id == bookingId && b.IsActive)
    .Include(b => b.User)
    .Include(b => b.Show)
        .ThenInclude(s => s.Movie)
    .Include(b => b.BookedSeats)
    .FirstOrDefaultAsync();

// Get theatre with all screens and shows
var theatre = await _db.Theatres
    .Where(t => t.Id == theatreId && t.IsActive)
    .Include(t => t.Screens.Where(s => s.IsActive))
        .ThenInclude(s => s.Shows.Where(sh => sh.IsActive))
            .ThenInclude(sh => sh.SeatPrices)
    .FirstOrDefaultAsync();

// ─────────────────────────────────────────────────────────────────────────────────
// 7. SEEDED DATA (Initial Admin User & Roles)
// ─────────────────────────────────────────────────────────────────────────────────

// In DbContext.OnModelCreating:
private void SeedRolesAndAdminUser(ModelBuilder builder)
{
    var adminRole = new IdentityRole
    {
        Id = "admin-role-id",
        Name = "Admin",
        NormalizedName = "ADMIN",
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };

    var userRole = new IdentityRole
    {
        Id = "user-role-id",
        Name = "User",
        NormalizedName = "USER",
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };

    builder.Entity<IdentityRole>().HasData(adminRole, userRole);

    var adminUser = new ApplicationUser
    {
        Id = "admin-user-id",
        UserName = "admin@movie.com",
        NormalizedUserName = "ADMIN@MOVIE.COM",
        Email = "admin@movie.com",
        NormalizedEmail = "ADMIN@MOVIE.COM",
        EmailConfirmed = true,
        PhoneNumber = "+1234567890",
        FirstName = "Admin",
        LastName = "User",
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        ConcurrencyStamp = Guid.NewGuid().ToString()
    };

    var passwordHasher = new PasswordHasher<ApplicationUser>();
    adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123!");

    builder.Entity<ApplicationUser>().HasData(adminUser);

    builder.Entity<IdentityUserRole<string>>().HasData(
        new IdentityUserRole<string>
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        }
    );
}

// ─────────────────────────────────────────────────────────────────────────────────
// 8. PASSWORD POLICY (Identity Configuration)
// ─────────────────────────────────────────────────────────────────────────────────

// In Program.cs:
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;              // e.g., 0-9
    options.Password.RequireNonAlphanumeric = true;    // e.g., !@#$%^&*
    options.Password.RequireUppercase = true;         // e.g., A-Z
    options.Password.RequireLowercase = true;         // e.g., a-z

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;     // Can enable later
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Example valid passwords:
// ✓ Admin@123!
// ✓ Test#User456
// ✓ MyPass@2025

// ─────────────────────────────────────────────────────────────────────────────────
// 9. CONNECTION STRING (appsettings.json)
// ─────────────────────────────────────────────────────────────────────────────────

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MovieTicketDb;Trusted_Connection=true;MultipleActiveResultSets=true;"
  }
}

// For SQL Server (non-local):
// "DefaultConnection": "Server=YOUR_SERVER;Database=MovieTicketDb;User Id=SA;Password=YourPassword;"

// For Azure SQL Database:
// "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=MovieTicketDb;Persist Security Info=False;User ID=admin@yourserver;Password=YourPassword;Encrypt=True;Connection Timeout=30;"

// ─────────────────────────────────────────────────────────────────────────────────
// 10. MIGRATION COMMANDS (Terminal)
// ─────────────────────────────────────────────────────────────────────────────────

// Create initial migration:
dotnet ef migrations add InitialCreate

// Apply migration to database:
dotnet ef database update

// Rollback last migration:
dotnet ef database update PreviousMigrationName

// Remove last migration:
dotnet ef migrations remove

// View all migrations:
dotnet ef migrations list

// Generate SQL script (without executing):
dotnet ef migrations script -o migration.sql

// Drop database (careful!):
dotnet ef database drop

// ═════════════════════════════════════════════════════════════════════════════════
*/
