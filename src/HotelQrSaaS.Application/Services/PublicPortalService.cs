using System.Text.Json;
using Microsoft.Extensions.Configuration;
using HotelQrSaaS.Application.Common.Exceptions;
using HotelQrSaaS.Application.DTOs;
using HotelQrSaaS.Application.Interfaces.Services;
using HotelQrSaaS.Application.Interfaces.UnitOfWork;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Application.Services;

public interface IPublicPortalService
{
    Task<PublicHotelPortalDto> GetPublicPortalAsync(string slug, string? lang, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<PublicFullCatalogDto> GetFullCatalogAsync(string slug, string? lang, CancellationToken cancellationToken = default);
    Task<PublicHubDetailDto> GetHubDetailAsync(string slug, string hubCodeOrId, string? lang, CancellationToken cancellationToken = default);
    Task<PublicCatalogCategoryDetailDto> GetCatalogCategoryDetailAsync(string slug, Guid categoryId, string? lang, CancellationToken cancellationToken = default);
    Task<PublicCatalogCategoryDetailDto> GetCatalogCategoryDetailAsync(string slug, string categoryCodeOrId, string? lang, CancellationToken cancellationToken = default);
    Task<PublicCatalogItemDto> GetCatalogItemDetailAsync(string slug, Guid itemId, string? lang, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<PublicCatalogItemDto> GetCatalogItemDetailAsync(string slug, string itemCodeOrId, string? lang, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<GuestOrderDto> SubmitOrderAsync(string slug, SubmitOrderRequestDto request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<bool> SubmitReviewAsync(string slug, SubmitReviewRequestDto request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<string> GetPwaManifestJsonAsync(string slug, CancellationToken cancellationToken = default);

    // Existing methods preserved 100%
    Task<(byte[] VCardBytes, string FileName)> GetVCardAsync(string slug, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<bool> SubmitInquiryAsync(string slug, SubmitInquiryRequestDto request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<string> ProcessRedirectAsync(Guid linkId, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<string> ProcessQrScanRedirectAsync(string slug, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<byte[]> GetQrPngAsync(string slug, string baseUrl, CancellationToken cancellationToken = default);
    Task<string> GetQrSvgAsync(string slug, string baseUrl, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublicActivityDto>> GetActivitiesAsync(string slug, string? lang, DateTime? date = null, CancellationToken cancellationToken = default);
}

public class PublicPortalService : IPublicPortalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IVCardService _vCardService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IConfiguration _configuration;

    public PublicPortalService(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher,
        IVCardService vCardService,
        IQrCodeService qrCodeService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
        _vCardService = vCardService;
        _qrCodeService = qrCodeService;
        _configuration = configuration;
    }

    private static string? Localize(string lang, string? az, string? en, string? ru) =>
        lang switch
        {
            "en" => !string.IsNullOrWhiteSpace(en) ? en : az,
            "ru" => !string.IsNullOrWhiteSpace(ru) ? ru : az,
            _ => az
        };

    private static List<string> ParseBullets(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        return text
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.TrimStart('•', '-', '*', ' '))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    private static LocalizedTextDto MakeTr(string? az, string? en = null, string? ru = null) =>
        new(az, !string.IsNullOrWhiteSpace(en) ? en : az, !string.IsNullOrWhiteSpace(ru) ? ru : az);

    private static string? BuildActionUrl(string? actionUrl, string? actionType, string? actionValue)
    {
        if (!string.IsNullOrWhiteSpace(actionUrl)) return actionUrl;
        if (string.IsNullOrWhiteSpace(actionType) || string.IsNullOrWhiteSpace(actionValue)) return null;

        return actionType.ToLowerInvariant() switch
        {
            "phone" => actionValue.StartsWith("tel:", StringComparison.OrdinalIgnoreCase) ? actionValue : $"tel:{actionValue}",
            "whatsapp" => actionValue.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? actionValue : $"https://wa.me/{actionValue.TrimStart('+')}",
            "email" => actionValue.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ? actionValue : $"mailto:{actionValue}",
            "map" or "link" => actionValue,
            _ => actionValue
        };
    }

    private static string? ExtractFirstLineSubtitle(string? explicitSub, string? description)
    {
        if (!string.IsNullOrWhiteSpace(explicitSub)) return explicitSub;
        if (string.IsNullOrWhiteSpace(description)) return null;
        var firstNewLine = description.IndexOf('\n');
        if (firstNewLine > 0)
        {
            var firstLine = description[..firstNewLine].Trim();
            if (firstLine.Length <= 80) return firstLine;
        }
        return description.Length <= 120 ? description.Trim() : null;
    }

    private static PublicCatalogItemDto MapCatalogItem(
        CatalogItem i,
        string lang,
        CatalogCategory? category = null,
        HotelHub? hub = null)
    {
        var descAz = i.DescriptionAz;
        var descEn = !string.IsNullOrWhiteSpace(i.DescriptionEn) ? i.DescriptionEn : descAz;
        var descRu = !string.IsNullOrWhiteSpace(i.DescriptionRu) ? i.DescriptionRu : descAz;

        var subAz = ExtractFirstLineSubtitle(i.SubtitleAz, descAz);
        var subEn = ExtractFirstLineSubtitle(i.SubtitleEn, descEn) ?? subAz;
        var subRu = ExtractFirstLineSubtitle(i.SubtitleRu, descRu) ?? subAz;

        var resolvedActionUrl = BuildActionUrl(i.ActionUrl, i.ActionType, i.ActionValue);
        var filterTag = !string.IsNullOrWhiteSpace(i.FilterTag)
            ? i.FilterTag
            : category?.ExternalId?.ToString();

        return new PublicCatalogItemDto(
            i.Id,
            i.HubId,
            i.CategoryId,
            i.ItemType,
            Localize(lang, i.TitleAz, i.TitleEn, i.TitleRu) ?? i.TitleAz,
            Localize(lang, descAz, descEn, descRu),
            Localize(lang, i.BadgeAz, i.BadgeEn, i.BadgeRu),
            filterTag,
            i.Price,
            i.OldPrice,
            i.Currency,
            i.PortionOrDuration,
            ParseBullets(Localize(lang, i.BulletPointsAz, i.BulletPointsEn, i.BulletPointsRu)),
            i.ImageUrl,
            i.IsOrderable,
            i.IsFeaturedInDailySpecials,
            Localize(lang, i.ActionLabelAz, i.ActionLabelEn, i.ActionLabelRu),
            resolvedActionUrl,
            i.DisplayOrder,
            i.ExternalId,
            category?.ExternalId,
            hub?.ExternalId,
            Localize(lang, subAz, subEn, subRu),
            i.IconKey,
            i.ActionType,
            i.ActionValue,
            i.DiscountPercent,
            i.CostPrice,
            i.PrepTimeMinutes,
            i.NoServiceFee,
            MakeTr(i.TitleAz, i.TitleEn, i.TitleRu),
            MakeTr(subAz, subEn, subRu),
            MakeTr(descAz, descEn, descRu),
            MakeTr(i.BadgeAz, i.BadgeEn, i.BadgeRu),
            category != null ? MakeTr(category.TitleAz, category.TitleEn, category.TitleRu) : null,
            MakeTr(i.ActionLabelAz, i.ActionLabelEn, i.ActionLabelRu),
            new List<object>()
        );
    }

    private static PublicCatalogCategoryDto MapCatalogCategory(
        string slug,
        CatalogCategory c,
        IReadOnlyList<CatalogItem> categoryItems,
        string lang)
    {
        var itemsCount = categoryItems.Count;
        Guid? singleItemId = null;
        int? singleItemExtId = null;
        string? actionType = null;
        string? actionValue = null;
        string? iconKey = c.IconKey;

        var catIdentifier = c.ExternalId?.ToString() ?? c.Id.ToString();
        string targetUrl = $"/{slug}/hotel/c/{catIdentifier}";

        if (c.CollapseIfSingleItem && itemsCount == 1)
        {
            var single = categoryItems[0];
            singleItemId = single.Id;
            singleItemExtId = single.ExternalId;
            actionType = single.ActionType;
            actionValue = single.ActionValue;
            if (string.IsNullOrWhiteSpace(iconKey)) iconKey = single.IconKey;

            var directActionUrl = BuildActionUrl(single.ActionUrl, single.ActionType, single.ActionValue);
            // In UniPOS: if a 1-item category has a direct action AND no body text (like 43135 Call Reception or 43213 Call SPA), link directly to the action URL
            if (!string.IsNullOrWhiteSpace(directActionUrl) && string.IsNullOrWhiteSpace(single.DescriptionAz))
            {
                targetUrl = directActionUrl;
            }
            else
            {
                var itemIdentifier = single.ExternalId?.ToString() ?? single.Id.ToString();
                targetUrl = $"/{slug}/hotel/p/{itemIdentifier}";
            }
        }

        return new PublicCatalogCategoryDto(
            c.Id,
            c.HubId,
            c.ParentCategoryId,
            Localize(lang, c.TitleAz, c.TitleEn, c.TitleRu) ?? c.TitleAz,
            Localize(lang, c.SubtitleAz, c.SubtitleEn, c.SubtitleRu),
            c.ImageUrl,
            iconKey,
            c.WorkingHours,
            c.InternalExtension,
            c.LayoutStyle,
            c.DisplayOrder,
            itemsCount,
            c.ExternalId,
            c.CollapseIfSingleItem,
            singleItemId,
            singleItemExtId,
            targetUrl,
            actionType,
            actionValue,
            MakeTr(c.TitleAz, c.TitleEn, c.TitleRu),
            MakeTr(c.SubtitleAz, c.SubtitleEn, c.SubtitleRu)
        );
    }

    public async Task<PublicHotelPortalDto> GetPublicPortalAsync(
        string slug,
        string? lang,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, includeFullDetails: true, cancellationToken);
        if (tenant == null)
        {
            throw new NotFoundException("Otel tapılmadı.");
        }

        // KILL-SWITCH / SUBSCRIPTION VERIFICATION:
        if (!tenant.CanServePublicTraffic())
        {
            throw new TenantSuspendedException(
                tenant.SuspensionReason ?? "Bu otelin rəqəmsal konsyerj xidməti müvəqqəti olaraq dayandırılmışdır.");
        }

        // Record scan / view analytics
        await RecordAnalyticsAsync(tenant.Id, null, AnalyticsEventType.ProfileView, ipAddress, userAgent);

        var selectedLang = !string.IsNullOrWhiteSpace(lang) ? lang.ToLowerInvariant() : tenant.DefaultLanguage;
        var bio = Localize(selectedLang, tenant.BioAz, tenant.BioEn, tenant.BioRu);
        var subtitle = Localize(selectedLang, tenant.SubtitleAz, tenant.SubtitleEn, tenant.SubtitleRu);
        var tagline = Localize(selectedLang, tenant.TaglineAz, tenant.TaglineEn, tenant.TaglineRu) ?? "Hotel & Spa";

        // 1. Existing Link Categories & Links
        var categories = tenant.LinkCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c =>
            {
                var title = Localize(selectedLang, c.TitleAz, c.TitleEn, c.TitleRu) ?? c.TitleAz;
                var links = c.Links
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.DisplayOrder)
                    .Select(l =>
                    {
                        var linkTitle = Localize(selectedLang, l.TitleAz, l.TitleEn, l.TitleRu) ?? l.TitleAz;
                        var actionUrl = $"/api/r/{l.Id}";
                        return new PublicLinkDto(
                            l.Id,
                            linkTitle,
                            l.LinkType,
                            actionUrl,
                            l.IconKey,
                            l.IsCustomIcon,
                            l.OpenInNewTab,
                            l.DisplayOrder
                        );
                    }).ToList();

                return new PublicCategoryDto(c.Id, title, c.LayoutType, c.DisplayOrder, links);
            }).ToList();

        // 2. Existing Activities
        var activities = (await _unitOfWork.Activities.FindAsync(a => a.TenantId == tenant.Id && a.IsActive, cancellationToken))
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.StartTime)
            .Select(a => new PublicActivityDto(
                a.Id,
                Localize(selectedLang, a.TitleAz, a.TitleEn, a.TitleRu) ?? a.TitleAz,
                a.Description,
                a.Location,
                a.StartTime,
                a.EndTime,
                a.Date,
                a.Category,
                a.ImageUrl
            )).ToList();

        // 3. Skyhill Top Stories / Highlights (with VideoUrl & Trilingual ButtonText)
        var stories = (await _unitOfWork.Stories.FindAsync(s => s.TenantId == tenant.Id && s.IsActive, cancellationToken))
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PublicStoryDto(
                s.Id,
                Localize(selectedLang, s.TitleAz, s.TitleEn, s.TitleRu) ?? s.TitleAz,
                Localize(selectedLang, s.DescriptionAz, s.DescriptionEn, s.DescriptionRu),
                s.ImageUrl,
                s.BannerImageUrl,
                s.BadgeText,
                s.ActionUrl,
                s.DisplayOrder,
                s.ExternalId,
                s.VideoUrl,
                Localize(selectedLang, s.ButtonTextAz, s.ButtonTextEn, s.ButtonTextRu),
                MakeTr(s.TitleAz, s.TitleEn, s.TitleRu),
                MakeTr(s.DescriptionAz, s.DescriptionEn, s.DescriptionRu),
                MakeTr(s.ButtonTextAz, s.ButtonTextEn, s.ButtonTextRu)
            )).ToList();

        // 4. Skyhill 6 Main Hubs
        var hubsEntities = (await _unitOfWork.Hubs.FindAsync(h => h.TenantId == tenant.Id && h.IsActive, cancellationToken))
            .OrderBy(h => h.DisplayOrder)
            .ToList();

        var hubs = hubsEntities
            .Select(h => new PublicHubSummaryDto(
                h.Id,
                h.Code,
                h.HubType,
                Localize(selectedLang, h.KickerAz, h.KickerEn, h.KickerRu),
                Localize(selectedLang, h.TitleAz, h.TitleEn, h.TitleRu) ?? h.TitleAz,
                Localize(selectedLang, h.SubtitleAz, h.SubtitleEn, h.SubtitleRu),
                h.IconKey,
                h.CoverImageUrl,
                h.DisplayOrder,
                h.ExternalId,
                h.DisplayMode,
                MakeTr(h.KickerAz, h.KickerEn, h.KickerRu),
                MakeTr(h.TitleAz, h.TitleEn, h.TitleRu),
                MakeTr(h.SubtitleAz, h.SubtitleEn, h.SubtitleRu)
            )).ToList();

        // 5. Featured Daily Specials (including items in Hub 196 / DailySpecials)
        var specialsHubIds = hubsEntities.Where(h => h.HubType == HubType.DailySpecials).Select(h => h.Id).ToHashSet();
        var specials = (await _unitOfWork.CatalogItems.FindAsync(
                i => i.TenantId == tenant.Id && i.IsActive && (i.IsFeaturedInDailySpecials || specialsHubIds.Contains(i.HubId)), cancellationToken))
            .OrderBy(i => i.DisplayOrder)
            .Select(i => MapCatalogItem(i, selectedLang))
            .ToList();

        // 6. Department Contacts
        var departments = (await _unitOfWork.DepartmentContacts.FindAsync(
                d => d.TenantId == tenant.Id && d.IsActive, cancellationToken))
            .OrderBy(d => d.DisplayOrder)
            .Select(d => new PublicDepartmentContactDto(
                d.Id,
                Localize(selectedLang, d.TitleAz, d.TitleEn, d.TitleRu) ?? d.TitleAz,
                Localize(selectedLang, d.SubtitleAz, d.SubtitleEn, d.SubtitleRu),
                d.PhoneNumber,
                d.InternalExtension,
                d.WhatsAppNumber,
                d.Email,
                d.WorkingHours,
                d.IconKey,
                d.DisplayOrder,
                MakeTr(d.TitleAz, d.TitleEn, d.TitleRu),
                MakeTr(d.SubtitleAz, d.SubtitleEn, d.SubtitleRu)
            )).ToList();

        return new PublicHotelPortalDto(
            tenant.Name,
            tenant.Slug,
            bio,
            tenant.LogoUrl,
            tenant.CoverImageUrl,
            tenant.PrimaryColor,
            tenant.ThemeMode,
            tenant.Phone,
            tenant.Email,
            tenant.WhatsApp,
            tenant.Telegram,
            tenant.Address,
            tenant.MapUrl,
            categories,
            activities,
            subtitle,
            tenant.StarRating,
            stories,
            hubs,
            specials,
            departments,
            tagline,
            MakeTr(tenant.SubtitleAz, tenant.SubtitleEn, tenant.SubtitleRu),
            MakeTr(tenant.TaglineAz ?? "Hotel & Spa", tenant.TaglineEn ?? "Hotel & Spa", tenant.TaglineRu ?? "Отель и спа"),
            MakeTr(tenant.BioAz, tenant.BioEn, tenant.BioRu),
            tenant.CurrencyCode,
            tenant.ServiceFeePercent,
            tenant.ThemePreset,
            tenant.ThemeStyle,
            tenant.InstagramUrl,
            tenant.FacebookUrl,
            tenant.YoutubeUrl,
            tenant.TikTokUrl,
            tenant.TripAdvisorUrl,
            tenant.BookingUrl
        );
    }

    public async Task<PublicFullCatalogDto> GetFullCatalogAsync(
        string slug,
        string? lang,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");
        if (!tenant.CanServePublicTraffic())
            throw new TenantSuspendedException(tenant.SuspensionReason ?? "Bu otelin xidməti aktiv deyil.");

        var selectedLang = !string.IsNullOrWhiteSpace(lang) ? lang.ToLowerInvariant() : tenant.DefaultLanguage;

        var hubs = (await _unitOfWork.Hubs.FindAsync(h => h.TenantId == tenant.Id && h.IsActive, cancellationToken))
            .OrderBy(h => h.DisplayOrder)
            .ToList();

        var categories = (await _unitOfWork.CatalogCategories.FindAsync(c => c.TenantId == tenant.Id && c.IsActive, cancellationToken))
            .OrderBy(c => c.ExternalId ?? int.MaxValue)
            .ThenBy(c => c.DisplayOrder)
            .ToList();

        var items = (await _unitOfWork.CatalogItems.FindAsync(i => i.TenantId == tenant.Id && i.IsActive, cancellationToken))
            .OrderBy(i => i.ExternalId ?? int.MaxValue)
            .ThenBy(i => i.DisplayOrder)
            .ToList();

        var stories = (await _unitOfWork.Stories.FindAsync(s => s.TenantId == tenant.Id && s.IsActive, cancellationToken))
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new PublicStoryDto(
                s.Id,
                Localize(selectedLang, s.TitleAz, s.TitleEn, s.TitleRu) ?? s.TitleAz,
                Localize(selectedLang, s.DescriptionAz, s.DescriptionEn, s.DescriptionRu),
                s.ImageUrl,
                s.BannerImageUrl,
                s.BadgeText,
                s.ActionUrl,
                s.DisplayOrder,
                s.ExternalId,
                s.VideoUrl,
                Localize(selectedLang, s.ButtonTextAz, s.ButtonTextEn, s.ButtonTextRu),
                MakeTr(s.TitleAz, s.TitleEn, s.TitleRu),
                MakeTr(s.DescriptionAz, s.DescriptionEn, s.DescriptionRu),
                MakeTr(s.ButtonTextAz, s.ButtonTextEn, s.ButtonTextRu)
            )).ToList();

        var catById = categories.ToDictionary(c => c.Id);

        var uniPosProducts = items.Select((i, idx) =>
        {
            var cat = i.CategoryId.HasValue && catById.TryGetValue(i.CategoryId.Value, out var foundCat) ? foundCat : null;
            var catExtId = cat?.ExternalId ?? 0;
            var catNameAz = cat?.TitleAz ?? string.Empty;
            var catNameTr = cat != null
                ? MakeTr(cat.TitleAz, cat.TitleEn, cat.TitleRu)
                : MakeTr(string.Empty, string.Empty, string.Empty);

            return new UniPosProductDto(
                i.ExternalId ?? (43107 + idx),
                i.Id,
                i.TitleAz,
                MakeTr(i.TitleAz, i.TitleEn, i.TitleRu),
                i.Price ?? 0m,
                i.CostPrice,
                i.PrepTimeMinutes,
                i.ImageUrl,
                i.DescriptionAz,
                i.DiscountPercent,
                i.NoServiceFee,
                catExtId,
                i.CategoryId,
                catNameAz,
                catNameTr,
                i.DisplayOrder,
                i.IconKey,
                new LocalizedTextDto(i.BadgeAz, i.BadgeEn, i.BadgeRu),
                i.ActionType,
                i.ActionValue,
                !i.IsOrderable,
                new List<object>()
            );
        }).ToList();

        var categoriesDto = categories
            .Select(c =>
            {
                var catItems = items.Where(i => i.CategoryId == c.Id).OrderBy(i => i.DisplayOrder).ToList();
                return MapCatalogCategory(tenant.Slug, c, catItems, selectedLang);
            }).ToList();

        var hubsDto = hubs
            .Select(h => new PublicHubSummaryDto(
                h.Id,
                h.Code,
                h.HubType,
                Localize(selectedLang, h.KickerAz, h.KickerEn, h.KickerRu),
                Localize(selectedLang, h.TitleAz, h.TitleEn, h.TitleRu) ?? h.TitleAz,
                Localize(selectedLang, h.SubtitleAz, h.SubtitleEn, h.SubtitleRu),
                h.IconKey,
                h.CoverImageUrl,
                h.DisplayOrder,
                h.ExternalId,
                h.DisplayMode,
                MakeTr(h.KickerAz, h.KickerEn, h.KickerRu),
                MakeTr(h.TitleAz, h.TitleEn, h.TitleRu),
                MakeTr(h.SubtitleAz, h.SubtitleEn, h.SubtitleRu)
            )).ToList();

        return new PublicFullCatalogDto(
            tenant.Slug,
            tenant.ServiceFeePercent,
            tenant.CurrencyCode,
            uniPosProducts,
            categoriesDto,
            hubsDto,
            stories
        );
    }

    public async Task<PublicHubDetailDto> GetHubDetailAsync(
        string slug,
        string hubCodeOrId,
        string? lang,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");
        if (!tenant.CanServePublicTraffic())
            throw new TenantSuspendedException(tenant.SuspensionReason ?? "Bu otelin xidməti aktiv deyil.");

        var selectedLang = !string.IsNullOrWhiteSpace(lang) ? lang.ToLowerInvariant() : tenant.DefaultLanguage;

        HotelHub? hub;
        if (Guid.TryParse(hubCodeOrId, out var hubGuid))
        {
            hub = (await _unitOfWork.Hubs.FindAsync(
                h => h.TenantId == tenant.Id && h.Id == hubGuid && h.IsActive, cancellationToken)).FirstOrDefault();
        }
        else if (int.TryParse(hubCodeOrId, out var hubExtId))
        {
            hub = (await _unitOfWork.Hubs.FindAsync(
                h => h.TenantId == tenant.Id && (h.ExternalId == hubExtId || h.Code == hubCodeOrId) && h.IsActive, cancellationToken)).FirstOrDefault();
        }
        else
        {
            var cleanCode = hubCodeOrId.Trim().ToLowerInvariant();
            hub = (await _unitOfWork.Hubs.FindAsync(
                h => h.TenantId == tenant.Id && h.Code.ToLower() == cleanCode && h.IsActive, cancellationToken)).FirstOrDefault();
        }

        if (hub == null) throw new NotFoundException("Bölmə tapılmadı.");

        var allHubCategories = (await _unitOfWork.CatalogCategories.FindAsync(
            c => c.TenantId == tenant.Id && c.HubId == hub.Id && c.IsActive && c.ParentCategoryId == null,
            cancellationToken))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.ExternalId ?? int.MaxValue)
            .ToList();

        var allHubItems = (await _unitOfWork.CatalogItems.FindAsync(
            i => i.TenantId == tenant.Id && (i.HubId == hub.Id || (hub.HubType == HubType.DailySpecials && i.IsFeaturedInDailySpecials)) && i.IsActive,
            cancellationToken))
            .OrderBy(i => i.ExternalId ?? int.MaxValue)
            .ThenBy(i => i.DisplayOrder)
            .ToList();

        var catById = allHubCategories.ToDictionary(c => c.Id);

        var categoriesDto = allHubCategories
            .Select(c =>
            {
                var catItems = allHubItems.Where(i => i.CategoryId == c.Id).ToList();
                return MapCatalogCategory(tenant.Slug, c, catItems, selectedLang);
            }).ToList();

        var includeAllHubItemsAsDirect =
            hub.DisplayMode == "filter_items" ||
            hub.DisplayMode == "direct_items" ||
            hub.HubType == HubType.DailySpecials ||
            hub.HubType == HubType.Entertainment ||
            hub.HubType == HubType.Contacts;

        var directItemsDto = allHubItems
            .Where(i => i.CategoryId == null || includeAllHubItemsAsDirect)
            .Select(i =>
            {
                var cat = i.CategoryId.HasValue && catById.TryGetValue(i.CategoryId.Value, out var foundCat) ? foundCat : null;
                return MapCatalogItem(i, selectedLang, cat, hub);
            })
            .ToList();

        List<PublicDepartmentContactDto>? deptContacts = null;
        if (hub.HubType == HubType.Contacts)
        {
            var depts = await _unitOfWork.DepartmentContacts.FindAsync(
                d => d.TenantId == tenant.Id && d.IsActive, cancellationToken);
            deptContacts = depts
                .OrderBy(d => d.DisplayOrder)
                .Select(d => new PublicDepartmentContactDto(
                    d.Id,
                    Localize(selectedLang, d.TitleAz, d.TitleEn, d.TitleRu) ?? d.TitleAz,
                    Localize(selectedLang, d.SubtitleAz, d.SubtitleEn, d.SubtitleRu),
                    d.PhoneNumber,
                    d.InternalExtension,
                    d.WhatsAppNumber,
                    d.Email,
                    d.WorkingHours,
                    d.IconKey,
                    d.DisplayOrder,
                    MakeTr(d.TitleAz, d.TitleEn, d.TitleRu),
                    MakeTr(d.SubtitleAz, d.SubtitleEn, d.SubtitleRu)
                )).ToList();
        }

        return new PublicHubDetailDto(
            hub.Id,
            hub.Code,
            hub.HubType,
            Localize(selectedLang, hub.KickerAz, hub.KickerEn, hub.KickerRu),
            Localize(selectedLang, hub.TitleAz, hub.TitleEn, hub.TitleRu) ?? hub.TitleAz,
            Localize(selectedLang, hub.SubtitleAz, hub.SubtitleEn, hub.SubtitleRu),
            hub.IconKey,
            hub.CoverImageUrl,
            categoriesDto,
            directItemsDto,
            deptContacts,
            hub.ExternalId,
            hub.DisplayMode,
            MakeTr(hub.KickerAz, hub.KickerEn, hub.KickerRu),
            MakeTr(hub.TitleAz, hub.TitleEn, hub.TitleRu),
            MakeTr(hub.SubtitleAz, hub.SubtitleEn, hub.SubtitleRu)
        );
    }

    public Task<PublicCatalogCategoryDetailDto> GetCatalogCategoryDetailAsync(
        string slug,
        Guid categoryId,
        string? lang,
        CancellationToken cancellationToken = default) =>
        GetCatalogCategoryDetailAsync(slug, categoryId.ToString(), lang, cancellationToken);

    public async Task<PublicCatalogCategoryDetailDto> GetCatalogCategoryDetailAsync(
        string slug,
        string categoryCodeOrId,
        string? lang,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");
        if (!tenant.CanServePublicTraffic())
            throw new TenantSuspendedException(tenant.SuspensionReason ?? "Bu otelin xidməti aktiv deyil.");

        var selectedLang = !string.IsNullOrWhiteSpace(lang) ? lang.ToLowerInvariant() : tenant.DefaultLanguage;

        CatalogCategory? category;
        if (Guid.TryParse(categoryCodeOrId, out var catGuid))
        {
            category = await _unitOfWork.CatalogCategories.GetByIdAsync(catGuid, cancellationToken);
        }
        else if (int.TryParse(categoryCodeOrId, out var catExtId))
        {
            category = (await _unitOfWork.CatalogCategories.FindAsync(
                c => c.TenantId == tenant.Id && c.ExternalId == catExtId && c.IsActive, cancellationToken)).FirstOrDefault();
        }
        else
        {
            var clean = categoryCodeOrId.Trim().ToLowerInvariant();
            category = (await _unitOfWork.CatalogCategories.FindAsync(
                c => c.TenantId == tenant.Id && c.TitleAz.ToLower() == clean && c.IsActive, cancellationToken)).FirstOrDefault();
        }

        if (category == null || category.TenantId != tenant.Id || !category.IsActive)
            throw new NotFoundException("Kateqoriya tapılmadı.");

        var hub = await _unitOfWork.Hubs.GetByIdAsync(category.HubId, cancellationToken);

        var subCategories = await _unitOfWork.CatalogCategories.FindAsync(
            c => c.TenantId == tenant.Id && c.ParentCategoryId == category.Id && c.IsActive,
            cancellationToken);

        var subCategoryIds = subCategories.Select(sc => sc.Id).ToHashSet();

        var items = (await _unitOfWork.CatalogItems.FindAsync(
            i => i.TenantId == tenant.Id && i.IsActive &&
                 (i.CategoryId == category.Id || (i.CategoryId.HasValue && subCategoryIds.Contains(i.CategoryId.Value))),
            cancellationToken))
            .OrderBy(i => i.DisplayOrder)
            .ThenBy(i => i.ExternalId ?? int.MaxValue)
            .ToList();

        var subCategoriesDto = subCategories
            .OrderBy(sc => sc.DisplayOrder)
            .Select(sc =>
            {
                var scItems = items.Where(i => i.CategoryId == sc.Id).ToList();
                return MapCatalogCategory(tenant.Slug, sc, scItems, selectedLang);
            }).ToList();

        var itemsDto = items
            .Select(i => MapCatalogItem(i, selectedLang, category, hub))
            .ToList();

        return new PublicCatalogCategoryDetailDto(
            category.Id,
            category.HubId,
            hub != null ? (Localize(selectedLang, hub.TitleAz, hub.TitleEn, hub.TitleRu) ?? hub.TitleAz) : string.Empty,
            Localize(selectedLang, category.TitleAz, category.TitleEn, category.TitleRu) ?? category.TitleAz,
            Localize(selectedLang, category.SubtitleAz, category.SubtitleEn, category.SubtitleRu),
            category.ImageUrl,
            category.WorkingHours,
            category.InternalExtension,
            category.LayoutStyle,
            subCategoriesDto,
            itemsDto,
            category.ExternalId,
            hub?.ExternalId,
            hub?.Code,
            MakeTr(category.TitleAz, category.TitleEn, category.TitleRu),
            MakeTr(category.SubtitleAz, category.SubtitleEn, category.SubtitleRu),
            hub != null ? MakeTr(hub.TitleAz, hub.TitleEn, hub.TitleRu) : null
        );
    }

    public Task<PublicCatalogItemDto> GetCatalogItemDetailAsync(
        string slug,
        Guid itemId,
        string? lang,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default) =>
        GetCatalogItemDetailAsync(slug, itemId.ToString(), lang, ipAddress, userAgent, cancellationToken);

    public async Task<PublicCatalogItemDto> GetCatalogItemDetailAsync(
        string slug,
        string itemCodeOrId,
        string? lang,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");
        if (!tenant.CanServePublicTraffic())
            throw new TenantSuspendedException(tenant.SuspensionReason ?? "Bu otelin xidməti aktiv deyil.");

        CatalogItem? item;
        if (Guid.TryParse(itemCodeOrId, out var itemGuid))
        {
            item = await _unitOfWork.CatalogItems.GetByIdAsync(itemGuid, cancellationToken);
        }
        else if (int.TryParse(itemCodeOrId, out var itemExtId))
        {
            item = (await _unitOfWork.CatalogItems.FindAsync(
                i => i.TenantId == tenant.Id && i.ExternalId == itemExtId && i.IsActive, cancellationToken)).FirstOrDefault();
        }
        else
        {
            item = null;
        }

        if (item == null || item.TenantId != tenant.Id || !item.IsActive)
            throw new NotFoundException("Məhsul və ya məlumat tapılmadı.");

        var category = item.CategoryId.HasValue
            ? await _unitOfWork.CatalogCategories.GetByIdAsync(item.CategoryId.Value, cancellationToken)
            : null;
        var hub = await _unitOfWork.Hubs.GetByIdAsync(item.HubId, cancellationToken);

        await RecordAnalyticsAsync(tenant.Id, null, AnalyticsEventType.CatalogItemView, ipAddress, userAgent);

        var selectedLang = !string.IsNullOrWhiteSpace(lang) ? lang.ToLowerInvariant() : tenant.DefaultLanguage;
        return MapCatalogItem(item, selectedLang, category, hub);
    }

    public async Task<GuestOrderDto> SubmitOrderAsync(
        string slug,
        SubmitOrderRequestDto request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");
        if (!tenant.CanServePublicTraffic())
            throw new TenantSuspendedException(tenant.SuspensionReason ?? "Bu otelin xidməti aktiv deyil.");

        if (request.Items == null || request.Items.Count == 0)
            throw new BadRequestException("Səbət boşdur. Sifariş üçün ən azı 1 məhsul seçilməlidir.");

        var randomSuffix = Random.Shared.Next(1000, 9999);
        var orderNumber = $"ORD-{DateTime.UtcNow:MMdd}-{randomSuffix}";

        var order = new GuestOrder
        {
            TenantId = tenant.Id,
            OrderNumber = orderNumber,
            OrderType = request.OrderType,
            RoomOrTableNumber = request.RoomOrTableNumber.Trim(),
            GuestName = request.GuestName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Notes = request.Notes?.Trim(),
            Currency = "₼",
            Status = OrderStatus.New
        };

        decimal total = 0;
        foreach (var reqItem in request.Items)
        {
            var qty = Math.Max(1, reqItem.Quantity);
            var lineTotal = qty * reqItem.UnitPrice;
            total += lineTotal;

            order.Items.Add(new GuestOrderItem
            {
                OrderId = order.Id,
                CatalogItemId = reqItem.CatalogItemId,
                ItemTitle = reqItem.ItemTitle.Trim(),
                Quantity = qty,
                UnitPrice = reqItem.UnitPrice,
                LineTotal = lineTotal,
                SpecialInstruction = reqItem.SpecialInstruction?.Trim()
            });
        }

        order.TotalAmount = total;

        await _unitOfWork.GuestOrders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationDispatcher.DispatchOrderNotificationAsync(tenant, order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RecordAnalyticsAsync(tenant.Id, null, AnalyticsEventType.OrderSubmitted, ipAddress, userAgent);

        return new GuestOrderDto(
            order.Id,
            order.OrderNumber,
            order.OrderType,
            order.RoomOrTableNumber,
            order.GuestName,
            order.PhoneNumber,
            order.Notes,
            order.TotalAmount,
            order.Currency,
            order.Status,
            order.IsEmailSent,
            order.IsTelegramSent,
            order.CreatedAtUtc,
            order.Items.Select(i => new GuestOrderItemDto(
                i.Id, i.CatalogItemId, i.ItemTitle, i.Quantity, i.UnitPrice, i.LineTotal, i.SpecialInstruction
            )).ToList()
        );
    }

    public async Task<bool> SubmitReviewAsync(
        string slug,
        SubmitReviewRequestDto request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");
        if (!tenant.CanServePublicTraffic())
            throw new TenantSuspendedException(tenant.SuspensionReason ?? "Bu otelin xidməti aktiv deyil.");

        var review = new GuestReview
        {
            TenantId = tenant.Id,
            Rating = Math.Clamp(request.Rating, 1, 5),
            GuestName = request.GuestName?.Trim(),
            RoomNumber = request.RoomNumber?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Department = request.Department?.Trim() ?? "General",
            Comment = request.Comment?.Trim()
        };

        await _unitOfWork.GuestReviews.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationDispatcher.DispatchReviewNotificationAsync(tenant, review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RecordAnalyticsAsync(tenant.Id, null, AnalyticsEventType.ReviewSubmitted, ipAddress, userAgent);

        return true;
    }

    public async Task<string> GetPwaManifestJsonAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var manifest = new
        {
            id = $"/{tenant.Slug}",
            name = tenant.Name,
            short_name = tenant.Name,
            description = tenant.BioAz ?? $"{tenant.Name} — menyu, qiymətlər və rəqəmsal konsyerj.",
            start_url = $"/{tenant.Slug}",
            scope = $"/{tenant.Slug}",
            display = "standalone",
            display_override = new[] { "standalone", "minimal-ui" },
            background_color = "#F6F1E8",
            theme_color = !string.IsNullOrWhiteSpace(tenant.PrimaryColor) ? tenant.PrimaryColor : "#2A1D14",
            lang = tenant.DefaultLanguage,
            dir = "ltr",
            categories = new[] { "food", "shopping", "travel" },
            icons = new object[]
            {
                new { src = tenant.LogoUrl ?? "/assets/company-logo.png", sizes = "192x192", type = "image/png" },
                new { src = tenant.LogoUrl ?? "/assets/company-logo.png", sizes = "512x512", type = "image/png" }
            },
            shortcuts = new object[]
            {
                new
                {
                    name = "Sevimlilər",
                    url = $"/{tenant.Slug}/favourites",
                    icons = new[] { new { src = tenant.LogoUrl ?? "/assets/company-logo.png", sizes = "192x192", type = "image/png" } }
                },
                new
                {
                    name = "Səbət",
                    url = $"/{tenant.Slug}/cart",
                    icons = new[] { new { src = tenant.LogoUrl ?? "/assets/company-logo.png", sizes = "192x192", type = "image/png" } }
                }
            }
        };

        return JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<IReadOnlyList<PublicActivityDto>> GetActivitiesAsync(
        string slug,
        string? lang,
        DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        if (!tenant.CanServePublicTraffic())
        {
            throw new TenantSuspendedException("Bu otelin xidməti aktiv deyil.");
        }

        var targetDate = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : (DateTime?)null;
        var selectedLang = !string.IsNullOrWhiteSpace(lang) ? lang.ToLowerInvariant() : tenant.DefaultLanguage;

        var activities = await _unitOfWork.Activities.FindAsync(
            a => a.TenantId == tenant.Id && a.IsActive && (!targetDate.HasValue || a.Date.Date == targetDate.Value.Date),
            cancellationToken);

        return activities
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.StartTime)
            .Select(a =>
            {
                var title = Localize(selectedLang, a.TitleAz, a.TitleEn, a.TitleRu) ?? a.TitleAz;
                return new PublicActivityDto(
                    a.Id,
                    title,
                    a.Description,
                    a.Location,
                    a.StartTime,
                    a.EndTime,
                    a.Date,
                    a.Category,
                    a.ImageUrl
                );
            }).ToList();
    }

    public async Task<(byte[] VCardBytes, string FileName)> GetVCardAsync(
        string slug,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        if (!tenant.CanServePublicTraffic())
        {
            throw new TenantSuspendedException(
                tenant.SuspensionReason ?? "Bu otelin rəqəmsal xidmət abunəliyi aktiv deyil.");
        }

        await RecordAnalyticsAsync(tenant.Id, null, AnalyticsEventType.VCardDownload, ipAddress, userAgent);

        var bytes = _vCardService.GenerateVCardBytes(tenant);
        var cleanFileName = $"{tenant.Slug}.vcf";

        return (bytes, cleanFileName);
    }

    public async Task<bool> SubmitInquiryAsync(
        string slug,
        SubmitInquiryRequestDto request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        if (!tenant.CanServePublicTraffic())
        {
            throw new TenantSuspendedException(
                tenant.SuspensionReason ?? "Bu otelin rəqəmsal xidmət abunəliyi aktiv deyil.");
        }

        var inquiry = new GuestInquiry
        {
            TenantId = tenant.Id,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Email = request.Email?.Trim(),
            RoomNumber = request.RoomNumber?.Trim(),
            Message = request.Message.Trim(),
            Status = InquiryStatus.New
        };

        await _unitOfWork.GuestInquiries.AddAsync(inquiry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch to Email and/or Telegram based on hotel admin preferences
        await _notificationDispatcher.DispatchInquiryNotificationAsync(tenant, inquiry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await RecordAnalyticsAsync(tenant.Id, null, AnalyticsEventType.InquirySubmitted, ipAddress, userAgent);

        return true;
    }

    public async Task<string> ProcessRedirectAsync(
        Guid linkId,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.LinkItems.GetWithCategoryAsync(linkId, cancellationToken);
        if (link == null || !link.IsActive)
        {
            throw new NotFoundException("Keçid tapılmadı və ya deaktivdir.");
        }

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(link.TenantId, cancellationToken);
        if (tenant == null || !tenant.CanServePublicTraffic())
        {
            throw new TenantSuspendedException("Bu otelin xidməti aktiv deyil.");
        }

        // Increment click count & record event
        await _unitOfWork.LinkItems.IncrementClickCountAsync(linkId, cancellationToken);
        await RecordAnalyticsAsync(tenant.Id, linkId, AnalyticsEventType.LinkClick, ipAddress, userAgent);

        return link.TargetUrl;
    }

    public async Task<string> ProcessQrScanRedirectAsync(string slug, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null || tenant.IsDeleted)
        {
            throw new NotFoundException("Otel tapılmadı.");
        }

        // 1. Qonağın QR kod skanını analitikaya yaz (+1 scan)
        await RecordAnalyticsAsync(tenant.Id, null, AnalyticsEventType.QrCodeScan, ipAddress, userAgent);

        // 2. Əgər otelin xüsusi domeni varsa (məs: hilton.az)
        if (!string.IsNullOrWhiteSpace(tenant.CustomDomain))
        {
            return tenant.CustomDomain.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? tenant.CustomDomain
                : $"https://{tenant.CustomDomain}";
        }

        // 3. Konfiqurasiyadakı Frontend BaseUrl
        var frontendBaseUrl = _configuration["FrontendSettings:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            return $"{frontendBaseUrl.TrimEnd('/')}/{tenant.Slug}";
        }

        // 4. Default olaraq public API səhifəsi
        return $"/api/public/{tenant.Slug}";
    }

    public async Task<byte[]> GetQrPngAsync(string slug, string baseUrl, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var fullPortalUrl = ResolveQrUrl(tenant, baseUrl);
        var qrSetting = (await _unitOfWork.QrSettings.FindAsync(q => q.TenantId == tenant.Id, cancellationToken)).FirstOrDefault();

        return _qrCodeService.GeneratePngQrCode(
            fullPortalUrl,
            qrSetting?.PixelsPerModule ?? 20,
            qrSetting?.ForegroundColorHex,
            qrSetting?.BackgroundColorHex
        );
    }

    public async Task<string> GetQrSvgAsync(string slug, string baseUrl, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetBySlugAsync(slug, cancellationToken: cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var fullPortalUrl = ResolveQrUrl(tenant, baseUrl);
        var qrSetting = (await _unitOfWork.QrSettings.FindAsync(q => q.TenantId == tenant.Id, cancellationToken)).FirstOrDefault();

        return _qrCodeService.GenerateSvgQrCode(
            fullPortalUrl,
            qrSetting?.ForegroundColorHex,
            qrSetting?.BackgroundColorHex
        );
    }

    private string ResolveQrUrl(Tenant tenant, string baseUrl)
    {
        var directLink = _configuration.GetValue<bool>("FrontendSettings:DirectQrLink");
        var frontendBaseUrl = _configuration["FrontendSettings:BaseUrl"];

        // Əgər birbaşa frontend linki tələb olunubsa və təyin edilibsə
        if (directLink && !string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            return $"{frontendBaseUrl.TrimEnd('/')}/{tenant.Slug}";
        }

        // Dinamik və analitika toplayan yönləndirmə linki (/q/{slug})
        return $"{baseUrl.TrimEnd('/')}/q/{tenant.Slug}";
    }

    private async Task RecordAnalyticsAsync(
        Guid tenantId,
        Guid? linkId,
        AnalyticsEventType eventType,
        string? ipAddress,
        string? userAgent)
    {
        try
        {
            var device = DetermineDevice(userAgent);
            var analyticsEvent = new AnalyticsEvent
            {
                TenantId = tenantId,
                LinkItemId = linkId,
                EventType = eventType,
                IpAddress = ipAddress,
                UserAgent = userAgent != null && userAgent.Length > 300 ? userAgent[..300] : userAgent,
                DeviceType = device
            };

            await _unitOfWork.Analytics.AddAsync(analyticsEvent);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // Analytics failures must not break the user experience
        }
    }

    private static string DetermineDevice(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return "Desktop";
        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone")) return "Mobile";
        if (ua.Contains("tablet") || ua.Contains("ipad")) return "Tablet";
        return "Desktop";
    }
}
