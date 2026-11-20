using CREEOT.API.Data;
using CREEOT.API.Services;
using CREEOT.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace CREEOT.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("CREEOTDBConnection")));

            // Register Identity services so UserManager<ApplicationUser> can be injected into AuthController
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password strength rules
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // Unique email
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;

                // Set 6-digit tokens to expire after 10-15 minutes
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // EDM Model Definition
            static IEdmModel GetEdmModel()
            {
                var modelBuilder = new ODataConventionModelBuilder();
                // Exposes /odata/Influencers
                modelBuilder.EntitySet<Influencer>("Influencers");
                // Exposes /odata/InfluencersMetricsSnapshot
                modelBuilder.EntitySet<InfluencerMetricsSnapshot>("InfluencersMetricsSnapshot");
                // Exposes /odata/InfluencersPriceHistory
                modelBuilder.EntitySet<InfluencerPriceHistory>("InfluencersPriceHistory");
                // Exposes /odata/Countries
                modelBuilder.EntitySet<Country>("Countries");
                return modelBuilder.GetEdmModel();
            }

            builder.Services.AddScoped<IEmailService, EmailService>();

            // Register Controllers with OData query features enabled
            builder.Services.AddControllers()
                .AddOData(opt => opt
                    .AddRouteComponents("odata", GetEdmModel())
                    .Select()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Count()
                    .SetMaxTop(100));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure the HTTP request pipeline.
            var app = builder.Build();
            app.UseRouting();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
