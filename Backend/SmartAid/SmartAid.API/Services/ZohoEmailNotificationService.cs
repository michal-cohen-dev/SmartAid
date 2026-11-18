
using BLL.Services.Interfaces;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
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

            const string websiteUrl =
                "https://smartaid-client.runasp.net/";

            var message = new MimeMessage();

            message.From.Add(
                MailboxAddress.Parse(_settings.From));

            message.To.Add(
                MailboxAddress.Parse(recipientEmail));

            // Email subject
            message.Subject = "נפתחה קריאה חדשה";

            // Plain text version
            var plainText = """
                שלום!

                נפתחה קריאה חדשה במערכת SmartAid,
                שעשויה להתאים לך.

                אנחנו מזמינים אותך להיכנס למערכת,
                לצפות בקריאות הזמינות ולבחור כיצד לעזור.

                לכניסה למערכת:
                https://smartaid-client.runasp.net/

                תודה,
                צוות SmartAid
                """;

            // HTML version
            var htmlBody = """
                <!DOCTYPE html>
                <html lang="he" dir="rtl">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport"
                          content="width=device-width, initial-scale=1.0">
                    <title>נפתחה קריאה חדשה</title>
                </head>

                <body style="
                    margin: 0;
                    padding: 0;
                    background-color: #f1f5f9;
                    font-family: Arial, Helvetica, sans-serif;
                    direction: rtl;
                    text-align: right;
                ">

                    <table role="presentation"
                           width="100%"
                           cellpadding="0"
                           cellspacing="0"
                           border="0"
                           style="background-color: #f1f5f9; padding: 35px 12px;">

                        <tr>
                            <td align="center">

                                <table role="presentation"
                                       width="100%"
                                       cellpadding="0"
                                       cellspacing="0"
                                       border="0"
                                       style="
                                           max-width: 580px;
                                           background-color: #ffffff;
                                           border-radius: 16px;
                                           overflow: hidden;
                                       ">

                                    <!-- Header -->
                                    <tr>
                                        <td style="
                                            background-color: #123b5d;
                                            padding: 30px 25px;
                                            text-align: center;
                                        ">
                                            <div style="
                                                color: #ffffff;
                                                font-size: 30px;
                                                font-weight: bold;
                                                letter-spacing: 1px;
                                            ">
                                                SmartAid
                                            </div>

                                            <div style="
                                                color: #cbdde9;
                                                font-size: 14px;
                                                margin-top: 8px;
                                            ">
                                                עזרה קטנה. שינוי גדול.
                                            </div>
                                        </td>
                                    </tr>

                                    <!-- Main content -->
                                    <tr>
                                        <td style="
                                            padding: 38px 30px 25px;
                                            color: #243447;
                                            text-align: right;
                                        ">

                                            <div style="
                                                font-size: 32px;
                                                text-align: center;
                                                margin-bottom: 18px;
                                            ">
                                                🔔
                                            </div>

                                            <h1 style="
                                                color: #123b5d;
                                                font-size: 25px;
                                                text-align: center;
                                                margin: 0 0 25px;
                                            ">
                                                נפתחה קריאה חדשה!
                                            </h1>

                                            <p style="
                                                font-size: 17px;
                                                line-height: 1.8;
                                                margin: 0 0 18px;
                                            ">
                                                שלום,
                                            </p>

                                            <p style="
                                                font-size: 16px;
                                                line-height: 1.9;
                                                margin: 0 0 18px;
                                            ">
                                                יש לנו חדשות טובות!
                                                נפתחה במערכת SmartAid
                                                קריאה חדשה שעשויה להתאים
                                                לך.
                                            </p>

                                            <p style="
                                                font-size: 16px;
                                                line-height: 1.9;
                                                margin: 0 0 25px;
                                            ">
                                                זו ההזדמנות שלך להושיט יד,
                                                לסייע למי שזקוק לעזרה
                                                ולהיות חלק מקהילה שעושה טוב.
                                            </p>

                                            <!-- Call to action -->
                                            <table role="presentation"
                                                   width="100%"
                                                   cellpadding="0"
                                                   cellspacing="0"
                                                   border="0">

                                                <tr>
                                                    <td align="center"
                                                        style="padding: 10px 0 25px;">

                                                        <a href="https://smartaid-client.runasp.net/"
                                                           target="_blank"
                                                           style="
                                                               display: inline-block;
                                                               background-color: #0d9488;
                                                               color: #ffffff;
                                                               text-decoration: none;
                                                               font-size: 17px;
                                                               font-weight: bold;
                                                               padding: 16px 35px;
                                                               border-radius: 9px;
                                                           ">
                                                            כניסה למערכת SmartAid
                                                        </a>

                                                    </td>
                                                </tr>
                                            </table>

                                            <p style="
                                                font-size: 14px;
                                                line-height: 1.8;
                                                color: #64748b;
                                                margin: 0;
                                            ">
                                                אם הכפתור אינו פועל,
                                                אפשר להיכנס למערכת
                                                באמצעות הקישור הבא:
                                            </p>

                                            <p style="
                                                text-align: center;
                                                direction: ltr;
                                                word-break: break-all;
                                                font-size: 13px;
                                                margin-top: 12px;
                                            ">
                                                <a href="https://smartaid-client.runasp.net/"
                                                   style="color: #0d9488;">
                                                    https://smartaid-client.runasp.net/
                                                </a>
                                            </p>

                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td style="
                                            background-color: #f8fafc;
                                            border-top: 1px solid #e2e8f0;
                                            padding: 25px;
                                            text-align: center;
                                        ">
                                            <p style="
                                                color: #123b5d;
                                                font-size: 16px;
                                                font-weight: bold;
                                                margin: 0 0 8px;
                                            ">
                                                תודה שאת/ה חלק מ-SmartAid!
                                            </p>

                                            <p style="
                                                color: #64748b;
                                                font-size: 13px;
                                                line-height: 1.7;
                                                margin: 0;
                                            ">
                                                יחד יוצרים קהילה טובה יותר.
                                            </p>
                                        </td>
                                    </tr>

                                </table>

                            </td>
                        </tr>

                    </table>

                </body>
                </html>
                """;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = plainText,
                HtmlBody = htmlBody
            };

            message.Body = bodyBuilder.ToMessageBody();

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
