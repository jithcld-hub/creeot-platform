using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using CREEOT.API.Services;
using CREEOT.API.Models;
namespace CREEOT.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(SendEmailRequest request, CancellationToken ct = default)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderName = _configuration["EmailSettings:SenderName"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(request.ToName ?? request.ToEmail, request.ToEmail));
                message.Subject = request.Subject;

                var builder= new BodyBuilder();
                if (request.isHtml)
                {
                    builder.HtmlBody = request.Body;
                }
                else
                {
                    builder.TextBody = request.Body;
                }
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                var secureOption = (smtpPort == 465? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                await client.ConnectAsync(smtpServer, smtpPort, secureOption, ct);

                if(!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                {
                    await client.AuthenticateAsync(smtpUsername, smtpPassword, ct);
                }

                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

                _logger.LogInformation("Email sent to {ToEmail} with subject {Subject}", request.ToEmail, request.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", request.ToEmail);
                throw;
            }
        }
    }
}
