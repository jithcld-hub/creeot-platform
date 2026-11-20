using CREEOT.WEB.Data;
using CREEOT.WEB.Models;
using CREEOT.WEB.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace CREEOT.WEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

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

            var emailApiConfig = builder.Configuration.GetSection("EmailApiSettings");

            builder.Services.AddHttpClient<IEmailApiClient, EmailApiClient>(client =>
            {
                client.BaseAddress = new Uri(emailApiConfig["BaseUrl"]!);
                client.DefaultRequestHeaders.Add("X-Api-Key", emailApiConfig["ApiKey"]);
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("X-Api-Key", builder.Configuration["ApiSettings:ApiKey"]!);
            });

            var connectionString = builder.Configuration.GetConnectionString("CREEOTDBConnection") ?? throw new InvalidOperationException("Connection string 'CREEOTDBConnection' not found.");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient<IModashService, ModashService>(client =>
            {
                // Points to the running MODASH API base address
                client.BaseAddress = new Uri("https://api.modash.io/");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", builder.Configuration["Modash:ApiKey"]);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // Register the OData API typed client
            builder.Services.AddHttpClient<IInfluencerApiClient, InfluencerApiClient>(client =>
            {
                // Points to the running OData API base address
                client.BaseAddress = new Uri("http://localhost:6624/");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // Configure the HTTP request pipeline.
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                //DeveloperExceptionPageOptions developerExceptionPageOptions = new DeveloperExceptionPageOptions { SourceCodeLineCount = 10 };
                //app.UseDeveloperExceptionPage(developerExceptionPageOptions);
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //app.MapRazorPages();
            app.Run();
        }
    }
}
