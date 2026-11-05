using HotelQrSaaS.Domain.Common;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Domain.Entities;

public class Tenant : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }

    // Bio & Localization
    public string? SubtitleAz { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleRu { get; set; }
    public string? TaglineAz { get; set; }
    public string? TaglineEn { get; set; }
    public string? TaglineRu { get; set; }
    public int? StarRating { get; set; } = 5;
    public string? BioAz { get; set; }
    public string? BioEn { get; set; }
    public string? BioRu { get; set; }
    public string DefaultLanguage { get; set; } = "az";
    public string CurrencyCode { get; set; } = "₼";
    public decimal ServiceFeePercent { get; set; } = 0;

    // Visual Branding
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string PrimaryColor { get; set; } = "#191a1a";
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Dark;
    public string ThemePreset { get; set; } = "otel";
    public string ThemeStyle { get; set; } = "yumsaq";

    // Direct Contacts & Socials for vCard, Header, and Footer
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WhatsApp { get; set; }
    public string? Telegram { get; set; }
    public string? Address { get; set; }
    public string? MapUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? TikTokUrl { get; set; }
    public string? TripAdvisorUrl { get; set; }
    public string? BookingUrl { get; set; }

    // Subscription & Kill-Switch
    public bool IsActive { get; set; } = true;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Active;
    public DateTime? SubscriptionEndDate { get; set; }
    public string? SuspensionReason { get; set; }

    // Notification Preferences
    public NotificationChannel PreferredNotificationChannel { get; set; } = NotificationChannel.Both;
    public string? NotificationEmail { get; set; }
    public string? CustomSmtpHost { get; set; }
    public int? CustomSmtpPort { get; set; }
    public string? CustomSmtpUser { get; set; }
    public string? CustomSmtpPassword { get; set; }
    public string? TelegramBotToken { get; set; }
    public string? TelegramChatId { get; set; }
    public string? TelegramBotUsername { get; set; }
    public string? CustomTelegramBotToken { get => TelegramBotToken; set => TelegramBotToken = value; }

    // Audit & Soft Delete
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    // Navigations
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<LinkCategory> LinkCategories { get; set; } = new List<LinkCategory>();
    public virtual ICollection<LinkItem> LinkItems { get; set; } = new List<LinkItem>();
    public virtual ICollection<GuestInquiry> GuestInquiries { get; set; } = new List<GuestInquiry>();
    public virtual ICollection<AnalyticsEvent> AnalyticsEvents { get; set; } = new List<AnalyticsEvent>();
    public virtual ICollection<HotelActivity> Activities { get; set; } = new List<HotelActivity>();
    public virtual ICollection<HotelStory> Stories { get; set; } = new List<HotelStory>();
    public virtual ICollection<HotelHub> Hubs { get; set; } = new List<HotelHub>();
    public virtual ICollection<CatalogCategory> CatalogCategories { get; set; } = new List<CatalogCategory>();
    public virtual ICollection<CatalogItem> CatalogItems { get; set; } = new List<CatalogItem>();
    public virtual ICollection<GuestOrder> GuestOrders { get; set; } = new List<GuestOrder>();
    public virtual ICollection<GuestReview> GuestReviews { get; set; } = new List<GuestReview>();
    public virtual ICollection<HotelDepartmentContact> DepartmentContacts { get; set; } = new List<HotelDepartmentContact>();
    public virtual QrCodeSetting? QrCodeSetting { get; set; }

    /// <summary>
    /// Checks if this hotel is allowed to serve QR scans, public landing page, and redirects.
    /// If false, public portal returns HTTP 403 / Inactive Subscription error.
    /// </summary>
    public bool CanServePublicTraffic()
    {
        if (!IsActive) return false;
        if (SubscriptionStatus == SubscriptionStatus.Suspended || SubscriptionStatus == SubscriptionStatus.Expired) return false;
        if (SubscriptionEndDate.HasValue && SubscriptionEndDate.Value < DateTime.UtcNow) return false;
        return true;
    }
}

