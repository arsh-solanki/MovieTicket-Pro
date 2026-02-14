using CategoryMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CategoryMvcApp.Data
{
    /// <summary>
    /// Sample Data Seeding Service for Development & Testing
    /// Populates database with realistic movie ticket booking data
    /// </summary>
    public class SampleDataSeeding
    {
        public static async Task SeedSampleDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // ─────────────────────────────────────────────────────────
                // 1. Ensure database is created
                // ─────────────────────────────────────────────────────────
                await context.Database.EnsureCreatedAsync();

                // ─────────────────────────────────────────────────────────
                // 2. Seed Roles
                // ─────────────────────────────────────────────────────────
                await SeedRolesAsync(roleManager);

                // ─────────────────────────────────────────────────────────
                // 3. Seed Users
                // ─────────────────────────────────────────────────────────
                await SeedUsersAsync(userManager);

                // ─────────────────────────────────────────────────────────
                // 4. Seed Movies (only if empty)
                // ─────────────────────────────────────────────────────────
                if (!context.Movies.Any())
                {
                    await SeedMoviesAsync(context);
                }

                // ─────────────────────────────────────────────────────────
                // 5. Seed Theatres & Screens
                // ─────────────────────────────────────────────────────────
                if (!context.Theatres.Any())
                {
                    await SeedTheatresAndScreensAsync(context);
                }

                // ─────────────────────────────────────────────────────────
                // 6. Seed Seat Prices
                // ─────────────────────────────────────────────────────────
                if (!context.SeatPrices.Any())
                {
                    await SeedSeatPricesAsync(context);
                }

                // ─────────────────────────────────────────────────────────
                // 7. Seed Shows (Movie Showtimes)
                // ─────────────────────────────────────────────────────────
                if (!context.Shows.Any())
                {
                    await SeedShowsAsync(context);
                }

                Console.WriteLine("✅ Sample data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during seeding: {ex.Message}");
                throw;
            }
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // SEEDING METHODS
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Seed default roles: Admin, User
        /// </summary>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"  ✓ Created role: {roleName}");
                }
            }
        }

        /// <summary>
        /// Seed sample users with different roles
        /// </summary>
        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            // Admin User
            var adminEmail = "admin@movieticket.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "Admin User",
                    PhoneNumber = "+1-800-ADMIN-1",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"  ✓ Created admin user: {adminEmail}");
                }
            }

            // Sample Customer Users
            var sampleCustomers = new[]
            {
                new { Email = "john@example.com", Name = "John Doe", Phone = "+1-555-0101" },
                new { Email = "jane@example.com", Name = "Jane Smith", Phone = "+1-555-0102" },
                new { Email = "bob@example.com", Name = "Bob Wilson", Phone = "+1-555-0103" },
                new { Email = "alice@example.com", Name = "Alice Brown", Phone = "+1-555-0104" }
            };

            foreach (var customer in sampleCustomers)
            {
                if (await userManager.FindByEmailAsync(customer.Email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = customer.Email.Split('@')[0],
                        Email = customer.Email,
                        FullName = customer.Name,
                        PhoneNumber = customer.Phone,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "User@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                        Console.WriteLine($"  ✓ Created user: {customer.Email}");
                    }
                }
            }
        }

        /// <summary>
        /// Seed 5 popular movies with real poster URLs from Unsplash/TMDB
        /// </summary>
        private static async Task SeedMoviesAsync(ApplicationDbContext context)
        {
            var movies = new[]
            {
                new Movie
                {
                    Title = "Inception",
                    Description = "A skilled thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O. Stars Leonardo DiCaprio and Ellen Page in this mind-bending sci-fi masterpiece.",
                    ReleaseDate = new DateTime(2023, 6, 15),
                    DurationMinutes = 148,
                    Genre = "Sci-Fi/Thriller",
                    Director = "Christopher Nolan",
                    PosterUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400&q=80",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Movie
                {
                    Title = "The Dark Knight Rises",
                    Description = "Eight years after the Joker's reign of anarchy, Batman, with the help of the enigmatic Catwoman, is forced from his exile to save Gotham City from the brutal guerrilla terrorist Bane.",
                    ReleaseDate = new DateTime(2023, 7, 20),
                    DurationMinutes = 165,
                    Genre = "Action/Crime",
                    Director = "Christopher Nolan",
                    PosterUrl = "https://images.unsplash.com/photo-1533389752211-118fcffe3312?w=400&q=80",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Movie
                {
                    Title = "Interstellar",
                    Description = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival. Features stunning visuals and an epic musical score by Hans Zimmer.",
                    ReleaseDate = new DateTime(2023, 8, 10),
                    DurationMinutes = 169,
                    Genre = "Sci-Fi/Drama",
                    Director = "Christopher Nolan",
                    PosterUrl = "https://images.unsplash.com/photo-1485846234645-a62644f84728?w=400&q=80",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Movie
                {
                    Title = "The Shawshank Redemption",
                    Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency. A timeless classic of cinema.",
                    ReleaseDate = new DateTime(2023, 9, 5),
                    DurationMinutes = 142,
                    Genre = "Drama",
                    Director = "Frank Darabont",
                    PosterUrl = "https://images.unsplash.com/photo-1517604931442-7e0c6ed2b127?w=400&q=80",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Movie
                {
                    Title = "Pulp Fiction",
                    Description = "The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption. A Tarantino masterpiece.",
                    ReleaseDate = new DateTime(2023, 9, 25),
                    DurationMinutes = 154,
                    Genre = "Crime/Drama",
                    Director = "Quentin Tarantino",
                    PosterUrl = "https://images.unsplash.com/photo-1459749411175-04bf5292ceea?w=400&q=80",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Movies.AddRange(movies);
            await context.SaveChangesAsync();
            Console.WriteLine($"  ✓ Created {movies.Length} movies");
        }

        /// <summary>
        /// Seed theatres and cinema screens
        /// </summary>
        private static async Task SeedTheatresAndScreensAsync(ApplicationDbContext context)
        {
            var theatres = new[]
            {
                new Theatre
                {
                    Name = "PVR Cinemas - Downtown",
                    City = "New York",
                    State = "NY",
                    Address = "1234 Broadway, New York, NY 10001",
                    PhoneNumber = "+1-212-555-0101",
                    TotalScreens = 8,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Screens = new List<Screen>
                    {
                        new Screen
                        {
                            Name = "IMAX Screen 1",
                            TotalRows = 10,
                            SeatsPerRow = 15,
                            ScreenType = "IMAX",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Screen
                        {
                            Name = "Dolby Cinema Screen 2",
                            TotalRows = 10,
                            SeatsPerRow = 15,
                            ScreenType = "Dolby",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                },
                new Theatre
                {
                    Name = "AMC Cinemas - Midtown",
                    City = "New York",
                    State = "NY",
                    Address = "5678 5th Avenue, New York, NY 10022",
                    PhoneNumber = "+1-212-555-0102",
                    TotalScreens = 12,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Screens = new List<Screen>
                    {
                        new Screen
                        {
                            Name = "Standard Screen 1",
                            TotalRows = 10,
                            SeatsPerRow = 15,
                            ScreenType = "Standard",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Screen
                        {
                            Name = "Premium Screen 2",
                            TotalRows = 10,
                            SeatsPerRow = 15,
                            ScreenType = "Premium",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                }
            };

            context.Theatres.AddRange(theatres);
            await context.SaveChangesAsync();
            Console.WriteLine($"  ✓ Created {theatres.Length} theatres with {theatres.Sum(t => t.Screens.Count)} screens");
        }

        /// <summary>
        /// Seed seat pricing categories
        /// Premium: ₹500, Gold: ₹300, Silver: ₹150
        /// </summary>
        private static async Task SeedSeatPricesAsync(ApplicationDbContext context)
        {
            var screenIds = await context.Screens.Select(s => s.Id).ToListAsync();

            foreach (var screenId in screenIds)
            {
                var prices = new[]
                {
                    new SeatPrice
                    {
                        ScreenId = screenId,
                        Category = "Premium",
                        Price = 500,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SeatPrice
                    {
                        ScreenId = screenId,
                        Category = "Gold",
                        Price = 300,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SeatPrice
                    {
                        ScreenId = screenId,
                        Category = "Silver",
                        Price = 150,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.SeatPrices.AddRange(prices);
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"  ✓ Created seat prices for all screens");
        }

        /// <summary>
        /// Seed movie showtimes (Shows)
        /// Creates 4 shows per day for next 7 days (10:00 AM, 1:00 PM, 4:00 PM, 7:00 PM, 10:00 PM)
        /// </summary>
        private static async Task SeedShowsAsync(ApplicationDbContext context)
        {
            var movies = await context.Movies.ToListAsync();
            var screens = await context.Screens.ToListAsync();

            if (!movies.Any() || !screens.Any())
                return;

            var showTimings = new[] { 10, 13, 16, 19, 22 }; // 10 AM, 1 PM, 4 PM, 7 PM, 10 PM
            var startDate = DateTime.UtcNow.Date.AddDays(1); // Start from tomorrow

            foreach (var day in Enumerable.Range(0, 7))
            {
                var showDate = startDate.AddDays(day);

                foreach (var movie in movies)
                {
                    var screen = screens[day % screens.Count];

                    foreach (var hour in showTimings)
                    {
                        var showStartTime = showDate.AddHours(hour);

                        // Skip if show time is in the past
                        if (showStartTime < DateTime.UtcNow)
                            continue;

                        var show = new Show
                        {
                            MovieId = movie.Id,
                            ScreenId = screen.Id,
                            StartDateTime = showStartTime,
                            EndDateTime = showStartTime.AddMinutes(movie.DurationMinutes + 15), // +15 min cleanup
                            ShowFormat = hour == 19 ? "4K" : "2K",
                            Price = 250, // Deprecated, use SeatPrice instead
                            AvailableSeats = screen.TotalRows * screen.SeatsPerRow,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        context.Shows.Add(show);
                    }
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"  ✓ Created shows for next 7 days");
        }
    }
}
