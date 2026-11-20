using CREEOT.API.Data;
using CREEOT.API.DTOs;
using CREEOT.API.Models;
using CREEOT.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Runtime.InteropServices;

namespace CREEOT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _appDBContext;
        public AuthController(UserManager<ApplicationUser> userManager, AppDbContext appDBContext)
        {
            this._userManager = userManager;
            this._appDBContext = appDBContext;
        }

        [HttpPost("signup")]
        public async Task<ActionResult<SignUpResponseDto>> SignUp([FromBody] SignUpRequestDto dto)
        {
            if (dto.Role != CREEOT.API.DTOs.UserRole.Influencer && dto.Role != CREEOT.API.DTOs.UserRole.Client)
            {
                return BadRequest(new SignUpResponseDto
                {
                    Succeeded = false,
                    Errors=new List<String> { "Invalid account role specified." }
                });
            }

            string roleName=dto.Role== CREEOT.API.DTOs.UserRole.Client ? "Client" : "Influencer";
            string currency = dto.CountryCode switch
            {
                "AE" => "AED",
                "QA" => "QAR",
                _ => "INR"
            };

            await using var transaction = await _appDBContext.Database.BeginTransactionAsync();
            try {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    FullName = dto.FullName,
                    UserType = roleName,
                    EmailConfirmed = false
                };
                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if(!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new SignUpResponseDto
                    {
                        Succeeded = false,
                        Errors = createResult.Errors.Select(e => e.Description).ToList()
                    });
                }

                var roleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new SignUpResponseDto
                    {
                        Succeeded = false,
                        Errors = roleResult.Errors.Select(e => e.Description).ToList()
                    });
                }

                if (dto.Role == UserRole.Client)
                {
                    var brand = new Brand
                    {
                        UserId = user.Id,
                        CompanyName = dto.CompanyName!,
                        ContactPersonName = dto.FullName,
                        Email = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        CountryCode = dto.CountryCode,
                        Industry = dto.Industry!,
                        WebsiteUrl = dto.WebsiteUrl,
                        TaxOrVatNumber = dto.TaxOrVatNumber,
                        BillingCurrency = currency,
                        IsActive = true
                    };
                    _appDBContext.Brands.Add(brand);
                }
                else
                {
                    var names = dto.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var initials = names.Length >= 2
                        ? $"{names[0][0]}{names[^1][0]}".ToUpper()
                        : dto.FullName.Substring(0, Math.Min(2, dto.FullName.Length)).ToUpper();

                    var influencer = new Influencer
                    {
                        UserId = user.Id,
                        FullName = dto.FullName,
                        Initials = initials,
                        Email = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        CountryCode = dto.CountryCode,
                        PrimaryPlatform = dto.PrimaryPlatform!,
                        PrimaryNiche = dto.PrimaryNiche!,
                        Handle = dto.Handle!.StartsWith("@") ? dto.Handle : $"@{dto.Handle}",
                        CurrencyCode = currency,
                        IsActive = true
                    };
                    _appDBContext.Influencers.Add(influencer);
                }
                await _appDBContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new SignUpResponseDto
                {
                    Succeeded = true,
                    UserId = user.Id,
                    RoleName = roleName
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new SignUpResponseDto
                {
                    Succeeded = false,
                    Errors = new List<string> { "An unexpected error occurred while creating your account." }
                });
            }
        }
    }
}
