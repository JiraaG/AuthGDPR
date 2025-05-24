using AuthGDPR.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AuthGDPR.Application.Services
{
    public class EmailCustomSender : IEmailCustomSender
    {
        private readonly IConfiguration _configuration;

        public EmailCustomSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Implementerai ad esempio con SendGrid, SMTP, ecc.
        // Da ricordare che l'api per il conferma dell'email è [HttpGet("confirm-email")] presente in AccountController
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Crea il messaggio email
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(
                _configuration["EmailSettings:SenderName"],
                _configuration["EmailSettings:FromEmail"]));
            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            // Utilizza TurboSMTP tramite SMTP con MailKit
            using (var client = new SmtpClient())
            {
                // Per la produzione, rimuovi o sostituisci la callback con una validazione corretta.
                //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Connetti al server SMTP di TurboSMTP
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

                // Autenticazione richiesta da TurboSMTP
                await client.AuthenticateAsync(
                    _configuration["EmailSettings:SmtpUser"],
                    _configuration["EmailSettings:SmtpPass"]);

                // Invia il messaggio
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }


    }
}
