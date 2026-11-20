using System.Text.Json;
using CREEOT.WEB.Models;

namespace CREEOT.WEB.Services
{
    public interface IInfluencerApiClient
    {
        Task<List<Influencer>> GetTrendingInfluencersAsync(int count = 6);
        Task<(List<Influencer>Items, int TotalCount)> GetExploreInfluencersAsync(string? country,
            string? platform,
            string? niche,
            int? budgetTier,
            string? searchTerm,
            string? sortBy,
            int page = 1,
            int pageSize = 9);
    }
}
