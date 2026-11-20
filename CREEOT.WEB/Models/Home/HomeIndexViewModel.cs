namespace CREEOT.WEB.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Influencer> Influencers { get; set; } = new List<Influencer>();
        public IEnumerable<Country> Countries { get; set; } = new List<Country>();
        public IEnumerable<Platform> Platforms { get; set; } = new List<Platform>();
        public IEnumerable<Niche> Niches { get; set; } = new List<Niche>();
        public List<BudgetRange> BudgetRanges { get; set; } = new();
        public string? SelectedCountryCode { get; set; } = "IN";
        public long? SelectedBudgetRangeId { get; set; }
        public string? SelectedPlatform { get; set; }
        public string? SelectedNiche { get; set; }
    }
}
