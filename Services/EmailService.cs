namespace MusicApp.Services;

public interface IEmailService
{
    Task<bool> SendResetPasswordEmailAsync(string toEmail, string username, string resetLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string username, string resetLink)
    {
        var host = _config["Mail:Host"];
        var port = int.TryParse(_config["Mail:Port"], out var p) ? p : 587;
        var user = _config["Mail:Username"];
        var pass = _config["Mail:Password"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            return false;

        try
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);

            if (!string.IsNullOrEmpty(pass))
                await client.AuthenticateAsync(user, pass);

            var body = $@"
 Xin chào {username},

 Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản trên underground.fm.

 Nhấn vào link bên dưới để đặt lại mật khẩu:
 {resetLink}

 Link sẽ hết hạn sau 30 phút. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.
            ".Trim();

            var msg = new MimeKit.MimeMessage();
            msg.From.Add(new MimeKit.MailboxAddress("underground.fm", user));
            msg.To.Add(new MimeKit.MailboxAddress(username, toEmail));
            msg.Subject = "Đặt lại mật khẩu — underground.fm";
            msg.Body = new MimeKit.TextPart("plain") { Text = body };

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}