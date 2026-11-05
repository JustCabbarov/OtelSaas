using System.Text;
using System.Text.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using HotelQrSaaS.Application.Interfaces.Services;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Infrastructure.Notifications;

public interface IEmailNotificationSender
{
    Task<bool> SendInquiryEmailAsync(Tenant tenant, GuestInquiry inquiry, CancellationToken cancellationToken = default);
    Task<bool> SendOrderEmailAsync(Tenant tenant, GuestOrder order, CancellationToken cancellationToken = default);
    Task<bool> SendReviewEmailAsync(Tenant tenant, GuestReview review, CancellationToken cancellationToken = default);
    Task<bool> SendTestEmailAsync(Tenant tenant, string targetEmail, CancellationToken cancellationToken = default);
}

public interface ITelegramNotificationSender
{
    Task<bool> SendInquiryTelegramAsync(Tenant tenant, GuestInquiry inquiry, CancellationToken cancellationToken = default);
    Task<bool> SendOrderTelegramAsync(Tenant tenant, GuestOrder order, CancellationToken cancellationToken = default);
    Task<bool> SendReviewTelegramAsync(Tenant tenant, GuestReview review, CancellationToken cancellationToken = default);
    Task<bool> SendTestTelegramAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<(bool Success, string? BotUsername, string? Error)> ValidateBotTokenAsync(string botToken, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ChatId, string? ChatTitle, string? Error)> DetectChatIdAsync(string botToken, CancellationToken cancellationToken = default);
    Task<bool> SendDirectMessageAsync(string botToken, string chatId, string message, CancellationToken cancellationToken = default);
}

public class EmailNotificationSender : IEmailNotificationSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(IConfiguration configuration, ILogger<EmailNotificationSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendInquiryEmailAsync(Tenant tenant, GuestInquiry inquiry, CancellationToken cancellationToken = default)
    {
        var targetEmail = !string.IsNullOrWhiteSpace(tenant.NotificationEmail) ? tenant.NotificationEmail : tenant.Email;
        if (string.IsNullOrWhiteSpace(targetEmail))
        {
            _logger.LogWarning("Otel ({HotelName}) üçün bildiriş e-poçt ünvanı təyin edilməyib.", tenant.Name);
            return false;
        }

        var subject = $"🔔 Yeni Qonaq Müraciəti — {tenant.Name} (Otaq: {inquiry.RoomNumber ?? "Təyin edilməyib"})";
        var body = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                    <h2 style="color: #191a1a; border-bottom: 2px solid #e0e0e0; padding-bottom: 10px;">Yeni Qonaq Müraciəti</h2>
                    <p style="font-size: 15px; color: #555;"><strong>Otel:</strong> {tenant.Name}</p>
                    <p style="font-size: 15px; color: #555;"><strong>Qonağın Adı:</strong> {inquiry.FullName}</p>
                    <p style="font-size: 15px; color: #555;"><strong>Telefon:</strong> <a href="tel:{inquiry.PhoneNumber}">{inquiry.PhoneNumber}</a></p>
                    <p style="font-size: 15px; color: #555;"><strong>E-poçt:</strong> {inquiry.Email ?? "Göstərilməyib"}</p>
                    <p style="font-size: 15px; color: #555;"><strong>Otaq Nömrəsi:</strong> {inquiry.RoomNumber ?? "Yoxdur"}</p>
                    <div style="background: #f9f9f9; padding: 15px; border-left: 4px solid #191a1a; margin-top: 20px;">
                        <p style="margin: 0; font-size: 14px; color: #333;"><strong>Mesaj:</strong><br/>{inquiry.Message}</p>
                    </div>
                    <p style="font-size: 12px; color: #999; margin-top: 30px; text-align: center;">Bu bildiriş Hotel QR SaaS Platforması tərəfindən avtomatik göndərilib.</p>
                </div>
            </body>
            </html>
            """;

        var emails = targetEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return await SendRawEmailAsync(tenant, emails, subject, body, cancellationToken);
    }

    public async Task<bool> SendTestEmailAsync(Tenant tenant, string targetEmail, CancellationToken cancellationToken = default)
    {
        var subject = $"🧪 Sınaq Bildirişi — {tenant.Name}";
        var body = $"Salam! Bu, {tenant.Name} üçün Hotel QR sisteminin e-poçt bildiriş kanalının sınaq mesajıdır. Sistem uğurla işləyir!";
        var emails = targetEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return await SendRawEmailAsync(tenant, emails, subject, body, cancellationToken);
    }

    public async Task<bool> SendOrderEmailAsync(Tenant tenant, GuestOrder order, CancellationToken cancellationToken = default)
    {
        var targetEmail = !string.IsNullOrWhiteSpace(tenant.NotificationEmail) ? tenant.NotificationEmail : tenant.Email;
        if (string.IsNullOrWhiteSpace(targetEmail)) return false;

        var itemsHtml = string.Join("", order.Items.Select(i =>
            $"<tr><td style='padding:8px;border-bottom:1px solid #eee;'>{i.ItemTitle}</td><td style='padding:8px;border-bottom:1px solid #eee;text-align:center;'>x{i.Quantity}</td><td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>{i.LineTotal:0.00} {order.Currency}</td></tr>"));

        var subject = $"🍽️ Yeni Sifariş #{order.OrderNumber} — {tenant.Name} (Otaq/Masa: {order.RoomOrTableNumber})";
        var body = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background: #ffffff; padding: 30px; border-radius: 8px;">
                    <h2 style="color: #2A1D14; border-bottom: 2px solid #B8925A; padding-bottom: 10px;">Yeni Səbət Sifarişi (#{order.OrderNumber})</h2>
                    <p><strong>Otel:</strong> {tenant.Name}</p>
                    <p><strong>Sifariş növü:</strong> {order.OrderType}</p>
                    <p><strong>Otaq / Masa:</strong> {order.RoomOrTableNumber}</p>
                    <p><strong>Qonaq:</strong> {order.GuestName} (<a href="tel:{order.PhoneNumber}">{order.PhoneNumber}</a>)</p>
                    <table style="width:100%; border-collapse: collapse; margin-top: 15px;">
                        <thead>
                            <tr style="background:#f9f5ee;"><th style="padding:8px;text-align:left;">Məhsul</th><th style="padding:8px;">Say</th><th style="padding:8px;text-align:right;">Məbləğ</th></tr>
                        </thead>
                        <tbody>{itemsHtml}</tbody>
                    </table>
                    <h3 style="text-align:right; color:#9A7444;">Cəmi: {order.TotalAmount:0.00} {order.Currency}</h3>
                    {(string.IsNullOrWhiteSpace(order.Notes) ? "" : $"<p style='background:#f9f9f9;padding:10px;'><strong>Qeyd:</strong> {order.Notes}</p>")}
                </div>
            </body>
            </html>
            """;

        var emails = targetEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return await SendRawEmailAsync(tenant, emails, subject, body, cancellationToken);
    }

    public async Task<bool> SendReviewEmailAsync(Tenant tenant, GuestReview review, CancellationToken cancellationToken = default)
    {
        var targetEmail = !string.IsNullOrWhiteSpace(tenant.NotificationEmail) ? tenant.NotificationEmail : tenant.Email;
        if (string.IsNullOrWhiteSpace(targetEmail)) return false;

        var stars = new string('★', Math.Clamp(review.Rating, 1, 5)) + new string('☆', 5 - Math.Clamp(review.Rating, 1, 5));
        var subject = $"{(review.Rating <= 3 ? "⚠️ DİQQƏT: Mənfi Qonaq Rəyi" : "⭐ Yeni Qonaq Rəyi")} ({review.Rating}/5) — {tenant.Name}";
        var body = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background: #ffffff; padding: 25px; border-radius: 8px; border: 1px solid #ddd;">
                    <h2>Yeni Qonaq Qiymətləndirməsi: {stars} ({review.Rating}/5)</h2>
                    <p><strong>Otel:</strong> {tenant.Name}</p>
                    <p><strong>Qonaq:</strong> {review.GuestName ?? "Anonim"}</p>
                    <p><strong>Otaq:</strong> {review.RoomNumber ?? "Göstərilməyib"}</p>
                    <p><strong>Telefon:</strong> {review.PhoneNumber ?? "Göstərilməyib"}</p>
                    <p><strong>Şöbə:</strong> {review.Department ?? "Ümumi"}</p>
                    <p><strong>Şərh:</strong><br/>{review.Comment ?? "Şərh yazılmayıb."}</p>
                </div>
            </body>
            </html>
            """;

        var emails = targetEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return await SendRawEmailAsync(tenant, emails, subject, body, cancellationToken);
    }

    private async Task<bool> SendRawEmailAsync(Tenant tenant, string[] toEmails, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = !string.IsNullOrWhiteSpace(tenant.CustomSmtpHost) ? tenant.CustomSmtpHost : (smtpSettings["Host"] ?? "smtp.gmail.com");
            var port = tenant.CustomSmtpPort ?? (int.TryParse(smtpSettings["Port"], out var p) ? p : 587);
            var userName = !string.IsNullOrWhiteSpace(tenant.CustomSmtpUser) ? tenant.CustomSmtpUser : (smtpSettings["UserName"] ?? "");
            var password = !string.IsNullOrWhiteSpace(tenant.CustomSmtpPassword) ? tenant.CustomSmtpPassword : (smtpSettings["Password"] ?? "");
            var fromEmail = userName.Length > 0 ? userName : (smtpSettings["FromEmail"] ?? "noreply@hotelqr.az");
            var fromName = tenant.Name;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogInformation("SMTP tənzimləmələri tam daxil edilməyib (Simulyasiya edilir): {Subject} to {ToEmails}", subject, string.Join(", ", toEmails));
                return true;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            foreach (var email in toEmails)
            {
                message.To.Add(new MailboxAddress("", email));
            }
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(userName, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-poçt göndərilərkən xəta baş verdi: {Message}", ex.Message);
            return false;
        }
    }
}

public class TelegramNotificationSender : ITelegramNotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramNotificationSender> _logger;

    public TelegramNotificationSender(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramNotificationSender> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendInquiryTelegramAsync(Tenant tenant, GuestInquiry inquiry, CancellationToken cancellationToken = default)
    {
        var botToken = !string.IsNullOrWhiteSpace(tenant.TelegramBotToken) 
            ? tenant.TelegramBotToken 
            : _configuration["Telegram:BotToken"];

        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(tenant.TelegramChatId))
        {
            _logger.LogWarning("Otel ({HotelName}) üçün Telegram Bot Token və ya Chat ID təyin edilməyib.", tenant.Name);
            return false;
        }

        var message = $"""
            🔔 *Yeni Qonaq Müraciəti!*
            🏨 *Otel:* {EscapeMarkdown(tenant.Name)}
            👤 *Qonaq:* {EscapeMarkdown(inquiry.FullName)}
            📞 *Telefon:* `{inquiry.PhoneNumber}`
            {(string.IsNullOrWhiteSpace(inquiry.Email) ? "" : $"📧 *E-poçt:* `{inquiry.Email}`\n")}{(string.IsNullOrWhiteSpace(inquiry.RoomNumber) ? "" : $"🚪 *Otaq:* `{inquiry.RoomNumber}`\n")}
            💬 *Mesaj:*
            _{EscapeMarkdown(inquiry.Message)}_
            """;

        return await SendDirectMessageAsync(botToken, tenant.TelegramChatId, message, cancellationToken);
    }

    public async Task<bool> SendTestTelegramAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var botToken = !string.IsNullOrWhiteSpace(tenant.TelegramBotToken) 
            ? tenant.TelegramBotToken 
            : _configuration["Telegram:BotToken"];

        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(tenant.TelegramChatId))
        {
            return false;
        }

        var message = $"🧪 *Sınaq Bildirişi!* \n\nSalam! Bu, *{EscapeMarkdown(tenant.Name)}* üçün Telegram bildiriş botunun sınaq mesajıdır. Sistem uğurla qoşuldu!";
        return await SendDirectMessageAsync(botToken, tenant.TelegramChatId, message, cancellationToken);
    }

    public async Task<bool> SendOrderTelegramAsync(Tenant tenant, GuestOrder order, CancellationToken cancellationToken = default)
    {
        var botToken = !string.IsNullOrWhiteSpace(tenant.TelegramBotToken)
            ? tenant.TelegramBotToken
            : _configuration["Telegram:BotToken"];

        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(tenant.TelegramChatId))
        {
            return false;
        }

        var lines = string.Join("\n", order.Items.Select(i =>
            $"  • {EscapeMarkdown(i.ItemTitle)} x{i.Quantity} — *{i.LineTotal:0.00} {order.Currency}*"));

        var message = $"""
            🍽️ *Yeni Səbət Sifarişi!* (`#{order.OrderNumber}`)
            🏨 *Otel:* {EscapeMarkdown(tenant.Name)}
            📍 *Növ:* {order.OrderType}
            🚪 *Otaq / Masa:* `{EscapeMarkdown(order.RoomOrTableNumber)}`
            👤 *Qonaq:* {EscapeMarkdown(order.GuestName)}
            📞 *Telefon:* `{order.PhoneNumber}`

            🛒 *Sifariş Siyahısı:*
            {lines}

            💰 *Cəmi Məbləğ:* *{order.TotalAmount:0.00} {order.Currency}*
            {(string.IsNullOrWhiteSpace(order.Notes) ? "" : $"\n📝 *Qeyd:* _{EscapeMarkdown(order.Notes)}_")}
            """;

        return await SendDirectMessageAsync(botToken, tenant.TelegramChatId, message, cancellationToken);
    }

    public async Task<bool> SendReviewTelegramAsync(Tenant tenant, GuestReview review, CancellationToken cancellationToken = default)
    {
        var botToken = !string.IsNullOrWhiteSpace(tenant.TelegramBotToken)
            ? tenant.TelegramBotToken
            : _configuration["Telegram:BotToken"];

        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(tenant.TelegramChatId))
        {
            return false;
        }

        var stars = new string('⭐', Math.Clamp(review.Rating, 1, 5));
        var header = review.Rating <= 3
            ? "⚠️ *TƏCİLİ: Mənfi Qonaq Rəyi Daxil Oldu!*"
            : "🌟 *Yeni Qonaq Rəyi!*";

        var message = $"""
            {header}
            🏨 *Otel:* {EscapeMarkdown(tenant.Name)}
            📊 *Qiymət:* {stars} ({review.Rating}/5)
            👤 *Qonaq:* {EscapeMarkdown(review.GuestName ?? "Anonim")}
            {(string.IsNullOrWhiteSpace(review.RoomNumber) ? "" : $"🚪 *Otaq:* `{EscapeMarkdown(review.RoomNumber)}`\n")}{(string.IsNullOrWhiteSpace(review.PhoneNumber) ? "" : $"📞 *Telefon:* `{review.PhoneNumber}`\n")}{(string.IsNullOrWhiteSpace(review.Department) ? "" : $"🏢 *Şöbə:* {EscapeMarkdown(review.Department)}\n")}
            💬 *Rəy:*
            _{EscapeMarkdown(review.Comment ?? "Şərh yazılmayıb.")}_
            """;

        return await SendDirectMessageAsync(botToken, tenant.TelegramChatId, message, cancellationToken);
    }

    public async Task<(bool Success, string? BotUsername, string? Error)> ValidateBotTokenAsync(string botToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = $"https://api.telegram.org/bot{botToken.Trim()}/getMe";
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, null, "Daxil edilən Bot Token yanlışdır və ya Telegram tərəfindən rədd edildi.");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("result", out var result) &&
                result.TryGetProperty("username", out var username))
            {
                return (true, username.GetString(), null);
            }

            return (false, null, "Bot məlumatları alına bilmədi.");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? ChatId, string? ChatTitle, string? Error)> DetectChatIdAsync(string botToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = $"https://api.telegram.org/bot{botToken.Trim()}/getUpdates";
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, null, null, "Bot yeniləmələri alına bilmədi.");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("result", out var result) || result.GetArrayLength() == 0)
            {
                return (false, null, null, "Hələ heç bir mesaj tapılmadı. Zəhmət olmasa Telegram-da botunuza daxil olub '/start' göndərin və ya botu qəbul qrupunuza əlavə edin.");
            }

            // Get the latest message or chat
            var updates = result.EnumerateArray().Reverse();
            foreach (var update in updates)
            {
                if (update.TryGetProperty("message", out var message) && message.TryGetProperty("chat", out var chat))
                {
                    var chatId = chat.GetProperty("id").ToString();
                    string? chatTitle = null;
                    if (chat.TryGetProperty("title", out var title))
                    {
                        chatTitle = title.GetString();
                    }
                    else if (chat.TryGetProperty("first_name", out var fn))
                    {
                        chatTitle = fn.GetString();
                    }

                    return (true, chatId, chatTitle, null);
                }
                else if (update.TryGetProperty("my_chat_member", out var chatMember) && chatMember.TryGetProperty("chat", out var groupChat))
                {
                    var chatId = groupChat.GetProperty("id").ToString();
                    string? title = groupChat.TryGetProperty("title", out var t) ? t.GetString() : "Qrup";
                    return (true, chatId, title, null);
                }
            }

            return (false, null, null, "Aktiv çat ID tapılmadı.");
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }

    public async Task<bool> SendDirectMessageAsync(string botToken, string chatId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUri = $"https://api.telegram.org/bot{botToken.Trim()}/sendMessage";
            var payload = new
            {
                chat_id = chatId.Trim(),
                text = message,
                parse_mode = "Markdown"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(requestUri, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Telegram API xətası ({StatusCode}): {Body}", response.StatusCode, body);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram göndərilərkən xəta: {Message}", ex.Message);
            return false;
        }
    }

    private static string EscapeMarkdown(string text)
    {
        return text.Replace("_", "\\_").Replace("*", "\\*").Replace("[", "\\[").Replace("`", "\\`");
    }
}

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEmailNotificationSender _emailSender;
    private readonly ITelegramNotificationSender _telegramSender;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IEmailNotificationSender emailSender,
        ITelegramNotificationSender telegramSender,
        ILogger<NotificationDispatcher> logger)
    {
        _emailSender = emailSender;
        _telegramSender = telegramSender;
        _logger = logger;
    }

    public async Task<bool> DispatchInquiryNotificationAsync(Tenant tenant, GuestInquiry inquiry, CancellationToken cancellationToken = default)
    {
        var channel = tenant.PreferredNotificationChannel;
        var emailSuccess = false;
        var telegramSuccess = false;

        if (channel == NotificationChannel.None)
        {
            return true;
        }

        if (channel is NotificationChannel.Email or NotificationChannel.Both)
        {
            emailSuccess = await _emailSender.SendInquiryEmailAsync(tenant, inquiry, cancellationToken);
            inquiry.IsEmailSent = emailSuccess;
        }

        if (channel is NotificationChannel.Telegram or NotificationChannel.Both)
        {
            telegramSuccess = await _telegramSender.SendInquiryTelegramAsync(tenant, inquiry, cancellationToken);
            inquiry.IsTelegramSent = telegramSuccess;
        }

        return emailSuccess || telegramSuccess;
    }

    public async Task<bool> DispatchOrderNotificationAsync(Tenant tenant, GuestOrder order, CancellationToken cancellationToken = default)
    {
        var channel = tenant.PreferredNotificationChannel;
        if (channel == NotificationChannel.None) return true;

        var emailSuccess = false;
        var telegramSuccess = false;

        if (channel is NotificationChannel.Email or NotificationChannel.Both)
        {
            emailSuccess = await _emailSender.SendOrderEmailAsync(tenant, order, cancellationToken);
            order.IsEmailSent = emailSuccess;
        }

        if (channel is NotificationChannel.Telegram or NotificationChannel.Both)
        {
            telegramSuccess = await _telegramSender.SendOrderTelegramAsync(tenant, order, cancellationToken);
            order.IsTelegramSent = telegramSuccess;
        }

        return emailSuccess || telegramSuccess;
    }

    public async Task<bool> DispatchReviewNotificationAsync(Tenant tenant, GuestReview review, CancellationToken cancellationToken = default)
    {
        var channel = tenant.PreferredNotificationChannel;
        if (channel == NotificationChannel.None) return true;

        var emailSuccess = false;
        var telegramSuccess = false;

        if (channel is NotificationChannel.Email or NotificationChannel.Both)
        {
            emailSuccess = await _emailSender.SendReviewEmailAsync(tenant, review, cancellationToken);
        }

        if (channel is NotificationChannel.Telegram or NotificationChannel.Both)
        {
            telegramSuccess = await _telegramSender.SendReviewTelegramAsync(tenant, review, cancellationToken);
            review.IsTelegramAlertSent = telegramSuccess;
        }

        return emailSuccess || telegramSuccess;
    }

    public async Task<bool> SendTestNotificationAsync(Tenant tenant, NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        var emailResult = true;
        var telegramResult = true;

        if (channel is NotificationChannel.Email or NotificationChannel.Both)
        {
            var targetEmail = !string.IsNullOrWhiteSpace(tenant.NotificationEmail) ? tenant.NotificationEmail : tenant.Email;
            if (!string.IsNullOrWhiteSpace(targetEmail))
            {
                emailResult = await _emailSender.SendTestEmailAsync(tenant, targetEmail, cancellationToken);
            }
        }

        if (channel is NotificationChannel.Telegram or NotificationChannel.Both)
        {
            telegramResult = await _telegramSender.SendTestTelegramAsync(tenant, cancellationToken);
        }

        return emailResult && telegramResult;
    }

    public async Task<(bool Success, string BotUsername, string ChatId, string? ChatTitle, string Message)> ConnectAndVerifyTelegramBotAsync(
        Tenant tenant, string botToken, string? manualChatId, CancellationToken cancellationToken = default)
    {
        // 1. Validate Bot Token
        var (tokenValid, botUsername, tokenError) = await _telegramSender.ValidateBotTokenAsync(botToken, cancellationToken);
        if (!tokenValid || string.IsNullOrWhiteSpace(botUsername))
        {
            return (false, string.Empty, string.Empty, null, tokenError ?? "Bot Token yanlışdır.");
        }

        string targetChatId;
        string? targetChatTitle = null;

        // 2. Determine Chat ID (manual or auto-detect via getUpdates)
        if (!string.IsNullOrWhiteSpace(manualChatId))
        {
            targetChatId = manualChatId.Trim();
        }
        else
        {
            var (detected, detectedChatId, detectedTitle, detectError) = await _telegramSender.DetectChatIdAsync(botToken, cancellationToken);
            if (!detected || string.IsNullOrWhiteSpace(detectedChatId))
            {
                return (false, botUsername, string.Empty, null, 
                    $"Bot (@{botUsername}) uğurla tapıldı! Lakin heç bir aktiv çat tapılmadı. " +
                    $"Zəhmət olmasa Telegram-da @{botUsername} botuna daxil olub '/start' basın (və ya botu qrupunuza əlavə edin) və yenidən 'Qoş' vurun.");
            }

            targetChatId = detectedChatId;
            targetChatTitle = detectedTitle;
        }

        // 3. Send confirmation test message
        var welcomeMessage = $"""
            🎉 *Təbriklər! Bildiriş Botu Uğurla Qoşuldu!*
            🏨 *Otel:* {tenant.Name}
            🤖 *Bot:* @{botUsername}
            
            Artıq otaqlardan və masalardan gələn bütün qonaq müraciətləri bu çatda anında görünəcəkdir.
            """;

        var sendSuccess = await _telegramSender.SendDirectMessageAsync(botToken, targetChatId, welcomeMessage, cancellationToken);
        if (!sendSuccess)
        {
            return (false, botUsername, targetChatId, targetChatTitle, 
                "Bot tapıldı, lakin təyin edilmiş Chat ID-yə mesaj göndərmək mümkün olmadı. İstifadəçinin botu bloklamadığından və ya qrup icazələrindən əmin olun.");
        }

        return (true, botUsername, targetChatId, targetChatTitle, 
            $"@{botUsername} botu uğurla qoşuldu və '{targetChatTitle ?? targetChatId}' çatına təsdiq mesajı göndərildi.");
    }
}
