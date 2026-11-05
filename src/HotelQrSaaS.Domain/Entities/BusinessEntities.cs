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

/// <summary>
/// Ana səhifədəki yuxarı "Stories / Highlights" karuseli (məs: Günün menyusu, Canlı musiqi, Spa -20%, Ulduzlu gecə)
/// </summary>
public class HotelStory : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public int? ExternalId { get; set; } // e.g. 2..7 in Skyhill

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }

    public string? ButtonTextAz { get; set; }
    public string? ButtonTextEn { get; set; }
    public string? ButtonTextRu { get; set; }

    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; } // .mp4 video URL (e.g. story-welcome.mp4)
    public string? BannerImageUrl { get; set; }
    public string? BadgeText { get; set; }
    public string? ActionUrl { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

/// <summary>
/// Ana səhifədəki 6 əsas Otel Mərkəzi Plitəsi (Hubs: Otel məlumatı, Qida və içki, Spa və sağlamlıq, Əyləncə, Əlaqə, Günün təklifi)
/// </summary>
public class HotelHub : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public int? ExternalId { get; set; } // e.g. 191, 192, 193, 194, 195, 196
    public string Code { get; set; } = string.Empty; // e.g. "info", "food", "spa", "entertainment", "contacts", "specials"
    public HubType HubType { get; set; } = HubType.Custom;
    public string DisplayMode { get; set; } = "categories"; // "categories", "filter_items", "direct_items"

    public string? KickerAz { get; set; } // e.g. "MARAL"
    public string? KickerEn { get; set; }
    public string? KickerRu { get; set; }

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? SubtitleAz { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleRu { get; set; }

    public string? IconKey { get; set; }
    public string? CoverImageUrl { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<CatalogCategory> Categories { get; set; } = new List<CatalogCategory>();
    public virtual ICollection<CatalogItem> DirectItems { get; set; } = new List<CatalogItem>();
}

/// <summary>
/// Hub daxilindəki Məkanlar / Restoranlar / Spa Bölmələri / Alt-kateqoriyalar (məs: SHIRVAN, SAQI Lounge, Masajlar, Otaqlar)
/// </summary>
public class CatalogCategory : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public int? ExternalId { get; set; } // e.g. 4569..4598 in Skyhill
    public Guid HubId { get; set; }
    public virtual HotelHub Hub { get; set; } = null!;

    public Guid? ParentCategoryId { get; set; }
    public virtual CatalogCategory? ParentCategory { get; set; }
    public virtual ICollection<CatalogCategory> SubCategories { get; set; } = new List<CatalogCategory>();

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? SubtitleAz { get; set; } // e.g. "Restoran və teras · Bütün gün", "Lounge bar · 16:00–02:00"
    public string? SubtitleEn { get; set; }
    public string? SubtitleRu { get; set; }

    public string? ImageUrl { get; set; }
    public string? IconKey { get; set; }
    public string? WorkingHours { get; set; } // e.g. "08:00–20:00", "24 saat"
    public string? InternalExtension { get; set; } // e.g. "daxili 3"
    public string LayoutStyle { get; set; } = "list"; // "list", "grid", "cards", "filterable"
    public bool CollapseIfSingleItem { get; set; } = true; // UniPOS 1-item category collapse rule

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<CatalogItem> Items { get; set; } = new List<CatalogItem>();
}

/// <summary>
/// Kataloq Elementi: Yemək/İçki (qiymətlə), Spa proseduru, Otaq növü, Otel imkanı, A–Z Bələdçi məqaləsi və ya Günün təklifi
/// </summary>
public class CatalogItem : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public int? ExternalId { get; set; } // e.g. 43107..43234 in Skyhill
    public Guid HubId { get; set; }
    public virtual HotelHub Hub { get; set; } = null!;

    public Guid? CategoryId { get; set; }
    public virtual CatalogCategory? Category { get; set; }

    public CatalogItemType ItemType { get; set; } = CatalogItemType.MenuProduct;

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? SubtitleAz { get; set; } // Short venue/context subtitle on list rows (e.g. "SHIRVAN", "SAQI Lounge")
    public string? SubtitleEn { get; set; }
    public string? SubtitleRu { get; set; }

    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }

    public string? BadgeAz { get; set; } // e.g. "Aşpazın seçimi", "Yalnız bu gün", "Bu həftə", "5 ulduz"
    public string? BadgeEn { get; set; }
    public string? BadgeRu { get; set; }

    public string? IconKey { get; set; } // qrIcon in UniPOS (sparkles, hotel, bed, users, key, pool, etc.)
    public string? FilterTag { get; set; } // "food", "spa", "entertainment" or category ExternalId ("4594", "4595", "4596")
    public decimal? Price { get; set; } // null or 0 for informational cards, decimal for food/spa/packages
    public decimal? OldPrice { get; set; }
    public decimal CostPrice { get; set; } = 0;
    public decimal DiscountPercent { get; set; } = 0;
    public bool NoServiceFee { get; set; } = false;
    public int? PrepTimeMinutes { get; set; } // timer in UniPOS
    public string Currency { get; set; } = "₼";
    public string? PortionOrDuration { get; set; } // e.g. "60 dəq", "350 qr", "1 saat"

    /// <summary>
    /// Maddə-maddə xüsusiyyətlər (məs: "• 120 otaq və suit\n• 3 restoran və bar")
    /// </summary>
    public string? BulletPointsAz { get; set; }
    public string? BulletPointsEn { get; set; }
    public string? BulletPointsRu { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsOrderable { get; set; } = false; // true (!infoOnly) olduqda "Səbətə əlavə et" düyməsi aktiv olur
    public bool IsFeaturedInDailySpecials { get; set; } = false;

    public string? ActionType { get; set; } // qrActionType in UniPOS: "phone", "whatsapp", "email", "map", "link"
    public string? ActionValue { get; set; } // qrActionValue in UniPOS: "+994125551234", "https://www.google.com/maps/..."
    public string? ActionLabelAz { get; set; } // e.g. "SPA-ya zəng", "Resepsiyona zəng"
    public string? ActionLabelEn { get; set; }
    public string? ActionLabelRu { get; set; }
    public string? ActionUrl { get; set; } // e.g. "tel:+994125551234" or "https://wa.me/..."

    public string? ModifierGroupsJson { get; set; } // JSON array of modifier groups for POS customization

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

/// <summary>
/// Qonağın Səbət Sifarişi (Otaq xidməti, Restoran masası və ya Spa rezervasiyası)
/// </summary>
public class GuestOrder : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string OrderNumber { get; set; } = string.Empty; // e.g. "ORD-1042"
    public OrderType OrderType { get; set; } = OrderType.RoomService;
    public string RoomOrTableNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "₼";
    public OrderStatus Status { get; set; } = OrderStatus.New;

    public bool IsEmailSent { get; set; }
    public bool IsTelegramSent { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public virtual ICollection<GuestOrderItem> Items { get; set; } = new List<GuestOrderItem>();
}

public class GuestOrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public virtual GuestOrder Order { get; set; } = null!;

    public Guid? CatalogItemId { get; set; }
    public virtual CatalogItem? CatalogItem { get; set; }

    public string ItemTitle { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? SpecialInstruction { get; set; }
}

/// <summary>
/// Qonağın Daxili Ulduz və Emoji Qiymətləndirməsi (/review)
/// </summary>
public class GuestReview : BaseEntity, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public int Rating { get; set; } = 5; // 1..5 stars
    public string? GuestName { get; set; }
    public string? RoomNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; } // "General", "Restaurant", "Spa", "Rooms"
    public string? Comment { get; set; }

    public bool IsResolved { get; set; } = false;
    public bool IsTelegramAlertSent { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

/// <summary>
/// Əlaqə bölməsindəki "Şöbə əlaqələri" (Resepsiyon, Otaq xidməti, MARAL Spa, Təhlükəsizlik və s.)
/// </summary>
public class HotelDepartmentContact : BaseEntity, ITenantEntity, IAuditableEntity, ISoftDelete
{
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? SubtitleAz { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleRu { get; set; }

    public string? PhoneNumber { get; set; }
    public string? InternalExtension { get; set; } // e.g. "1", "3", "105"
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public string? WorkingHours { get; set; }
    public string? IconKey { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}
