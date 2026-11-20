using System.Net.Http.Json;
using CREEOT.WEB.Models;

namespace CREEOT.WEB.Services
{
    public class EmailApiClient : IEmailApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailApiClient> _logger;

        public EmailApiClient(HttpClient httpClient, ILogger<EmailApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(SendEmailDto emailDto, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/email/send", emailDto, ct);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email to {Recipient} delivered via API successfully.", emailDto.ToEmail);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Email API rejected request with status {StatusCode}: {Error}", response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call CREEOT.API email endpoint for {Recipient}", emailDto.ToEmail);
                return false;
            }
        }
    }
}