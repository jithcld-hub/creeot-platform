using CREEOT.WEB.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CREEOT.WEB.Data
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
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignMilestone> CampaignMilestones { get; set; }
        public DbSet<EscrowTransaction> EscrowTransactions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Niche> Niches { get; set; }
        public DbSet<BudgetRange> BudgetRanges { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Configures all AspNet* tables

            // 1-to-1 Relationship: User <-> Brand
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.BrandProfile)
                .WithOne(b => b.User)
                .HasForeignKey<Brand>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-to-1 Relationship: User <-> Influencer
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.InfluencerProfile)
                .WithOne(i => i.User)
                .HasForeignKey<Influencer>(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Platform>(entity =>
            {
                entity.HasIndex(e => e.PlatformCode).IsUnique();
            });

            builder.Entity<Niche>(entity =>
            {
                entity.HasIndex(e => e.NicheCode).IsUnique();
            });
        }
    }
}