using System.Net;
using System.Net.Mail;
using BLL.Services.Interfaces;
using Microsoft.Extensions.Options;
using SmartAid.API.Settings;

namespace SmartAid.API.Services
{
    public class ZohoEmailNotificationService
        : IEmailNotificationService
    {
        private readonly EmailSettings _settings;

        public ZohoEmailNotificationService(
            IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendNewOfferAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                return;

            using var message = new MailMessage();

            message.From = new MailAddress(_settings.From);
            message.To.Add(recipientEmail);

            message.Subject = "New help request available";
            message.Body =
                "A new help request matching your qualifications " +
                "is available in SmartAid.\n\n" +
                "Please sign in to the SmartAid application " +
                "to view the request.";

            message.IsBodyHtml = false;

            using var client = new SmtpClient(
                _settings.Host,
                _settings.Port)
            {
                Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password),

                EnableSsl = true
            };

            await client.SendMailAsync(
                message,
                cancellationToken);
        }
    }
}