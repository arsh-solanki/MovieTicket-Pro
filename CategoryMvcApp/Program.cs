using CategoryMvcApp.Data;
using CategoryMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ═════════════════════════════════════════════════════════════════════════════════
// ADD SERVICES TO THE CONTAINER
// ═════════════════════════════════════════════════════════════════════════════════

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity with roles and password policy
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements (strong security for academic project)
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings (brute-force protection)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // User settings
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();  // ← Adds default Identity pages (Login, Register, Logout, etc.)

// Add authorization policies (optional, for more advanced scenarios)
builder.Services.AddAuthorization(options =>
{
    // Policy: Only Admin role can access
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Policy: Only User role can access
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User"));
});

// Add MVC controllers and views
builder.Services.AddControllersWithViews();

// Add Razor Pages (required for Identity default UI)
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ═════════════════════════════════════════════════════════════════════════════════
// MIDDLEWARE: Authentication & Authorization
// ═════════════════════════════════════════════════════════════════════════════════
// ORDER IS CRITICAL: Authentication must come BEFORE Authorization

app.UseAuthentication();   // ← Identifies the user (who are you?)
app.UseAuthorization();    // ← Authorizes the user (what can you do?)

// ═════════════════════════════════════════════════════════════════════════════════
// ENDPOINT MAPPING
// ═════════════════════════════════════════════════════════════════════════════════

// Map Razor Pages (for Identity pages: /Identity/Account/Login, /Identity/Account/Register)
app.MapRazorPages();

// ═════════════════════════════════════════════════════════════════════════════════
// SEED SAMPLE DATA (Development/Testing Only)
// ═════════════════════════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        await SampleDataSeeding.SeedSampleDataAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ An error occurred while seeding the database.");
    }
}

// Map controller routes with areas support
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapStaticAssets();

app.Run();
