using System.Net;
using System.Net.Mail;

namespace FinMate.api.Services
{
    public class SmtpEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var fromAddress = _config["Email:FromAddress"];
            var fromName = _config["Email:FromName"] ?? "FinMate";

            var host = _config["Email:Smtp:Host"];
            var port = int.Parse(_config["Email:Smtp:Port"] ?? "587");
            var user = _config["Email:Smtp:User"];
            var pass = _config["Email:Smtp:Password"];
            var enableSsl = bool.Parse(_config["Email:Smtp:EnableSsl"] ?? "true");

            if (string.IsNullOrWhiteSpace(fromAddress)) throw new Exception("Email:FromAddress missing");
            if (string.IsNullOrWhiteSpace(host)) throw new Exception("Email:Smtp:Host missing");
            if (string.IsNullOrWhiteSpace(user)) throw new Exception("Email:Smtp:User missing");
            if (string.IsNullOrWhiteSpace(pass)) throw new Exception("Email:Smtp:Password missing");

            using var message = new MailMessage();
            message.From = new MailAddress(fromAddress, fromName);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = enableSsl
            };

            await client.SendMailAsync(message);
        }
    }
}
