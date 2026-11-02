using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Application.DTOs;

#region Auth DTOs
public record LoginRequestDto(string Email, string Password);

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Email,
    string FullName,
    IList<string> Roles,
    Guid? TenantId,
    string? TenantSlug,
    string? TenantName
);

public record RefreshTokenRequestDto(string AccessToken, string RefreshToken);

public record CurrentUserDto(
    Guid Id,
    string Email,
    string FullName,
    IList<string> Roles,
    Guid? TenantId,
    string? TenantSlug
);
#endregion

#region SuperAdmin DTOs
public record CreateTenantRequestDto(
    string Name,
    string Slug,
    string AdminEmail,
    string AdminPassword,
    string AdminFullName,
    string? Phone = null,
    DateTime? SubscriptionEndDate = null
);

public record UpdateTenantRequestDto(
    string Name,
    string Slug,
    string? CustomDomain,
    string? Phone,
    string? Email,
    string? WhatsApp,
    string? Telegram,
    string? Address,
    string? MapUrl
);

public record UpdateSubscriptionRequestDto(
    bool IsActive,
    SubscriptionStatus SubscriptionStatus,
    DateTime? SubscriptionEndDate,
    string? SuspensionReason
);

public record TenantSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    bool IsActive,
    SubscriptionStatus SubscriptionStatus,
    DateTime? SubscriptionEndDate,
    string? SuspensionReason,
    int LinksCount,
    long TotalClicks,
    DateTime CreatedAtUtc
);

public record SuperAdminDashboardDto(
    int TotalTenants,
    int ActiveTenants,
    int SuspendedTenants,
    int TotalInquiries,
    long TotalSystemClicks
);
#endregion

#region HotelAdmin DTOs
public record HotelProfileDto(
    Guid Id,
    string Name,
    string Slug,
    string? BioAz,
    string? BioEn,
    string? BioRu,
    string DefaultLanguage,
    string? LogoUrl,
    string? CoverImageUrl,
    string PrimaryColor,
    ThemeMode ThemeMode,
    string? Phone,
    string? Email,
    string? WhatsApp,
    string? Telegram,
    string? Address,
    string? MapUrl,
    bool IsActive,
    SubscriptionStatus SubscriptionStatus,
    DateTime? SubscriptionEndDate
);

public record UpdateHotelProfileDto(
    string Name,
    string? BioAz,
    string? BioEn,
    string? BioRu,
    string DefaultLanguage,
    string PrimaryColor,
    ThemeMode ThemeMode,
    string? Phone,
    string? Email,
    string? WhatsApp,
    string? Telegram,
    string? Address,
    string? MapUrl
);

public record NotificationSettingsDto(
    NotificationChannel PreferredNotificationChannel,
    string? NotificationEmail,
    bool IsEmailConfigured,
    string? TelegramBotToken,
    string? TelegramChatId,
    string? TelegramBotUsername,
    bool IsTelegramConfigured
);

public record UpdateNotificationSettingsDto(
    NotificationChannel PreferredNotificationChannel,
    string? NotificationEmail,
    string? TelegramBotToken,
    string? TelegramChatId
);

public record ConnectEmailRequestDto(
    string Email,
    bool SendTestEmail = true,
    string? CustomSmtpHost = null,
    int? CustomSmtpPort = null,
    string? CustomSmtpUser = null,
    string? CustomSmtpPassword = null
);

public record ConnectEmailResponseDto(
    bool Success,
    string Email,
    string Message
);

public record ConnectTelegramBotRequestDto(
    string BotToken,
    string? ChatId = null
);

public record ConnectTelegramBotResponseDto(
    bool Success,
    string BotUsername,
    string ChatId,
    string? ChatTitle,
    string Message
);

public record CategoryDto(
    Guid Id,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string LayoutType,
    int DisplayOrder,
    bool IsActive,
    List<LinkItemDto> Links
);

public record CreateCategoryDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string LayoutType,
    int DisplayOrder,
    bool IsActive
);

public record UpdateCategoryDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string LayoutType,
    int DisplayOrder,
    bool IsActive
);

public record LinkItemDto(
    Guid Id,
    Guid CategoryId,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    LinkType LinkType,
    string TargetUrl,
    string? IconKey,
    bool IsCustomIcon,
    bool OpenInNewTab,
    long ClickCount,
    int DisplayOrder,
    bool IsActive
);

public record CreateLinkItemDto(
    Guid CategoryId,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    LinkType LinkType,
    string TargetUrl,
    string? IconKey,
    bool IsCustomIcon,
    bool OpenInNewTab,
    int DisplayOrder,
    bool IsActive
);

public record UpdateLinkItemDto(
    Guid CategoryId,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    LinkType LinkType,
    string TargetUrl,
    string? IconKey,
    bool IsCustomIcon,
    bool OpenInNewTab,
    int DisplayOrder,
    bool IsActive
);

public record ReorderItemDto(Guid Id, int DisplayOrder);
public record ReorderLinksDto(List<ReorderItemDto> Items);

public record InquiryDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string? Email,
    string? RoomNumber,
    string Message,
    InquiryStatus Status,
    bool IsEmailSent,
    bool IsTelegramSent,
    string? NotificationLog,
    DateTime CreatedAtUtc
);

public record UpdateInquiryStatusDto(InquiryStatus Status);

public record AnalyticsSummaryDto(
    int TotalScans,
    int TotalVCardDownloads,
    int TotalInquiries,
    long TotalLinkClicks,
    List<LinkClickStatDto> TopLinks,
    Dictionary<string, int> DeviceBreakdown
);

public record LinkClickStatDto(Guid LinkId, string Title, long ClickCount);

public record HotelActivityDto(
    Guid Id,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? Description,
    string Location,
    string StartTime,
    string? EndTime,
    DateTime Date,
    string? Category,
    string? ImageUrl,
    bool IsActive,
    int DisplayOrder
);

public record CreateHotelActivityDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? Description,
    string Location,
    string StartTime,
    string? EndTime,
    DateTime? Date,
    string? Category,
    string? ImageUrl,
    int DisplayOrder = 0
);

public record UpdateHotelActivityDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? Description,
    string Location,
    string StartTime,
    string? EndTime,
    DateTime? Date,
    string? Category,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive
);

public record PublicActivityDto(
    Guid Id,
    string Title,
    string? Description,
    string Location,
    string StartTime,
    string? EndTime,
    DateTime Date,
    string? Category,
    string? ImageUrl
);
#endregion

#region Public Portal DTOs
public record PublicHotelPortalDto(
    string Name,
    string Slug,
    string? Bio,
    string? LogoUrl,
    string? CoverImageUrl,
    string PrimaryColor,
    ThemeMode ThemeMode,
    string? Phone,
    string? Email,
    string? WhatsApp,
    string? Telegram,
    string? Address,
    string? MapUrl,
    List<PublicCategoryDto> Categories,
    List<PublicActivityDto>? Activities = null
);

public record PublicCategoryDto(
    Guid Id,
    string Title,
    string LayoutType,
    int DisplayOrder,
    List<PublicLinkDto> Links
);

public record PublicLinkDto(
    Guid Id,
    string Title,
    LinkType LinkType,
    string ActionUrl, // Dynamic redirect url or direct link
    string? IconKey,
    bool IsCustomIcon,
    bool OpenInNewTab,
    int DisplayOrder
);

public record SubmitInquiryRequestDto(
    string FullName,
    string PhoneNumber,
    string? Email,
    string? RoomNumber,
    string Message
);
#endregion

