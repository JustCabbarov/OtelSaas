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
public record LocalizedTextDto(
    string? Az,
    string? En,
    string? Ru
);

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
    DateTime? SubscriptionEndDate,
    string? SubtitleAz = null,
    string? SubtitleEn = null,
    string? SubtitleRu = null,
    int? StarRating = 5,
    string? TaglineAz = null,
    string? TaglineEn = null,
    string? TaglineRu = null,
    string CurrencyCode = "₼",
    decimal ServiceFeePercent = 0,
    string ThemePreset = "otel",
    string ThemeStyle = "yumsaq",
    string? InstagramUrl = null,
    string? FacebookUrl = null,
    string? YoutubeUrl = null,
    string? TikTokUrl = null,
    string? TripAdvisorUrl = null,
    string? BookingUrl = null
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
    string? MapUrl,
    string? SubtitleAz = null,
    string? SubtitleEn = null,
    string? SubtitleRu = null,
    int? StarRating = 5,
    string? TaglineAz = null,
    string? TaglineEn = null,
    string? TaglineRu = null,
    string CurrencyCode = "₼",
    decimal ServiceFeePercent = 0,
    string ThemePreset = "otel",
    string ThemeStyle = "yumsaq",
    string? InstagramUrl = null,
    string? FacebookUrl = null,
    string? YoutubeUrl = null,
    string? TikTokUrl = null,
    string? TripAdvisorUrl = null,
    string? BookingUrl = null
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
    Dictionary<string, int> DeviceBreakdown,
    int TotalOrders = 0,
    decimal TotalOrderRevenue = 0,
    int TotalReviews = 0,
    double AverageRating = 5.0
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

// --- Skyhill New Modules: Stories, Hubs, Catalog, Orders, Reviews, Departments ---

public record HotelStoryDto(
    Guid Id,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? DescriptionAz,
    string? DescriptionEn,
    string? DescriptionRu,
    string? ImageUrl,
    string? BannerImageUrl,
    string? BadgeText,
    string? ActionUrl,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    string? VideoUrl = null,
    string? ButtonTextAz = null,
    string? ButtonTextEn = null,
    string? ButtonTextRu = null
);

public record CreateHotelStoryDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? DescriptionAz,
    string? DescriptionEn,
    string? DescriptionRu,
    string? ImageUrl,
    string? BannerImageUrl,
    string? BadgeText,
    string? ActionUrl,
    int DisplayOrder = 0,
    bool IsActive = true,
    int? ExternalId = null,
    string? VideoUrl = null,
    string? ButtonTextAz = null,
    string? ButtonTextEn = null,
    string? ButtonTextRu = null
);

public record UpdateHotelStoryDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? DescriptionAz,
    string? DescriptionEn,
    string? DescriptionRu,
    string? ImageUrl,
    string? BannerImageUrl,
    string? BadgeText,
    string? ActionUrl,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    string? VideoUrl = null,
    string? ButtonTextAz = null,
    string? ButtonTextEn = null,
    string? ButtonTextRu = null
);

public record HotelHubDto(
    Guid Id,
    string Code,
    HubType HubType,
    string? KickerAz,
    string? KickerEn,
    string? KickerRu,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? IconKey,
    string? CoverImageUrl,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    string DisplayMode = "categories"
);

public record CreateHotelHubDto(
    string Code,
    HubType HubType,
    string? KickerAz,
    string? KickerEn,
    string? KickerRu,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? IconKey,
    string? CoverImageUrl,
    int DisplayOrder = 0,
    bool IsActive = true,
    int? ExternalId = null,
    string DisplayMode = "categories"
);

public record UpdateHotelHubDto(
    string Code,
    HubType HubType,
    string? KickerAz,
    string? KickerEn,
    string? KickerRu,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? IconKey,
    string? CoverImageUrl,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    string DisplayMode = "categories"
);

public record CatalogCategoryDto(
    Guid Id,
    Guid HubId,
    Guid? ParentCategoryId,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? ImageUrl,
    string? IconKey,
    string? WorkingHours,
    string? InternalExtension,
    string LayoutStyle,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    bool CollapseIfSingleItem = true
);

public record CreateCatalogCategoryDto(
    Guid HubId,
    Guid? ParentCategoryId,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? ImageUrl,
    string? IconKey,
    string? WorkingHours,
    string? InternalExtension,
    string LayoutStyle = "list",
    int DisplayOrder = 0,
    bool IsActive = true,
    int? ExternalId = null,
    bool CollapseIfSingleItem = true
);

public record UpdateCatalogCategoryDto(
    Guid HubId,
    Guid? ParentCategoryId,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? ImageUrl,
    string? IconKey,
    string? WorkingHours,
    string? InternalExtension,
    string LayoutStyle,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    bool CollapseIfSingleItem = true
);

public record CatalogItemDto(
    Guid Id,
    Guid HubId,
    Guid? CategoryId,
    CatalogItemType ItemType,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? DescriptionAz,
    string? DescriptionEn,
    string? DescriptionRu,
    string? BadgeAz,
    string? BadgeEn,
    string? BadgeRu,
    string? FilterTag,
    decimal? Price,
    decimal? OldPrice,
    string Currency,
    string? PortionOrDuration,
    string? BulletPointsAz,
    string? BulletPointsEn,
    string? BulletPointsRu,
    string? ImageUrl,
    bool IsOrderable,
    bool IsFeaturedInDailySpecials,
    string? ActionLabelAz,
    string? ActionLabelEn,
    string? ActionLabelRu,
    string? ActionUrl,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    string? SubtitleAz = null,
    string? SubtitleEn = null,
    string? SubtitleRu = null,
    string? IconKey = null,
    string? ActionType = null,
    string? ActionValue = null,
    decimal DiscountPercent = 0,
    decimal CostPrice = 0,
    int? PrepTimeMinutes = null,
    bool NoServiceFee = false,
    string? ModifierGroupsJson = null
);

public record CreateCatalogItemDto(
    Guid HubId,
    Guid? CategoryId,
    CatalogItemType ItemType,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? DescriptionAz,
    string? DescriptionEn,
    string? DescriptionRu,
    string? BadgeAz,
    string? BadgeEn,
    string? BadgeRu,
    string? FilterTag,
    decimal? Price,
    decimal? OldPrice,
    string Currency = "₼",
    string? PortionOrDuration = null,
    string? BulletPointsAz = null,
    string? BulletPointsEn = null,
    string? BulletPointsRu = null,
    string? ImageUrl = null,
    bool IsOrderable = false,
    bool IsFeaturedInDailySpecials = false,
    string? ActionLabelAz = null,
    string? ActionLabelEn = null,
    string? ActionLabelRu = null,
    string? ActionUrl = null,
    int DisplayOrder = 0,
    bool IsActive = true,
    int? ExternalId = null,
    string? SubtitleAz = null,
    string? SubtitleEn = null,
    string? SubtitleRu = null,
    string? IconKey = null,
    string? ActionType = null,
    string? ActionValue = null,
    decimal DiscountPercent = 0,
    decimal CostPrice = 0,
    int? PrepTimeMinutes = null,
    bool NoServiceFee = false,
    string? ModifierGroupsJson = null
);

public record UpdateCatalogItemDto(
    Guid HubId,
    Guid? CategoryId,
    CatalogItemType ItemType,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? DescriptionAz,
    string? DescriptionEn,
    string? DescriptionRu,
    string? BadgeAz,
    string? BadgeEn,
    string? BadgeRu,
    string? FilterTag,
    decimal? Price,
    decimal? OldPrice,
    string Currency,
    string? PortionOrDuration,
    string? BulletPointsAz,
    string? BulletPointsEn,
    string? BulletPointsRu,
    string? ImageUrl,
    bool IsOrderable,
    bool IsFeaturedInDailySpecials,
    string? ActionLabelAz,
    string? ActionLabelEn,
    string? ActionLabelRu,
    string? ActionUrl,
    int DisplayOrder,
    bool IsActive,
    int? ExternalId = null,
    string? SubtitleAz = null,
    string? SubtitleEn = null,
    string? SubtitleRu = null,
    string? IconKey = null,
    string? ActionType = null,
    string? ActionValue = null,
    decimal DiscountPercent = 0,
    decimal CostPrice = 0,
    int? PrepTimeMinutes = null,
    bool NoServiceFee = false,
    string? ModifierGroupsJson = null
);

public record GuestOrderItemDto(
    Guid Id,
    Guid? CatalogItemId,
    string ItemTitle,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? SpecialInstruction
);

public record GuestOrderDto(
    Guid Id,
    string OrderNumber,
    OrderType OrderType,
    string RoomOrTableNumber,
    string GuestName,
    string PhoneNumber,
    string? Notes,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
    bool IsEmailSent,
    bool IsTelegramSent,
    DateTime CreatedAtUtc,
    List<GuestOrderItemDto> Items
);

public record UpdateOrderStatusDto(OrderStatus Status);

public record GuestReviewDto(
    Guid Id,
    int Rating,
    string? GuestName,
    string? RoomNumber,
    string? PhoneNumber,
    string? Department,
    string? Comment,
    bool IsResolved,
    bool IsTelegramAlertSent,
    DateTime CreatedAtUtc
);

public record DepartmentContactDto(
    Guid Id,
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? PhoneNumber,
    string? InternalExtension,
    string? WhatsAppNumber,
    string? Email,
    string? WorkingHours,
    string? IconKey,
    int DisplayOrder,
    bool IsActive
);

public record CreateDepartmentContactDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? PhoneNumber,
    string? InternalExtension,
    string? WhatsAppNumber,
    string? Email,
    string? WorkingHours,
    string? IconKey,
    int DisplayOrder = 0,
    bool IsActive = true
);

public record UpdateDepartmentContactDto(
    string TitleAz,
    string? TitleEn,
    string? TitleRu,
    string? SubtitleAz,
    string? SubtitleEn,
    string? SubtitleRu,
    string? PhoneNumber,
    string? InternalExtension,
    string? WhatsAppNumber,
    string? Email,
    string? WorkingHours,
    string? IconKey,
    int DisplayOrder,
    bool IsActive
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
    List<PublicActivityDto>? Activities = null,
    string? Subtitle = null,
    int? StarRating = 5,
    List<PublicStoryDto>? Stories = null,
    List<PublicHubSummaryDto>? Hubs = null,
    List<PublicCatalogItemDto>? DailySpecials = null,
    List<PublicDepartmentContactDto>? DepartmentContacts = null,
    string? Tagline = null,
    LocalizedTextDto? SubtitleTr = null,
    LocalizedTextDto? TaglineTr = null,
    LocalizedTextDto? BioTr = null,
    string CurrencyCode = "₼",
    decimal ServiceFeePercent = 0,
    string ThemePreset = "otel",
    string ThemeStyle = "yumsaq",
    string? InstagramUrl = null,
    string? FacebookUrl = null,
    string? YoutubeUrl = null,
    string? TikTokUrl = null,
    string? TripAdvisorUrl = null,
    string? BookingUrl = null
);

public record PublicStoryDto(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    string? BannerImageUrl,
    string? BadgeText,
    string? ActionUrl,
    int DisplayOrder,
    int? ExternalId = null,
    string? VideoUrl = null,
    string? ButtonText = null,
    LocalizedTextDto? TitleTr = null,
    LocalizedTextDto? CaptionTr = null,
    LocalizedTextDto? ButtonTr = null
);

public record PublicHubSummaryDto(
    Guid Id,
    string Code,
    HubType HubType,
    string? Kicker,
    string Title,
    string? Subtitle,
    string? IconKey,
    string? CoverImageUrl,
    int DisplayOrder,
    int? ExternalId = null,
    string DisplayMode = "categories",
    LocalizedTextDto? KickerTr = null,
    LocalizedTextDto? TitleTr = null,
    LocalizedTextDto? SubtitleTr = null
);

public record PublicHubDetailDto(
    Guid Id,
    string Code,
    HubType HubType,
    string? Kicker,
    string Title,
    string? Subtitle,
    string? IconKey,
    string? CoverImageUrl,
    List<PublicCatalogCategoryDto> Categories,
    List<PublicCatalogItemDto> DirectItems,
    List<PublicDepartmentContactDto>? DepartmentContacts = null,
    int? ExternalId = null,
    string DisplayMode = "categories",
    LocalizedTextDto? KickerTr = null,
    LocalizedTextDto? TitleTr = null,
    LocalizedTextDto? SubtitleTr = null
);

public record PublicCatalogCategoryDto(
    Guid Id,
    Guid HubId,
    Guid? ParentCategoryId,
    string Title,
    string? Subtitle,
    string? ImageUrl,
    string? IconKey,
    string? WorkingHours,
    string? InternalExtension,
    string LayoutStyle,
    int DisplayOrder,
    int ItemsCount = 0,
    int? ExternalId = null,
    bool CollapseIfSingleItem = true,
    Guid? SingleItemId = null,
    int? SingleItemExternalId = null,
    string? TargetUrl = null,
    string? ActionType = null,
    string? ActionValue = null,
    LocalizedTextDto? TitleTr = null,
    LocalizedTextDto? SubtitleTr = null
);

public record PublicCatalogCategoryDetailDto(
    Guid Id,
    Guid HubId,
    string HubTitle,
    string Title,
    string? Subtitle,
    string? ImageUrl,
    string? WorkingHours,
    string? InternalExtension,
    string LayoutStyle,
    List<PublicCatalogCategoryDto> SubCategories,
    List<PublicCatalogItemDto> Items,
    int? ExternalId = null,
    int? HubExternalId = null,
    string? HubCode = null,
    LocalizedTextDto? TitleTr = null,
    LocalizedTextDto? SubtitleTr = null,
    LocalizedTextDto? HubTitleTr = null
);

public record PublicCatalogItemDto(
    Guid Id,
    Guid HubId,
    Guid? CategoryId,
    CatalogItemType ItemType,
    string Title,
    string? Description,
    string? Badge,
    string? FilterTag,
    decimal? Price,
    decimal? OldPrice,
    string Currency,
    string? PortionOrDuration,
    List<string> BulletPoints,
    string? ImageUrl,
    bool IsOrderable,
    bool IsFeaturedInDailySpecials,
    string? ActionLabel,
    string? ActionUrl,
    int DisplayOrder,
    int? ExternalId = null,
    int? CategoryExternalId = null,
    int? HubExternalId = null,
    string? Subtitle = null,
    string? IconKey = null,
    string? ActionType = null,
    string? ActionValue = null,
    decimal DiscountPercent = 0,
    decimal CostPrice = 0,
    int? PrepTimeMinutes = null,
    bool NoServiceFee = false,
    LocalizedTextDto? TitleTr = null,
    LocalizedTextDto? SubtitleTr = null,
    LocalizedTextDto? DescriptionTr = null,
    LocalizedTextDto? BadgeTr = null,
    LocalizedTextDto? CategoryNameTr = null,
    LocalizedTextDto? ActionLabelTr = null,
    List<object>? ModifierGroups = null
);

public record PublicDepartmentContactDto(
    Guid Id,
    string Title,
    string? Subtitle,
    string? PhoneNumber,
    string? InternalExtension,
    string? WhatsAppNumber,
    string? Email,
    string? WorkingHours,
    string? IconKey,
    int DisplayOrder,
    LocalizedTextDto? TitleTr = null,
    LocalizedTextDto? SubtitleTr = null
);

public record UniPosProductDto(
    int Id,
    Guid GuidId,
    string Name,
    LocalizedTextDto NameTr,
    decimal Price,
    decimal CostPrice,
    int? Timer,
    string? Image,
    string? Ingredient,
    decimal Discount,
    bool NoServiceFee,
    int CategoryId,
    Guid? CategoryGuidId,
    string CategoryName,
    LocalizedTextDto CategoryNameTr,
    int Ordering,
    string? QrIcon,
    LocalizedTextDto QrBadge,
    string? QrActionType,
    string? QrActionValue,
    bool InfoOnly,
    List<object> ModifierGroups
);

public record PublicFullCatalogDto(
    string Restaurant,
    decimal ServiceFee,
    string Currency,
    List<UniPosProductDto> Products,
    List<PublicCatalogCategoryDto> Categories,
    List<PublicHubSummaryDto> Hubs,
    List<PublicStoryDto> Stories
);

public record SubmitOrderItemRequestDto(
    Guid? CatalogItemId,
    string ItemTitle,
    int Quantity,
    decimal UnitPrice,
    string? SpecialInstruction = null
);

public record SubmitOrderRequestDto(
    OrderType OrderType,
    string RoomOrTableNumber,
    string GuestName,
    string PhoneNumber,
    string? Notes,
    List<SubmitOrderItemRequestDto> Items
);

public record SubmitReviewRequestDto(
    int Rating,
    string? GuestName,
    string? RoomNumber,
    string? PhoneNumber,
    string? Department,
    string? Comment
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

