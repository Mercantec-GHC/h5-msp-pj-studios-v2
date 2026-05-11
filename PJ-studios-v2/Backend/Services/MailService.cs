using System.Net;
using System.Net.Mail;

namespace Backend.Services
{
    public class MailService
    {
        private readonly IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendMail(string to, string subject, string body)
        {
            var host = _config["MailSettings:Host"];
            var port = int.Parse(_config["MailSettings:Port"]);
            var user = _config["MailSettings:User"];
            var pass = _config["MailSettings:Password"];
            var from = _config["MailSettings:From"];

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            var message = new MailMessage(from, to, subject, body);
            await client.SendMailAsync(message);
        }
    }
}
