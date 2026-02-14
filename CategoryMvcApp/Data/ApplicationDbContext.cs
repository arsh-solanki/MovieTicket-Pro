using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CategoryMvcApp.Models;

namespace CategoryMvcApp.Data
{
    /// <summary>
    /// ApplicationDbContext – Core database context for the Movie Ticket Management System.
    /// 
    /// Extends IdentityDbContext{ApplicationUser} to leverage ASP.NET Core Identity tables:
    /// - AspNetUsers, AspNetUserRoles, AspNetRoles, AspNetRoleClaims, etc.
    /// 
    /// Custom DbSets:
    /// - Movies, Theatres, Screens, Shows, SeatPrices, Bookings, BookedSeats
    /// 
    /// Features:
    /// - Fluent API configuration for relationships, unique indexes, default values
    /// - Seed data: Role creation (Admin, User) and default admin user
    /// - Best practices: async support, parameterized queries, EF Core 8.0+
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Theatre> Theatres { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Show> Shows { get; set; }
        public DbSet<SeatPrice> SeatPrices { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookedSeat> BookedSeats { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ═════════════════════════════════════════════════════════════════════
            // ENTITY CONFIGURATION
            // ═════════════════════════════════════════════════════════════════════

            // ─────────────────────────────────────────────────────────────────────
            // Movie Configuration
            // ─────────────────────────────────────────────────────────────────────
            builder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.DurationMinutes)
                    .IsRequired();

                entity.Property(e => e.ReleaseDate)
                    .IsRequired();

                entity.Property(e => e.PosterUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.TrailerUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Relationship: Movie 1 → N Shows
                entity.HasMany(m => m.Shows)
                    .WithOne(s => s.Movie)
                    .HasForeignKey(s => s.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for common queries
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Title);
            });

            // ─────────────────────────────────────────────────────────────────────
            // Theatre Configuration
            // ─────────────────────────────────────────────────────────────────────
            builder.Entity<Theatre>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Address)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Relationship: Theatre 1 → N Screens
                entity.HasMany(t => t.Screens)
                    .WithOne(s => s.Theatre)
                    .HasForeignKey(s => s.TheatreId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique index: (Name, City) per theatre
                entity.HasIndex(e => new { e.Name, e.City })
                    .IsUnique();

                // Index for soft-delete queries
                entity.HasIndex(e => e.IsActive);
            });

            // ─────────────────────────────────────────────────────────────────────
            // Screen Configuration
            // ─────────────────────────────────────────────────────────────────────
            builder.Entity<Screen>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TotalSeats)
                    .IsRequired();

                entity.Property(e => e.LayoutJson)
                    .HasMaxLength(5000)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Foreign key
                entity.HasOne(s => s.Theatre)
                    .WithMany(t => t.Screens)
                    .HasForeignKey(s => s.TheatreId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Screen 1 → N Shows
                entity.HasMany(s => s.Shows)
                    .WithOne(sh => sh.Screen)
                    .HasForeignKey(sh => sh.ScreenId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique index: (TheatreId, Name) – no duplicate screen names per theatre
                entity.HasIndex(e => new { e.TheatreId, e.Name })
                    .IsUnique();

                entity.HasIndex(e => e.IsActive);
            });

            // ─────────────────────────────────────────────────────────────────────
            // Show Configuration
            // ─────────────────────────────────────────────────────────────────────
            builder.Entity<Show>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.MovieId)
                    .IsRequired();

                entity.Property(e => e.ScreenId)
                    .IsRequired();

                entity.Property(e => e.StartDateTime)
                    .IsRequired();

                entity.Property(e => e.EndDateTime)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Relationships
                entity.HasOne(s => s.Movie)
                    .WithMany(m => m.Shows)
                    .HasForeignKey(s => s.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Screen)
                    .WithMany(sc => sc.Shows)
                    .HasForeignKey(s => s.ScreenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(s => s.SeatPrices)
                    .WithOne(sp => sp.Show)
                    .HasForeignKey(sp => sp.ShowId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(s => s.Bookings)
                    .WithOne(b => b.Show)
                    .HasForeignKey(b => b.ShowId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(s => s.BookedSeats)
                    .WithOne(bs => bs.Show)
                    .HasForeignKey(bs => bs.ShowId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for common queries
                entity.HasIndex(e => new { e.MovieId, e.ScreenId, e.StartDateTime });
                entity.HasIndex(e => e.IsActive);
            });

            // ─────────────────────────────────────────────────────────────────────
            // SeatPrice Configuration
            // ─────────────────────────────────────────────────────────────────────
            builder.Entity<SeatPrice>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ShowId)
                    .IsRequired();

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Foreign key
                entity.HasOne(sp => sp.Show)
                    .WithMany(s => s.SeatPrices)
                    .HasForeignKey(sp => sp.ShowId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique index: (ShowId, Category) – no duplicate categories per show
                entity.HasIndex(e => new { e.ShowId, e.Category })
                    .IsUnique();
            });

            // ─────────────────────────────────────────────────────────────────────
            // Booking Configuration
            // ─────────────────────────────────────────────────────────────────────
            builder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.ShowId)
                    .IsRequired();

                entity.Property(e => e.BookingDateTime)
                    .IsRequired();

                entity.Property(e => e.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(BookingStatus.Pending);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.Notes)
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Foreign keys
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Show)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(b => b.ShowId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship to BookedSeats
                entity.HasMany(b => b.BookedSeats)
                    .WithOne(bs => bs.Booking)
                    .HasForeignKey(bs => bs.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for common queries
                entity.HasIndex(e => new { e.UserId, e.ShowId });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
            });

            // ─────────────────────────────────────────────────────────────────────
            // BookedSeat Configuration
            // ─────────────────────────────────────────────────────────────────────
            builder.Entity<BookedSeat>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.BookingId)
                    .IsRequired();

                entity.Property(e => e.ShowId)
                    .IsRequired();

                entity.Property(e => e.SeatNumber)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                // Foreign keys
                entity.HasOne(bs => bs.Booking)
                    .WithMany(b => b.BookedSeats)
                    .HasForeignKey(bs => bs.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bs => bs.Show)
                    .WithMany(s => s.BookedSeats)
                    .HasForeignKey(bs => bs.ShowId)
                    .OnDelete(DeleteBehavior.Cascade);

                // *** CRITICAL UNIQUE INDEX ***
                // Prevents double-booking: same seat in same show cannot be booked twice
                entity.HasIndex(e => new { e.ShowId, e.SeatNumber })
                    .IsUnique()
                    .HasName("UX_BookedSeat_ShowId_SeatNumber");

                // Additional indexes for performance
                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => e.IsActive);
            });

            // ═════════════════════════════════════════════════════════════════════
            // SEED DATA: ROLES & ADMIN USER
            // ═════════════════════════════════════════════════════════════════════
            SeedRolesAndAdminUser(builder);
        }

        /// <summary>
        /// Seed initial roles and admin user.
        /// Roles: "Admin", "User"
        /// Admin User: admin@movie.com / Admin@123!
        /// </summary>
        private void SeedRolesAndAdminUser(ModelBuilder builder)
        {
            // Create Admin Role
            var adminRole = new IdentityRole
            {
                Id = "admin-role-id",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            // Create User Role
            var userRole = new IdentityRole
            {
                Id = "user-role-id",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            // Seed roles
            builder.Entity<IdentityRole>().HasData(adminRole, userRole);

            // Create Admin User
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

            // Hash password using PasswordHasher
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123!");

            // Seed admin user
            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign Admin role to admin user
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                }
            );
        }
    }
}
