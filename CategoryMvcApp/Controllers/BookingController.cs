using CategoryMvcApp.Models;
using CategoryMvcApp.ViewModels;
using CategoryMvcApp.ViewModels.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CategoryMvcApp.Controllers
{
    /// <summary>
    /// BookingController: Handles all ticket booking functionality.
    /// Core features:
    /// • GET SelectSeats(showId) → Display seat map with AJAX integration
    /// • GET GetSeatStatus(showId) → JSON API for AJAX (booked seats + prices)
    /// • POST ReserveSeats(request) → AJAX endpoint (create booking in transaction)
    /// • GET Confirmation(bookingId) → Display booking confirmation
    /// 
    /// All actions require [Authorize] (logged-in users only).
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingController> _logger;

        public BookingController(ApplicationDbContext context, ILogger<BookingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Booking/SelectSeats/{showId}
        /// Displays interactive seat selection page for a specific show.
        /// 
        /// Flow:
        /// 1. Load Show with related Movie, Screen, Theatre, SeatPrices, BookedSeats
        /// 2. Map to SeatSelectionViewModel
        /// 3. Pass LayoutJson (theatre layout) and booked seats list to view
        /// 4. View renders seat grid and jQuery handles interactivity
        /// 
        /// @model SeatSelectionViewModel
        /// jQuery AJAX call:
        ///   $.getJSON('/Booking/GetSeatStatus/{showId}', (data) => { mark booked seats })
        /// </summary>
        [HttpGet("SelectSeats/{showId}")]
        public async Task<IActionResult> SelectSeats(int showId)
        {
            try
            {
                // Load show with all necessary related data
                var show = await _context.Shows
                    .Include(s => s.Movie)
                    .Include(s => s.Screen)
                        .ThenInclude(sc => sc.Theatre)
                    .Include(s => s.SeatPrices)
                    .Include(s => s.BookedSeats.Where(bs => bs.IsActive))
                    .FirstOrDefaultAsync(s => s.Id == showId && s.IsActive);

                if (show == null)
                {
                    _logger.LogWarning($"ShowId {showId} not found or inactive");
                    return NotFound("Show not found");
                }

                // Verify show hasn't started
                if (show.StartDateTime <= DateTime.UtcNow)
                {
                    return BadRequest("This show has already started. Cannot book seats.");
                }

                // Map to SeatSelectionViewModel
                var viewModel = new SeatSelectionViewModel
                {
                    ShowId = show.Id,
                    MovieTitle = show.Movie.Title,
                    PosterUrl = show.Movie.PosterUrl,
                    TheatreName = show.Screen.Theatre.Name,
                    ScreenName = show.Screen.Name,
                    StartDateTime = show.StartDateTime,
                    DurationMinutes = show.Movie.DurationMinutes,

                    // Map pricing information
                    Prices = show.SeatPrices
                        .Select(p => new SeatCategoryPriceViewModel
                        {
                            Category = p.Category,
                            Price = p.Price
                        })
                        .ToList(),

                    // List of already booked seats (for frontend to disable)
                    BookedSeats = show.BookedSeats
                        .Where(bs => bs.IsActive)
                        .Select(bs => bs.SeatNumber)
                        .ToList(),

                    // Theatre layout JSON from Screen (format: {"rows":10,"cols":15,"categories":{...}})
                    LayoutJson = show.Screen.LayoutJson ?? "{\"rows\":10,\"cols\":15}",

                    // Parse rows/columns if simple grid
                    Rows = 10,  // Default; could parse from LayoutJson in production
                    Columns = 15
                };

                _logger.LogInformation($"SelectSeats loaded for ShowId {showId}, {viewModel.BookedSeats.Count} seats already booked");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SelectSeats: {ex.Message}");
                return StatusCode(500, "An error occurred while loading seats");
            }
        }

        /// <summary>
        /// GET: /Booking/GetSeatStatus/{showId}
        /// AJAX endpoint: Returns JSON with booked seats and pricing information.
        /// Called by jQuery on page load to populate seat map.
        /// 
        /// Response format:
        /// {
        ///     success: true,
        ///     bookedSeats: ["A1", "A2", "B5"],
        ///     categories: { "Premium": 300, "Regular": 200, "Economy": 150 },
        ///     message: "Data loaded successfully"
        /// }
        /// 
        /// jQuery usage:
        /// $.getJSON('/Booking/GetSeatStatus/5', function(data) {
        ///     if (data.success) {
        ///         data.bookedSeats.forEach(seat => {
        ///             $(`[data-seat="${seat}"]`).addClass('booked').attr('disabled', true);
        ///         });
        ///     }
        /// });
        /// </summary>
        [HttpGet("GetSeatStatus/{showId}")]
        [AllowAnonymous]  // Allow anonymous to load seat status (no user data exposed)
        public async Task<IActionResult> GetSeatStatus(int showId)
        {
            try
            {
                var show = await _context.Shows
                    .Include(s => s.SeatPrices)
                    .Include(s => s.BookedSeats.Where(bs => bs.IsActive))
                    .FirstOrDefaultAsync(s => s.Id == showId && s.IsActive);

                if (show == null)
                {
                    return Json(new
                    {
                        success = false,
                        bookedSeats = new List<string>(),
                        categories = new Dictionary<string, decimal>(),
                        message = "Show not found"
                    });
                }

                // Get list of booked seat numbers
                var bookedSeats = show.BookedSeats
                    .Where(bs => bs.IsActive)
                    .Select(bs => bs.SeatNumber)
                    .ToList();

                // Get pricing by category
                var categories = show.SeatPrices
                    .ToDictionary(p => p.Category, p => p.Price);

                return Json(new
                {
                    success = true,
                    bookedSeats = bookedSeats,
                    categories = categories,
                    message = "Seat status loaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetSeatStatus: {ex.Message}");
                return Json(new
                {
                    success = false,
                    bookedSeats = new List<string>(),
                    categories = new Dictionary<string, decimal>(),
                    message = "Error loading seat status"
                });
            }
        }

        /// <summary>
        /// POST: /Booking/ReserveSeats
        /// AJAX endpoint: Creates booking with selected seats in a database transaction.
        /// 
        /// Request (JSON):
        /// {
        ///     "showId": 5,
        ///     "seatNumbers": ["A1", "A2", "B3"]
        /// }
        /// 
        /// Response on success:
        /// {
        ///     "success": true,
        ///     "message": "Booking confirmed successfully",
        ///     "bookingId": 42
        /// }
        /// 
        /// Response on error:
        /// {
        ///     "success": false,
        ///     "message": "Some seats already booked",
        ///     "errorDetails": "Seats A1, A2 are no longer available"
        /// }
        /// 
        /// CRITICAL FEATURES:
        /// ✓ Database transaction for atomicity (prevent double-booking race condition)
        /// ✓ Unique index on BookedSeat(ShowId, SeatNumber) prevents DB-level duplicates
        /// ✓ Verify user is logged in via [Authorize]
        /// ✓ Check seat availability before booking
        /// ✓ Calculate TotalAmount from SeatPrice entities
        /// ✓ Soft delete pattern (IsActive = true)
        /// </summary>
        [HttpPost("ReserveSeats")]
        [ValidateAntiForgeryToken]  // CSRF protection for POST from AJAX
        public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsRequest request)
        {
            try
            {
                // Validate request
                if (request == null || request.ShowId <= 0 || request.SeatNumbers == null || request.SeatNumbers.Count == 0)
                {
                    return Json(new ReserveSeatsResponse
                    {
                        Success = false,
                        Message = "Invalid request",
                        ErrorDetails = "ShowId and at least one seat number required"
                    });
                }

                // Get current user ID
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new ReserveSeatsResponse
                    {
                        Success = false,
                        Message = "Authentication required",
                        ErrorDetails = "User not authenticated"
                    });
                }

                // Load show with pricing and booked seats
                var show = await _context.Shows
                    .Include(s => s.SeatPrices)
                    .Include(s => s.BookedSeats.Where(bs => bs.IsActive))
                    .FirstOrDefaultAsync(s => s.Id == request.ShowId && s.IsActive);

                if (show == null)
                {
                    return Json(new ReserveSeatsResponse
                    {
                        Success = false,
                        Message = "Show not found",
                        ErrorDetails = $"ShowId {request.ShowId} does not exist"
                    });
                }

                // Check if show has already started
                if (show.StartDateTime <= DateTime.UtcNow)
                {
                    return Json(new ReserveSeatsResponse
                    {
                        Success = false,
                        Message = "Show has already started",
                        ErrorDetails = "Cannot book seats for shows that have started"
                    });
                }

                // CRITICAL: Check seat availability (race condition protection)
                var alreadyBooked = show.BookedSeats
                    .Where(bs => request.SeatNumbers.Contains(bs.SeatNumber))
                    .Select(bs => bs.SeatNumber)
                    .ToList();

                if (alreadyBooked.Any())
                {
                    return Json(new ReserveSeatsResponse
                    {
                        Success = false,
                        Message = "Some seats are already booked",
                        ErrorDetails = $"Seats {string.Join(", ", alreadyBooked)} are no longer available. Please select different seats."
                    });
                }

                // Start transaction for atomicity
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Create Booking record
                        var booking = new Booking
                        {
                            UserId = userId,
                            ShowId = request.ShowId,
                            BookingDateTime = DateTime.UtcNow,
                            Status = BookingStatus.Confirmed,
                            IsActive = true,
                            Notes = null
                        };

                        _context.Bookings.Add(booking);
                        await _context.SaveChangesAsync();

                        // Create BookedSeat records for each selected seat
                        decimal totalAmount = 0;
                        var bookedSeats = new List<BookedSeat>();

                        foreach (var seatNumber in request.SeatNumbers)
                        {
                            // Determine seat category (simple logic: first letter = category)
                            // In production, use theatre layout to determine category
                            string category = DetermineSeatCategory(seatNumber, show.Screen.LayoutJson);

                            // Find price for this category
                            var seatPrice = show.SeatPrices
                                .FirstOrDefault(p => p.Category == category);

                            if (seatPrice == null)
                            {
                                // If no price found, use default or return error
                                _logger.LogWarning($"No price found for category {category} in show {request.ShowId}");
                                seatPrice = show.SeatPrices.FirstOrDefault() ?? new SeatPrice { Price = 0 };
                            }

                            var bookedSeat = new BookedSeat
                            {
                                BookingId = booking.Id,
                                ShowId = request.ShowId,
                                SeatNumber = seatNumber,
                                Category = category,
                                Price = seatPrice.Price,
                                IsActive = true
                            };

                            bookedSeats.Add(bookedSeat);
                            totalAmount += seatPrice.Price;

                            _context.BookedSeats.Add(bookedSeat);
                        }

                        // Update booking total amount
                        booking.TotalAmount = totalAmount;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Booking created: BookingId={booking.Id}, UserId={userId}, ShowId={request.ShowId}, Seats={string.Join(",", request.SeatNumbers)}, Total=₹{totalAmount}");

                        return Json(new ReserveSeatsResponse
                        {
                            Success = true,
                            Message = "Booking confirmed successfully",
                            BookingId = booking.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError($"Error during booking transaction: {ex.Message}");
                        return Json(new ReserveSeatsResponse
                        {
                            Success = false,
                            Message = "Booking failed",
                            ErrorDetails = ex.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ReserveSeats: {ex.Message}");
                return Json(new ReserveSeatsResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// GET: /Booking/Confirmation/{bookingId}
        /// Displays booking confirmation with ticket details.
        /// Verifies user owns the booking (security check).
        /// 
        /// @model BookingConfirmationViewModel
        /// Shows: Booking ID, Confirmation #, Movie, Theatre, Seats, Total, Booking Date
        /// </summary>
        [HttpGet("Confirmation/{bookingId}")]
        public async Task<IActionResult> Confirmation(int bookingId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Load booking with all related data
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.Show)
                        .ThenInclude(s => s.Movie)
                    .Include(b => b.Show)
                        .ThenInclude(s => s.Screen)
                            .ThenInclude(sc => sc.Theatre)
                    .Include(b => b.BookedSeats)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.IsActive);

                if (booking == null)
                {
                    _logger.LogWarning($"Booking {bookingId} not found or inactive");
                    return NotFound("Booking not found");
                }

                // Security: Verify user owns this booking
                if (booking.UserId != userId)
                {
                    _logger.LogWarning($"Unauthorized access attempt to booking {bookingId} by user {userId}");
                    return Forbid();
                }

                // Map to BookingConfirmationViewModel
                var viewModel = new BookingConfirmationViewModel
                {
                    BookingId = booking.Id,
                    ConfirmationNumber = $"BOOK-{booking.Id:D8}",
                    MovieTitle = booking.Show.Movie.Title,
                    TheatreName = booking.Show.Screen.Theatre.Name,
                    ScreenName = booking.Show.Screen.Name,
                    ShowTime = booking.Show.StartDateTime,
                    Seats = booking.BookedSeats
                        .Where(bs => bs.IsActive)
                        .Select(bs => new BookedSeatDetailViewModel
                        {
                            SeatNumber = bs.SeatNumber,
                            Category = bs.Category,
                            Price = bs.Price
                        })
                        .ToList(),
                    TotalAmount = booking.TotalAmount,
                    BookingDateTime = booking.BookingDateTime,
                    Status = booking.Status.ToString(),
                    Notes = booking.Notes
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Confirmation: {ex.Message}");
                return StatusCode(500, "An error occurred while loading confirmation");
            }
        }

        /// <summary>
        /// GET: /Booking/History
        /// Displays user's booking history (past and upcoming bookings).
        /// 
        /// Features:
        /// • Shows all user bookings sorted by show date (descending)
        /// • Separates upcoming vs past bookings
        /// • Shows booking details: movie, theatre, seats, amount, status
        /// • Allows navigation to confirmation page for details
        /// 
        /// @model List<BookingHistoryItemViewModel>
        /// </summary>
        [HttpGet("History")]
        public async Task<IActionResult> History()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                // Load user's bookings with all related data
                var bookings = await _context.Bookings
                    .Where(b => b.UserId == userId && b.IsActive)
                    .Include(b => b.Show)
                        .ThenInclude(s => s.Movie)
                    .Include(b => b.Show)
                        .ThenInclude(s => s.Screen)
                            .ThenInclude(sc => sc.Theatre)
                    .Include(b => b.BookedSeats.Where(bs => bs.IsActive))
                    .OrderByDescending(b => b.Show.StartDateTime)
                    .ToListAsync();

                // Map to ViewModels
                var bookingHistory = bookings.Select(b => new BookingHistoryItemViewModel
                {
                    BookingId = b.Id,
                    ConfirmationNumber = $"BOOK-{b.Id:D8}",
                    MovieTitle = b.Show.Movie.Title,
                    MoviePosterUrl = b.Show.Movie.PosterUrl,
                    TheatreName = b.Show.Screen.Theatre.Name,
                    ScreenName = b.Show.Screen.Name,
                    ShowDateTime = b.Show.StartDateTime,
                    SeatCount = b.BookedSeats.Count(),
                    SeatNumbers = string.Join(", ", b.BookedSeats.OrderBy(bs => bs.SeatNumber).Select(bs => bs.SeatNumber)),
                    TotalAmount = b.TotalAmount,
                    BookingStatus = b.Status.ToString(),
                    BookedOn = b.BookingDateTime
                }).ToList();

                return View(bookingHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in History: {ex.Message}");
                return StatusCode(500, "An error occurred while loading booking history");
            }
        }

        /// <summary>
        /// Helper method: Determine seat category based on seat number.
        /// Simple implementation: Extract letter prefix (A=Premium, B=Regular, C=Economy, etc.)
        /// 
        /// In production, parse LayoutJson to get actual category mapping from theatre configuration.
        /// 
        /// Example layout JSON:
        /// {
        ///     "rows": [
        ///         {"letter": "A", "category": "Premium", "count": 15},
        ///         {"letter": "B", "category": "Regular", "count": 15},
        ///         {"letter": "C", "category": "Economy", "count": 15}
        ///     ]
        /// }
        /// </summary>
        private string DetermineSeatCategory(string seatNumber, string layoutJson)
        {
            // Simple implementation: use first letter as row identifier
            // A-C rows: Premium, Regular, Economy (example mapping)
            if (string.IsNullOrEmpty(seatNumber) || seatNumber.Length < 1)
                return "Regular";

            char rowLetter = char.ToUpper(seatNumber[0]);

            // Simple category mapping (can be extended based on layoutJson)
            return rowLetter switch
            {
                'A' => "Premium",
                'B' => "Regular",
                'C' => "Economy",
                _ => "Regular"
            };
        }
    }
}
