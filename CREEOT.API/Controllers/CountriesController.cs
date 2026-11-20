using CREEOT.API.Data;
using CREEOT.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CREEOT.API.Controllers
{
    public class CountriesController : ODataController
    {
        private readonly AppDbContext _appDbcontext;   
        public CountriesController(AppDbContext appDbcontext)
        {
            this._appDbcontext = appDbcontext;
        }

        // GET /odata/Countries
        [EnableQuery]
        [HttpGet]
        public ActionResult<IQueryable<Country>> Get()
        {
            // Returns IQueryable so OData filters, sorting, and projections run in SQL
            return Ok(_appDbcontext.Countries.AsNoTracking().AsQueryable());
        }

        // GET /odata/Countries(5)
        [EnableQuery]
        [HttpGet]
        public IActionResult Get(int key)
        {
            var country = _appDbcontext.Countries.Find(key);
            if (country == null)
            {
                return NotFound();
            }
            return Ok(country);
        }
    }
}
