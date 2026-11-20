using CREEOT.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CREEOT.API.Controllers
{
    public class InfluencersController : ODataController
    {
        private readonly AppDbContext _appDbcontext;   
        public InfluencersController(AppDbContext appDbcontext)
        {
            this._appDbcontext = appDbcontext;
        }

        // GET /odata/Influencers
        // Supports: ?$filter=...&$orderby=...&$top=...&$skip=...&$expand=MetricsSnapshots
        [EnableQuery(PageSize = 50)]
        public IActionResult Get()
        {
            return Ok(_appDbcontext.Influencers.AsNoTracking().Where(x => x.IsActive));
        }

        // GET /odata/Influencers(1)
        [EnableQuery]
        public IActionResult Get([FromRoute] long key)
        {
            var influencer = _appDbcontext.Influencers.Where(x => x.InfluencerId == key);
            if (influencer == null)
            {
                return NotFound();
            }
            return Ok(influencer);
        }

        // GET /odata/Influencers(1)/MetricsSnapshots
        [EnableQuery]
        public IActionResult GetMetricsSnapshots([FromRoute] long key)
        {
            var influencersMetricsSnapshots = _appDbcontext.InfluencersMetricsSnapshots.Where(x => x.InfluencerId == key);
            if (influencersMetricsSnapshots == null)
            {
                return NotFound();
            }
            return Ok(influencersMetricsSnapshots);
        }

        // GET /odata/Influencers(1)/PriceHistories
        [EnableQuery]
        public IActionResult GetPriceHistories([FromRoute] long key)
        {
            var influencerPriceHistories = _appDbcontext.InfluencerPriceHistories.Where(x => x.InfluencerId == key);
            if (influencerPriceHistories == null)
            {
                return NotFound();
            }
            return Ok(influencerPriceHistories);
        }
    }
}
