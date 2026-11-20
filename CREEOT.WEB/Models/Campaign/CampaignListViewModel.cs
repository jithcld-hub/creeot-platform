using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class CampaignListViewModel
    {
        public IEnumerable<Campaign> ActiveCampaigns { get; set; } = new List<Campaign>();
        public IEnumerable<Campaign> CompletedCampaigns { get; set; } = new List<Campaign>();
    }
}