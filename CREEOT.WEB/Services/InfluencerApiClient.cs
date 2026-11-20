using System.Text.Json;
using CREEOT.WEB.Models;

namespace CREEOT.WEB.Services
{
    public class InfluencerApiClient : IInfluencerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public InfluencerApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<Influencer>> GetTrendingInfluencersAsync(int count = 6)
        {
            // OData Query: Filter active trending influencers ordered by rank
            var odataQuery = $"/odata/Influencers?$filter=IsTrending eq true&$orderby=TrendingRank asc&$top={count}";

            var response = await _httpClient.GetAsync(odataQuery);
            if (!response.IsSuccessStatusCode)
                return new List<Influencer>();

            var content = await response.Content.ReadAsStringAsync();
            var odataResult = JsonSerializer.Deserialize<ODataResponse<Influencer>>(content, _jsonOptions);

            return odataResult?.Value ?? new List<Influencer>();
        }

        public async Task<(List<Influencer>Items, int TotalCount)> GetExploreInfluencersAsync(
            string? country,
            string? platform,
            string? niche,
            int? budgetTier,
            string? searchTerm,
            string? sortBy,
            int page = 1,
            int pageSize = 9)
        {
            var filters = new List<string> { "IsActive eq true" };

            if (!string.IsNullOrWhiteSpace(country))
                filters.Add($"CountryCode eq '{country}'");

            if (!string.IsNullOrWhiteSpace(platform))
                filters.Add($"PrimaryPlatform eq '{platform}'");

            if (!string.IsNullOrWhiteSpace(niche))
                filters.Add($"PrimaryNiche eq '{niche}'");

            if (!string.IsNullOrWhiteSpace(searchTerm))
                filters.Add($"(contains(tolower(FullName), '{searchTerm.ToLower()}') or contains(tolower(Handle), '{searchTerm.ToLower()}'))");

            // Budget tier filter mapping
            if (budgetTier.HasValue)
            {
                switch (budgetTier.Value)
                {
                    case 1: filters.Add("CurrentBasePrice lt 10000"); break;
                    case 2: filters.Add("CurrentBasePrice ge 10000 and CurrentBasePrice le 30000"); break;
                    case 3: filters.Add("CurrentBasePrice gt 30000 and CurrentBasePrice le 75000"); break;
                    case 4: filters.Add("CurrentBasePrice gt 75000"); break;
                }
            }

            var orderClause = sortBy switch
            {
                "price_asc" => "$orderby=CurrentBasePrice asc",
                "price_desc" => "$orderby=CurrentBasePrice desc",
                "rating_desc" => "$orderby=CurrentRating desc",
                _ => "$orderby=CurrentAvgViews desc"
            };

            var skip = (page - 1) * pageSize;
            var filterParam = string.Join(" and ", filters);

            // OData request asking for inline count and paginated window
            var url = $"/odata/Influencers?$count=true&$filter={Uri.EscapeDataString(filterParam)}&{orderClause}&$skip={skip}&$top={pageSize}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return (new List<Influencer>(), 0);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ODataResponse<Influencer>>(content, _jsonOptions);

                return (result?.Value ?? new List<Influencer>(), result?.Count ?? 0);
            }
            catch
            {
                return (new List<Influencer>(), 0);
            }
        }
    }
}
