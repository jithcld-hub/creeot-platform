using CREEOT.WEB.Data;
using CREEOT.WEB.Models;
using CREEOT.WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Globalization;
namespace CREEOT.WEB.Controllers
{
    public class ExploreController : Controller
    {
        private readonly IInfluencerApiClient _apiClient;
        private readonly IModashService _modashService;
        private readonly AppDbContext _context;
        public ExploreController(IInfluencerApiClient apiClient, IModashService modashService, AppDbContext context)
        {
            _apiClient = apiClient;
            _modashService = modashService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? targetMarket, string? platform, string? niche, int? budgetRangeId, string? searchTerm, string? sortBy = "views_desc", int page = 1)
        {
            const int pageSize = 3;
            var (items, totalCount) = await _apiClient.GetExploreInfluencersAsync(targetMarket, platform, niche, budgetRangeId, searchTerm, sortBy, page, pageSize);

            var countries = await _context.Countries.Where(c => c.IsActive).OrderBy(c => c.CountryName).ToListAsync();
            var platforms = await _context.Platforms.Where(c => c.IsActive).OrderBy(c => c.PlatformName).ToListAsync();
            var niches = await _context.Niches.Where(c => c.IsActive).OrderBy(c => c.NicheName).ToListAsync();
            var ranges = await _context.BudgetRanges.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToListAsync();

            var activeCountry = countries.FirstOrDefault(c => c.CountryCode == targetMarket);
            var activeCurrencySymbol = activeCountry?.CurrencySymbol ?? "₹";

            var model = new ExploreViewModel
            {
                Countries = countries,
                Platforms = platforms,
                Niches = niches,
                BudgetRanges = ranges,
                Influencers = items,
                TotalItems = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                targetMarket = targetMarket,
                Platform = platform,
                Niche = niche,
                budgetRangeId = budgetRangeId,
                SearchTerm = searchTerm,
                SortBy = sortBy
            };
            ViewBag.ActiveCurrencySymbol = activeCurrencySymbol;
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Global(string? targetMarket, string? platform, string? niche, int? budgetRangeId, string? searchTerm, string? sortBy = "views_desc", int page = 1)
        {
            const int pageSize = 9;

            var (items, totalCount) = await _modashService.SearchGlobalCreatorsAsync(targetMarket, platform, niche, budgetRangeId, searchTerm, sortBy, page, pageSize);

            var model = new ExploreViewModel
            {
                Influencers = items,
                TotalItems = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                targetMarket = targetMarket,
                Platform = platform,
                Niche = niche,
                budgetRangeId = budgetRangeId,
                SearchTerm = searchTerm,
                SortBy = sortBy
            };
            return View(model);
        }
    }
}