using Microsoft.AspNetCore.Mvc;
using CREEOT.API.Services;
using CREEOT.API.Models;

namespace CREEOT.API.Controllers
{
    public class EmailController : ControllerBase
    {
        public readonly IEmailService _emailService;
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request, CancellationToken ct)
        {
            
                if (!ModelState.IsValid)
                { 
                    //return BadRequest(new { message = "Invalid email request" });
                    return BadRequest(ModelState);
                }
            try
            {
                await _emailService.SendAsync(request, ct);
                return Ok(new { success = true, message = "Email queued and sent successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Email delivery error", details = ex.Message });
            }
        }
    }
}
