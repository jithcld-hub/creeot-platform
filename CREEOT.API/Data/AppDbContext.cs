using CREEOT.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CREEOT.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)     
        {

        }
        public DbSet<Influencer> Influencers => Set<Influencer>();
        public DbSet<InfluencerMetricsSnapshot> InfluencersMetricsSnapshots => Set<InfluencerMetricsSnapshot>();
        public DbSet<InfluencerPriceHistory> InfluencerPriceHistories => Set<InfluencerPriceHistory>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Country> Countries => Set<Country>();
    }
}
