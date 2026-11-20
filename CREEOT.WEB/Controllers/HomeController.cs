using CREEOT.WEB.Models;
using CREEOT.WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using CREEOT.WEB.Data;

namespace CREEOT.WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IInfluencerApiClient _apiClient;
        private readonly AppDbContext _context; 

        public HomeController(ILogger<HomeController> logger, IInfluencerApiClient apiClient, AppDbContext context)
        {
            _logger = logger;
            _apiClient = apiClient;
            _context = context;
        }

        [HttpGet]
        public IActionResult EscrowSecurity()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult HowItWorks()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? targetMarket, string? platform, string? niche, long? budgetRangeId)
        {
            var countries = await _context.Countries.Where(c => c.IsActive).OrderBy(c => c.CountryName).ToListAsync();

            // Default target market to "IN" if not supplied
            var selectedCountryCode = string.IsNullOrWhiteSpace(targetMarket) ? "IN" : targetMarket;

            // Find matching currency to extract its country code (e.g., '₹', 'AED', '$')
            var activeCountry = countries.FirstOrDefault(c => c.CountryCode == selectedCountryCode);
            var activeCurrencySymbol = activeCountry?.CurrencySymbol ?? "₹";

            // 1. Fetch active budget ranges ordered by DisplayOrder
            var ranges = await _context.BudgetRanges
                                .Where(b => b.IsActive)
                                .OrderBy(b => b.DisplayOrder)
                                .ToListAsync();


            // 2. Base query for influencers
            var query = _context.Influencers.Where(i => i.IsActive);

            // Apply country filter
            query = query.Where(i => i.CountryCode == selectedCountryCode);

            // 3. Apply Budget Range filter if user selected one
            if (budgetRangeId.HasValue && budgetRangeId.Value > 0)
            {
                var selectedRange = ranges.FirstOrDefault(r => r.Id == budgetRangeId.Value);
                if (selectedRange != null)
                {
                    if (selectedRange.MinAmount.HasValue)
                    {
                        query = query.Where(i => i.CurrentBasePrice >= selectedRange.MinAmount.Value);
                    }
                    if (selectedRange.MaxAmount.HasValue)
                    {
                        query = query.Where(i => i.CurrentBasePrice <= selectedRange.MaxAmount.Value);
                    }
                }
            }

            var vm = new HomeIndexViewModel
            {
                // Fetch Countries for Target Marketplace
                Countries = countries,

                // Fetch Budget Ranges
                BudgetRanges = ranges,

                // Fetch Platforms
                Platforms = await _context.Platforms
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.PlatformName)
                        .ToListAsync(),

                // Fetch Niches
                Niches = await _context.Niches
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.NicheName)
                        .ToListAsync(),

                // Fetch top trending verified influencers via OData API
                Influencers = await _apiClient.GetTrendingInfluencersAsync(count: 9),

                SelectedCountryCode = selectedCountryCode
            };
            ViewBag.ActiveCurrencySymbol = activeCurrencySymbol;
            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
