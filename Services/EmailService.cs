using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AustimAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpAsync(string toEmail, string code)
        {
            var body = $@"
<div style='font-family:Arial;text-align:center;padding:30px;background:#f9f9f9'>
    <div style='background:white;padding:30px;border-radius:10px;max-width:400px;margin:auto'>
        <h2 style='color:#333'>NeuroNest</h2>
        <p style='color:#666'>كود التحقق بتاعك:</p>
        <div style='background:#4CAF50;color:white;font-size:36px;font-weight:bold;
                    padding:20px;border-radius:8px;letter-spacing:10px;margin:20px 0'>
            {code}
        </div>
        <p style='color:#888'>صالح 15 دقيقة بس</p>
        <p style='color:#bbb;font-size:12px'>لو مطلبتش الكود ده، تجاهل الإيميل</p>
    </div>
</div>";

            var fromEmail = _config["EmailSettings:FromEmail"];
            var appPassword = _config["EmailSettings:AppPassword"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NeuroNest", fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "كود التحقق - NeuroNest";
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(fromEmail, appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

}