using CREEOT.API.Models;

namespace CREEOT.API.Services
{
    public interface IEmailService
    {
        Task SendAsync(SendEmailRequest request, CancellationToken ct =default);
    }
}
