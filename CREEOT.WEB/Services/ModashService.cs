using CREEOT.WEB.Models;
using CREEOT.WEB.Models.Modash;
using System.Text.Json;

namespace CREEOT.WEB.Services
{
        public class ModashService : IModashService
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger<ModashService> _logger;
            private readonly JsonSerializerOptions _jsonOptions;

        public ModashService(HttpClient httpClient, ILogger<ModashService> logger)
        {
                _httpClient = httpClient;
                _logger = logger;
                _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        public async Task<(List<Influencer> Items, int TotalCount)> SearchGlobalCreatorsAsync(string? country,string? platform,string? niche,int? budgetTier,string? searchTerm,string? sortBy,int page = 1,int pageSize = 9)
        {
            // Normalize platform (Modash uses: instagram, youtube, tiktok)
            var targetPlatform = string.IsNullOrWhiteSpace(platform) ? "instagram" : platform.ToLower();
            var endpoint = $"v1/{targetPlatform}/search";

            // Build request payload for Modash API
            var filterPayload = new Dictionary<string, object>();

            var filterObj = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(country))
            {
                filterObj["country"] = new { code = country.ToUpper() };
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filterObj["query"] = searchTerm;
            }

            // Budget tier to follower/price estimation mapping if needed
            if (budgetTier.HasValue)
            {
                var followersRange = budgetTier.Value switch
                {
                    1 => new { lt = 50000 },
                    2 => new { gte = 50000, lte = 200000 },
                    3 => new { gte = 200000, lte = 500000 },
                    _ => (object)new { gte = 500000 }
                };
                filterObj["followers"] = followersRange;
            }

            var requestBody = new
            {
                filter = filterObj,
                sort = GetModashSortField(sortBy),
                page = page - 1, // Modash pages are 0-indexed
                limit = pageSize
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody);
                if (!response.IsSuccessStatusCode)
                    return (new List<Influencer>(), 0);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ModashSearchResponse>(content, _jsonOptions);

                if (result == null || result.Results == null)
                    return (new List<Influencer>(), 0);

                // Map Modash results to your existing Influencer presentation entity
                var influencers = result.Results.Select(p => new Influencer
                {
                    InfluencerId = Math.Abs(p.UserId?.GetHashCode() ?? Guid.NewGuid().GetHashCode()),
                    FullName = string.IsNullOrWhiteSpace(p.FullName) ? (p.Handle ?? "Unknown") : p.FullName,
                    Handle = p.Handle?.StartsWith("@") == true ? p.Handle : $"@{p.Handle}",
                    Initials = GetInitials(p.FullName ?? p.Handle ?? "CR"),
                    CountryCode = p.Country ?? country ?? "US",
                    PrimaryPlatform = targetPlatform,
                    PrimaryNiche = !string.IsNullOrWhiteSpace(niche) ? char.ToUpper(niche[0]) + niche[1..] : "General",
                    CurrentAvgViews = p.AverageViews > 0 ? p.AverageViews : (long)(p.Followers * 0.12),
                    CurrentFollowers = p.Followers,
                    CurrentRating = 4.8m, // Third-party baseline display score
                    CurrentReviewCount = 0,
                    IsVerified = p.IsVerified,
                    CurrentBasePrice = EstimateBasePrice(p.Followers),
                    CurrencyCode = "USD"
                }).ToList();

                return (influencers, result.Total);
            }
            catch
            {
                return (new List<Influencer>(), 0);
            }
        }
        private static string GetModashSortField(string? sortBy) => sortBy switch
        {
            "views_desc" => "averageViews:desc",
            "rating_desc" => "engagementRate:desc",
            _ => "followers:desc"
        };
        private static string GetInitials(string text)
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return text.Length >= 2 ? text[..2].ToUpper() : text.ToUpper();
        }
        private static decimal EstimateBasePrice(long followers)
        {
            // Fallback estimation metric based on follower band
            return followers switch
            {
                < 20000 => 150m,
                < 100000 => 450m,
                < 500000 => 1200m,
                _ => 3000m
            };
        }
    }
}
