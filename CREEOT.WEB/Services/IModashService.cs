using CREEOT.WEB.Models;

namespace CREEOT.WEB.Services
{
    public interface IModashService
    {
        Task<(List<Influencer> Items, int TotalCount)> SearchGlobalCreatorsAsync(
            string? country,
            string? platform,
            string? niche,
            int? budgetTier,
            string? searchTerm,
            string? sortBy,
            int page = 1,
            int pageSize = 9);
    }
}
