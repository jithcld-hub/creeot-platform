using CREEOT.WEB.DTOs;
using System.Net.Http.Json;
namespace CREEOT.WEB.Services
{
    public interface IAuthApiClient
    {
        Task<SignUpResponseDto> RegisterAsync(SignUpRequestDto request);
    }
}
