/*
 * ═════════════════════════════════════════════════════════════════════════════════
 * RECOMMENDED OVERRIDE: Register.cshtml.cs – AUTO-ASSIGN "USER" ROLE
 * ═════════════════════════════════════════════════════════════════════════════════
 * 
 * After scaffolding Identity UI with:
 *     dotnet aspnet-codegenerator identity -dc ApplicationDbContext
 * 
 * Edit the scaffolded file: Areas/Identity/Pages/Account/Register.cshtml.cs
 * 
 * Find the OnPostAsync() method and modify it to auto-assign the "User" role to
 * newly registered users. This ensures users can log in and access booking features.
 * 
 * ═════════════════════════════════════════════════════════════════════════════════
 */

// ORIGINAL CODE (in OnPostAsync method, after password validation):

public async Task OnPostAsync(string returnUrl = null)
{
    returnUrl ??= Url.Content("~/");
    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    if (ModelState.IsValid)
    {
        var user = new ApplicationUser 
        { 
            UserName = Input.Email, 
            Email = Input.Email,
            FirstName = Input.Email.Split('@')[0], // Optional: extract name
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");

            // ... existing code (email confirmation, sign-in, etc.)
        }
        // ... rest of method
    }
    // ...
}

// ═════════════════════════════════════════════════════════════════════════════════
// MODIFIED CODE (Add role assignment):
// ═════════════════════════════════════════════════════════════════════════════════

public async Task OnPostAsync(string returnUrl = null)
{
    returnUrl ??= Url.Content("~/");
    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    if (ModelState.IsValid)
    {
        var user = new ApplicationUser 
        { 
            UserName = Input.Email, 
            Email = Input.Email,
            FirstName = Input.Email.Split('@')[0], // Optional: extract first name from email
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            // ═════════════════════════════════════════════════════════════════════
            // AUTO-ASSIGN "USER" ROLE (NEW CODE)
            // ═════════════════════════════════════════════════════════════════════
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            
            if (!roleResult.Succeeded)
            {
                // If role assignment fails, show error
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
            // ═════════════════════════════════════════════════════════════════════

            _logger.LogInformation("User created a new account with password and assigned to 'User' role.");

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
            }
            else
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    // If we got this far, something failed, redisplay form
    return Page();
}

/*
 * ═════════════════════════════════════════════════════════════════════════════════
 * WHAT THIS DOES
 * ═════════════════════════════════════════════════════════════════════════════════
 * 
 * 1. After user account is created successfully
 * 2. Calls _userManager.AddToRoleAsync(user, "User")
 * 3. Assigns the "User" role to the newly registered user
 * 4. If role assignment fails, returns error message
 * 5. If successful, logs the user in and redirects
 * 
 * RESULT:
 * • New users can browse movies and book tickets
 * • They cannot access /Admin/* (requires "Admin" role)
 * • Only admin@movie.com (seeded in Phase 2) has "Admin" role
 * 
 * ═════════════════════════════════════════════════════════════════════════════════
 * REQUIRED USINGS (at top of Register.cshtml.cs file)
 * ═════════════════════════════════════════════════════════════════════════════════
 * 
 * using CategoryMvcApp.Models;
 * using Microsoft.AspNetCore.Identity;
 * using Microsoft.AspNetCore.Mvc;
 * using Microsoft.AspNetCore.Mvc.RazorPages;
 * using System.ComponentModel.DataAnnotations;
 * 
 * These should already be present after scaffolding.
 * 
 * ═════════════════════════════════════════════════════════════════════════════════
 * ALTERNATIVE: Soft-Default Assignment Without Code
 * ═════════════════════════════════════════════════════════════════════════════════
 * 
 * If you don't want to modify Register.cshtml.cs, you can create middleware:
 * 
 * public class RoleAssignmentMiddleware
 * {
 *     private readonly RequestDelegate _next;
 * 
 *     public RoleAssignmentMiddleware(RequestDelegate next)
 *     {
 *         _next = next;
 *     }
 * 
 *     public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
 *     {
 *         if (context.User.Identity?.IsAuthenticated == true)
 *         {
 *             var user = await userManager.GetUserAsync(context.User);
 *             if (user != null && !await userManager.IsInRoleAsync(user, "User") && !await userManager.IsInRoleAsync(user, "Admin"))
 *             {
 *                 await userManager.AddToRoleAsync(user, "User");
 *             }
 *         }
 *         await _next(context);
 *     }
 * }
 * 
 * Then in Program.cs:
 *     app.UseMiddleware<RoleAssignmentMiddleware>();
 * 
 * (Before UseAuthentication)
 * 
 * But modifying Register.cshtml.cs is cleaner and more explicit.
 * 
 * ═════════════════════════════════════════════════════════════════════════════════
 */
