  namespace CREEOT.WEB.Models
{
    public class ExploreViewModel
    {
        public IEnumerable<Influencer> Influencers { get; set; } = new List<Influencer>();
        public IEnumerable<Country> Countries { get; set; } = new List<Country>();
        public IEnumerable<Platform> Platforms { get; set; } = new List<Platform>();
        public IEnumerable<Niche> Niches { get; set; } = new List<Niche>();
        public List<BudgetRange> BudgetRanges { get; set; } = new();
        // Filter Criteria
        public string? targetMarket { get; set; }
        public string? Platform { get; set; }
        public string? Niche { get; set; }
        public long? budgetRangeId { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "views_desc";

        // Pagination State
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
