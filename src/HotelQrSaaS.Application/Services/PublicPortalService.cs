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

        var bio = selectedLang switch
        {
            "en" => !string.IsNullOrWhiteSpace(tenant.BioEn) ? tenant.BioEn : tenant.BioAz,
            "ru" => !string.IsNullOrWhiteSpace(tenant.BioRu) ? tenant.BioRu : tenant.BioAz,
            _ => tenant.BioAz
        };

        var categories = tenant.LinkCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c =>
            {
                var title = selectedLang switch
                {
                    "en" => !string.IsNullOrWhiteSpace(c.TitleEn) ? c.TitleEn : c.TitleAz,
                    "ru" => !string.IsNullOrWhiteSpace(c.TitleRu) ? c.TitleRu : c.TitleAz,
                    _ => c.TitleAz
                };

                var links = c.Links
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.DisplayOrder)
                    .Select(l =>
                    {
                        var linkTitle = selectedLang switch
                        {
                            "en" => !string.IsNullOrWhiteSpace(l.TitleEn) ? l.TitleEn : l.TitleAz,
                            "ru" => !string.IsNullOrWhiteSpace(l.TitleRu) ? l.TitleRu : l.TitleAz,
                            _ => l.TitleAz
                        };

                        // Use redirect tracking route for links
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

        var activities = (await _unitOfWork.Activities.FindAsync(a => a.TenantId == tenant.Id && a.IsActive, cancellationToken))
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.StartTime)
            .Select(a =>
            {
                var title = selectedLang switch
                {
                    "en" => !string.IsNullOrWhiteSpace(a.TitleEn) ? a.TitleEn : a.TitleAz,
                    "ru" => !string.IsNullOrWhiteSpace(a.TitleRu) ? a.TitleRu : a.TitleAz,
                    _ => a.TitleAz
                };
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
            activities
        );
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
                var title = selectedLang switch
                {
                    "en" => !string.IsNullOrWhiteSpace(a.TitleEn) ? a.TitleEn : a.TitleAz,
                    "ru" => !string.IsNullOrWhiteSpace(a.TitleRu) ? a.TitleRu : a.TitleAz,
                    _ => a.TitleAz
                };
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

