using System.Security.Claims;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Application.Interfaces.Services;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    void SetTenant(Guid tenantId, string slug);
}

public interface IJwtTokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GenerateTokensAsync(
        ApplicationUser user, IList<string> roles, Tenant? tenant);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, Guid tenantId, CancellationToken cancellationToken = default);
    void DeleteFile(string? relativePath);
}

public interface INotificationDispatcher
{
    Task<bool> DispatchInquiryNotificationAsync(Tenant tenant, GuestInquiry inquiry, CancellationToken cancellationToken = default);
    Task<bool> SendTestNotificationAsync(Tenant tenant, NotificationChannel channel, CancellationToken cancellationToken = default);
    Task<(bool Success, string BotUsername, string ChatId, string? ChatTitle, string Message)> ConnectAndVerifyTelegramBotAsync(
        Tenant tenant, string botToken, string? manualChatId, CancellationToken cancellationToken = default);
}

public interface IQrCodeService
{
    byte[] GeneratePngQrCode(string url, int pixelsPerModule = 20, string? foregroundHex = null, string? backgroundHex = null);
    string GenerateSvgQrCode(string url, string? foregroundHex = null, string? backgroundHex = null);
}

public interface IVCardService
{
    byte[] GenerateVCardBytes(Tenant tenant);
    string GenerateVCardString(Tenant tenant);
}

