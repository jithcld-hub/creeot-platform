using CREEOT.WEB.Data;
using CREEOT.WEB.DTOs;
using CREEOT.WEB.Models;
using CREEOT.WEB.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace CREEOT.WEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailApiClient _emailClient;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext AppDbContext;
        private readonly IAuthApiClient _authApiClient;
        public AccountController(UserManager<ApplicationUser> userManager, IEmailApiClient emailClient, SignInManager<ApplicationUser> signInManager, AppDbContext appDbContext, IAuthApiClient authApiClient)
        {
            _userManager = userManager;
            _emailClient = emailClient;
            _signInManager = signInManager;
            AppDbContext = appDbContext;
            _authApiClient = authApiClient;
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            TempData["StatusMessage"] = "You must be signed in as a verified Brand/Client to launch campaigns.";
            return RedirectToAction("Login", "Account");
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

            // Enforce logging in through the correct portal role
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
                // Professional welcome message stored for the target page
                TempData["ToastMessage"] = $"Welcome back, {user.FullName ?? user.UserName}!";
                TempData["ToastType"] = "success";
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }
                // Optional: Route Creator vs Client to their respective dashboards
                if (user.UserType == "Influencer")
                {
                    return RedirectToAction("Dashboard", "Creator"); // or Index, Home
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

        private async Task SignInUserAsync(string userId, string name, string email, string role, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, name),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clears the existing authentication cookie
            await _signInManager.SignOutAsync();
            TempData["StatusMessage"] = "You have been signed out successfully.";
            // Redirect to home page or login page after logout
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult SignUp(UserRole? role)
        {
            var model = new SignUpViewModel
            {
                Role = role ?? UserRole.Client
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var requestDto = new SignUpRequestDto
            {
                Role = model.Role,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CountryCode = model.CountryCode,
                CompanyName = model.CompanyName,
                Industry = model.Industry,
                WebsiteUrl = model.WebsiteUrl,
                TaxOrVatNumber = model.TaxOrVatNumber,
                Handle = model.Handle,
                PrimaryPlatform = model.PrimaryPlatform,
                PrimaryNiche = model.PrimaryNiche,
                Password = model.Password
            };

            var apiResult = await _authApiClient.RegisterAsync(requestDto);

            if (!apiResult.Succeeded)
            {
                foreach (var error in apiResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            string roleName = model.Role == UserRole.Client ? "Client" : "Influencer";
            TempData["SuccessMessage"] = "Registration successfully completed! Please log in to continue.";

            return RedirectToAction(nameof(Login), new { role = roleName });
        }
    }
}