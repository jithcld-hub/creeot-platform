using CREEOT.WEB.Models;

namespace CREEOT.WEB.Services
{
    public interface IEmailApiClient
    {
        Task<bool> SendEmailAsync(SendEmailDto emailDto, CancellationToken ct = default);
    }
}