
using BLL.Services.Interfaces;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
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

            var message = new MimeMessage();

            message.From.Add(
                MailboxAddress.Parse(_settings.From));

            message.To.Add(
                MailboxAddress.Parse(recipientEmail));

            message.Subject = "New help request available";

            message.Body = new TextPart("plain")
            {
                Text =
                    "A new help request matching your qualifications " +
                    "is available in SmartAid.\n\n" +
                    "Please sign in to the SmartAid application " +
                    "to view the request."
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.SslOnConnect,
                cancellationToken);

            try
            {
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);

                await client.SendAsync(
                    message,
                    cancellationToken);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(
                        true,
                        cancellationToken);
                }
            }
        }
    }
}
