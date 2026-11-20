using CREEOT.WEB.Data;
using CREEOT.WEB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CREEOT.WEB.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _appDbContext;

        public RegistrationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext appDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appDbContext = appDbContext;
        }

        private async Task PopulateCountriesAsync(RegistrationViewModel model)
        {
            model.AvailableCountries = await _appDbContext.Countries
                .AsNoTracking()
                .OrderBy(c => c.CountryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CountryCode,
                    Text = !string.IsNullOrEmpty(c.PhoneCode)
                        ? $"{c.CountryName} ({c.PhoneCode})"
                        : $"{c.CountryName} ({c.CountryCode})"
                })
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Registration(UserRole? role)
        {
            var model = new RegistrationViewModel
            {
                Role = role ?? UserRole.Client
            };

            await PopulateCountriesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCountriesAsync(model);
                return View(model);
            }

            // Wrap in execution strategy / transaction to prevent orphaned Identity records
            var strategy = _appDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    // 1. Create Base Identity User
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        FullName = model.FullName,
                        UserType = model.Role == UserRole.Client ? "Client" : "Influencer"
                    };

                    var createResult = await _userManager.CreateAsync(user, model.Password);
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await PopulateCountriesAsync(model);
                        return View(model);
                    }

                    // 2. Add Role Claim
                    string roleName = model.Role == UserRole.Client ? "Client" : "Influencer";
                    var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await PopulateCountriesAsync(model);
                        return View(model);
                    }

                    // 3. Create Role-Specific Record
                    if (model.Role == UserRole.Client)
                    {
                        var brand = new Brand
                        {
                            UserId = user.Id,
                            CompanyName = model.CompanyName!,
                            ContactPersonName = model.FullName,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            CountryCode = model.CountryCode,
                            Industry = model.Industry!,
                            WebsiteUrl = model.WebsiteUrl,
                            TaxOrVatNumber = model.TaxOrVatNumber,
                            BillingCurrency = model.CountryCode switch
                            {
                                "AE" => "AED",
                                "QA" => "QAR",
                                _ => "INR"
                            },
                            IsActive = true
                        };
                        _appDbContext.Brands.Add(brand);
                    }
                    else
                    {
                        var names = model.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var initials = names.Length >= 2
                            ? $"{names[0][0]}{names[^1][0]}".ToUpper()
                            : model.FullName.Substring(0, Math.Min(2, model.FullName.Length)).ToUpper();

                        var influencer = new Influencer
                        {
                            UserId = user.Id,
                            FullName = model.FullName,
                            Initials = initials,
                            Email = model.Email,
                            PhoneNumber = model.PhoneNumber,
                            CountryCode = model.CountryCode,
                            PrimaryPlatform = model.PrimaryPlatform!,
                            PrimaryNiche = model.PrimaryNiche!,
                            Handle = model.Handle!.StartsWith("@") ? model.Handle : $"@{model.Handle}",
                            CurrencyCode = model.CountryCode switch
                            {
                                "AE" => "AED",
                                "QA" => "QAR",
                                _ => "INR"
                            },
                            IsActive = true
                        };
                        _appDbContext.Influencers.Add(influencer);
                    }

                    await _appDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Registration successfully completed! Please sign in.";
                    return RedirectToAction(nameof(Login), new { role = roleName });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, $"An error occurred during account creation: {ex.Message}");
                    await PopulateCountriesAsync(model);
                    return View(model);
                }
            });
        }

        // ==========================================
        // Social Logins (Google OAuth Entry Points)
        // ==========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? role, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl, role });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? role = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["ErrorMessage"] = $"Provider authentication error: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Could not load external login information.";
                return RedirectToAction(nameof(Login));
            }

            // Sign in if existing linked user
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            // New User Registration through Google
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "User";

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                UserType = (role == "Influencer") ? "Influencer" : "Client"
            };

            var identityResult = await _userManager.CreateAsync(user);
            if (identityResult.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _userManager.AddToRoleAsync(user, user.UserType);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Send to onboarding profile completion if extra fields are required
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Registration));
        }

        [HttpGet]
        public IActionResult Login(string? role, string? returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                Role = string.Equals(role, "Influencer", StringComparison.OrdinalIgnoreCase)
                    ? UserRole.Influencer
                    : UserRole.Client
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View(model);
            }

            string expectedRole = model.Role == UserRole.Client ? "Client" : "Influencer";
            if (user.UserType != expectedRole)
            {
                ModelState.AddModelError(string.Empty, $"This account is registered as an {user.UserType}. Please switch to the appropriate portal.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["ToastMessage"] = $"Welcome back, {user.FullName ?? user.UserName}!";
                TempData["ToastType"] = "success";

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                if (user.UserType == "Influencer")
                {
                    return RedirectToAction("Dashboard", "Creator");
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account is temporarily locked due to multiple failed login attempts.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["StatusMessage"] = "You have been signed out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            TempData["StatusMessage"] = "You must be signed in as a verified Brand/Client to launch campaigns.";
            return RedirectToAction("Login", "Account");
        }
    }
}