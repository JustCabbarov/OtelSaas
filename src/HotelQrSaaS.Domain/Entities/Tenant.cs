using HotelQrSaaS.Domain.Common;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Domain.Entities;

public class Tenant : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }

    // Bio & Localization
    public string? BioAz { get; set; }
    public string? BioEn { get; set; }
    public string? BioRu { get; set; }
    public string DefaultLanguage { get; set; } = "az";

    // Visual Branding
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string PrimaryColor { get; set; } = "#191a1a";
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Dark;

    // Direct Contacts for vCard and Header
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WhatsApp { get; set; }
    public string? Telegram { get; set; }
    public string? Address { get; set; }
    public string? MapUrl { get; set; }

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

