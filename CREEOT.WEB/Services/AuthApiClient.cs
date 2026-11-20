using CREEOT.WEB.DTOs;

namespace CREEOT.WEB.Services
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        public AuthApiClient(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<SignUpResponseDto> RegisterAsync(SignUpRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/signup", request);
                var result = await response.Content.ReadFromJsonAsync<SignUpResponseDto>();
                return result ?? new SignUpResponseDto
                {
                    Succeeded = false,
                    Errors = new List<string> { "Empty response received from authentication server." }
                };
            }
            catch(Exception ex)
            {
                return new SignUpResponseDto
                {
                    Succeeded = false,
                    Errors = new List<string> { "Unexpected error occured. Please try again later." }
                };

            }
            
        }

    }
}
