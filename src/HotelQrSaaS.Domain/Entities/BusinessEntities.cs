using HotelQrSaaS.Domain.Common;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Domain.Entities;

public class LinkCategory : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    /// <summary>
    /// Style layout: "icons" for compact grid (e.g. phone, mail, whatsapp) or "list" for detailed rows.
    /// </summary>
    public string LayoutType { get; set; } = "list"; // "icons" or "list"
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<LinkItem> Links { get; set; } = new List<LinkItem>();
}

public class LinkItem : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public virtual LinkCategory Category { get; set; } = null!;

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public LinkType LinkType { get; set; } = LinkType.StandardUrl;
    public string TargetUrl { get; set; } = string.Empty; // External URL or local path for PDF
    public string? IconKey { get; set; } // Standard icon key (phone, mail, whatsapp, etc.) or custom uploaded image path
    public bool IsCustomIcon { get; set; }

    public bool OpenInNewTab { get; set; } = true;
    public long ClickCount { get; set; } = 0;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

public class GuestInquiry : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? RoomNumber { get; set; }
    public string Message { get; set; } = string.Empty;

    public InquiryStatus Status { get; set; } = InquiryStatus.New;

    public bool IsEmailSent { get; set; }
    public bool IsTelegramSent { get; set; }
    public string? NotificationLog { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public class AnalyticsEvent : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public Guid? LinkItemId { get; set; }
    public virtual LinkItem? LinkItem { get; set; }

    public AnalyticsEventType EventType { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; } // Mobile, Desktop, Tablet
    public string? Referrer { get; set; }
    public string? Language { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class QrCodeSetting : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string ForegroundColorHex { get; set; } = "#000000";
    public string BackgroundColorHex { get; set; } = "#FFFFFF";
    public bool IncludeLogo { get; set; } = true;
    public int LogoSizePercent { get; set; } = 18;
    public int PixelsPerModule { get; set; } = 20;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public class HotelActivity : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty; // e.g. "Əsas Hovuz kənarı", "Sky Lounge Bar"
    public string StartTime { get; set; } = string.Empty; // e.g. "15:00"
    public string? EndTime { get; set; } // e.g. "18:00"
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public string? Category { get; set; } // "party", "music", "sport", "kids", "wellness", "food"
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

