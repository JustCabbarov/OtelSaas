using HotelQrSaaS.Application.Common.Exceptions;
using HotelQrSaaS.Application.DTOs;
using HotelQrSaaS.Application.Interfaces.Services;
using HotelQrSaaS.Application.Interfaces.UnitOfWork;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Application.Services;

public interface IHotelAdminService
{
    // Profile & Visuals
    Task<HotelProfileDto> GetProfileAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<HotelProfileDto> UpdateProfileAsync(Guid tenantId, UpdateHotelProfileDto request, CancellationToken cancellationToken = default);
    Task<string> UploadLogoAsync(Guid tenantId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> UploadCoverAsync(Guid tenantId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    // Notifications
    Task<NotificationSettingsDto> GetNotificationSettingsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(Guid tenantId, UpdateNotificationSettingsDto request, CancellationToken cancellationToken = default);
    Task<ConnectTelegramBotResponseDto> ConnectTelegramBotAsync(Guid tenantId, ConnectTelegramBotRequestDto request, CancellationToken cancellationToken = default);
    Task<ConnectEmailResponseDto> ConnectEmailAsync(Guid tenantId, ConnectEmailRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> SendTestNotificationAsync(Guid tenantId, CancellationToken cancellationToken = default);

    // Categories & Links
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(Guid tenantId, CreateCategoryDto request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateCategoryAsync(Guid tenantId, Guid categoryId, UpdateCategoryDto request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<LinkItemDto> CreateLinkAsync(Guid tenantId, CreateLinkItemDto request, CancellationToken cancellationToken = default);
    Task<LinkItemDto> UpdateLinkAsync(Guid tenantId, Guid linkId, UpdateLinkItemDto request, CancellationToken cancellationToken = default);
    Task<string> UploadLinkFileAsync(Guid tenantId, Guid linkId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteLinkAsync(Guid tenantId, Guid linkId, CancellationToken cancellationToken = default);
    Task ReorderLinksAsync(Guid tenantId, ReorderLinksDto request, CancellationToken cancellationToken = default);

    // Inquiries
    Task<IReadOnlyList<InquiryDto>> GetInquiriesAsync(Guid tenantId, InquiryStatus? status = null, CancellationToken cancellationToken = default);
    Task<InquiryDto> UpdateInquiryStatusAsync(Guid tenantId, Guid inquiryId, InquiryStatus status, CancellationToken cancellationToken = default);

    // Activities
    Task<IReadOnlyList<HotelActivityDto>> GetActivitiesAsync(Guid tenantId, DateTime? date = null, CancellationToken cancellationToken = default);
    Task<HotelActivityDto> CreateActivityAsync(Guid tenantId, CreateHotelActivityDto request, CancellationToken cancellationToken = default);
    Task<HotelActivityDto> UpdateActivityAsync(Guid tenantId, Guid activityId, UpdateHotelActivityDto request, CancellationToken cancellationToken = default);
    Task DeleteActivityAsync(Guid tenantId, Guid activityId, CancellationToken cancellationToken = default);
    Task<bool> ToggleActivityStatusAsync(Guid tenantId, Guid activityId, CancellationToken cancellationToken = default);

    // QR & Analytics
    Task<byte[]> GetPngQrCodeAsync(Guid tenantId, string baseUrl, CancellationToken cancellationToken = default);
    Task<string> GetSvgQrCodeAsync(Guid tenantId, string baseUrl, CancellationToken cancellationToken = default);
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public class HotelAdminService : IHotelAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IQrCodeService _qrCodeService;

    public HotelAdminService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        INotificationDispatcher notificationDispatcher,
        IQrCodeService qrCodeService)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _notificationDispatcher = notificationDispatcher;
        _qrCodeService = qrCodeService;
    }

    public async Task<HotelProfileDto> GetProfileAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        return MapToProfileDto(tenant);
    }

    public async Task<HotelProfileDto> UpdateProfileAsync(Guid tenantId, UpdateHotelProfileDto request, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        tenant.Name = request.Name.Trim();
        tenant.BioAz = request.BioAz;
        tenant.BioEn = request.BioEn;
        tenant.BioRu = request.BioRu;
        tenant.DefaultLanguage = request.DefaultLanguage;
        tenant.PrimaryColor = request.PrimaryColor;
        tenant.ThemeMode = request.ThemeMode;
        tenant.Phone = request.Phone;
        tenant.Email = request.Email;
        tenant.WhatsApp = request.WhatsApp;
        tenant.Telegram = request.Telegram;
        tenant.Address = request.Address;
        tenant.MapUrl = request.MapUrl;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToProfileDto(tenant);
    }

    public async Task<string> UploadLogoAsync(Guid tenantId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        // Delete old logo if exists
        _fileStorage.DeleteFile(tenant.LogoUrl);

        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, "logos", tenantId, cancellationToken);
        tenant.LogoUrl = url;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return url;
    }

    public async Task<string> UploadCoverAsync(Guid tenantId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        _fileStorage.DeleteFile(tenant.CoverImageUrl);

        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, "covers", tenantId, cancellationToken);
        tenant.CoverImageUrl = url;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return url;
    }

    public async Task<NotificationSettingsDto> GetNotificationSettingsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        return new NotificationSettingsDto(
            tenant.PreferredNotificationChannel,
            tenant.NotificationEmail,
            !string.IsNullOrWhiteSpace(tenant.NotificationEmail),
            tenant.TelegramBotToken,
            tenant.TelegramChatId,
            tenant.TelegramBotUsername,
            !string.IsNullOrWhiteSpace(tenant.TelegramBotToken) && !string.IsNullOrWhiteSpace(tenant.TelegramChatId)
        );
    }

    public async Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(Guid tenantId, UpdateNotificationSettingsDto request, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        tenant.PreferredNotificationChannel = request.PreferredNotificationChannel;
        tenant.NotificationEmail = request.NotificationEmail;
        tenant.TelegramBotToken = request.TelegramBotToken;
        tenant.TelegramChatId = request.TelegramChatId;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new NotificationSettingsDto(
            tenant.PreferredNotificationChannel,
            tenant.NotificationEmail,
            !string.IsNullOrWhiteSpace(tenant.NotificationEmail),
            tenant.TelegramBotToken,
            tenant.TelegramChatId,
            tenant.TelegramBotUsername,
            !string.IsNullOrWhiteSpace(tenant.TelegramBotToken) && !string.IsNullOrWhiteSpace(tenant.TelegramChatId)
        );
    }

    public async Task<ConnectEmailResponseDto> ConnectEmailAsync(Guid tenantId, ConnectEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var cleanEmail = request.Email.Trim();
        if (string.IsNullOrWhiteSpace(cleanEmail) || !cleanEmail.Contains('@'))
        {
            throw new BadRequestException("Düzgün e-poçt ünvanı daxil edin.");
        }

        tenant.NotificationEmail = cleanEmail;
        tenant.CustomSmtpHost = request.CustomSmtpHost?.Trim();
        tenant.CustomSmtpPort = request.CustomSmtpPort;
        tenant.CustomSmtpUser = request.CustomSmtpUser?.Trim();
        tenant.CustomSmtpPassword = request.CustomSmtpPassword;

        if (tenant.PreferredNotificationChannel == NotificationChannel.None)
        {
            tenant.PreferredNotificationChannel = NotificationChannel.Email;
        }

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.SendTestEmail)
        {
            await _notificationDispatcher.SendTestNotificationAsync(tenant, NotificationChannel.Email, cancellationToken);
        }

        return new ConnectEmailResponseDto(
            true,
            cleanEmail,
            $"Bildiriş e-poçt ünvanı '{cleanEmail}' olaraq təyin edildi" + (request.SendTestEmail ? " və sınaq e-poçtu göndərildi." : ".")
        );
    }

    public async Task<ConnectTelegramBotResponseDto> ConnectTelegramBotAsync(Guid tenantId, ConnectTelegramBotRequestDto request, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var result = await _notificationDispatcher.ConnectAndVerifyTelegramBotAsync(
            tenant, request.BotToken, request.ChatId, cancellationToken);

        if (!result.Success)
        {
            throw new BadRequestException(result.Message);
        }

        tenant.TelegramBotToken = request.BotToken.Trim();
        tenant.TelegramChatId = result.ChatId;
        tenant.TelegramBotUsername = result.BotUsername;
        tenant.PreferredNotificationChannel = NotificationChannel.Both;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConnectTelegramBotResponseDto(
            true,
            result.BotUsername,
            result.ChatId,
            result.ChatTitle,
            result.Message
        );
    }

    public async Task<bool> SendTestNotificationAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        return await _notificationDispatcher.SendTestNotificationAsync(tenant, tenant.PreferredNotificationChannel, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.LinkCategories.GetCategoriesWithLinksAsync(tenantId, activeOnly: false, cancellationToken);
        return categories.Select(MapToCategoryDto).ToList();
    }

    public async Task<CategoryDto> CreateCategoryAsync(Guid tenantId, CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = new LinkCategory
        {
            TenantId = tenantId,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            LayoutType = request.LayoutType,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.LinkCategories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToCategoryDto(category);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid tenantId, Guid categoryId, UpdateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.LinkCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category == null || category.TenantId != tenantId) throw new NotFoundException("Bölmə tapılmadı.");

        category.TitleAz = request.TitleAz.Trim();
        category.TitleEn = request.TitleEn?.Trim();
        category.TitleRu = request.TitleRu?.Trim();
        category.LayoutType = request.LayoutType;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        _unitOfWork.LinkCategories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToCategoryDto(category);
    }

    public async Task DeleteCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.LinkCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category == null || category.TenantId != tenantId) throw new NotFoundException("Bölmə tapılmadı.");

        _unitOfWork.LinkCategories.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<LinkItemDto> CreateLinkAsync(Guid tenantId, CreateLinkItemDto request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.LinkCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null || category.TenantId != tenantId) throw new NotFoundException("Bölmə tapılmadı.");

        var link = new LinkItem
        {
            TenantId = tenantId,
            CategoryId = request.CategoryId,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            LinkType = request.LinkType,
            TargetUrl = request.TargetUrl.Trim(),
            IconKey = request.IconKey,
            IsCustomIcon = request.IsCustomIcon,
            OpenInNewTab = request.OpenInNewTab,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.LinkItems.AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToLinkDto(link);
    }

    public async Task<LinkItemDto> UpdateLinkAsync(Guid tenantId, Guid linkId, UpdateLinkItemDto request, CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.LinkItems.GetByIdAsync(linkId, cancellationToken);
        if (link == null || link.TenantId != tenantId) throw new NotFoundException("Keçid tapılmadı.");

        link.CategoryId = request.CategoryId;
        link.TitleAz = request.TitleAz.Trim();
        link.TitleEn = request.TitleEn?.Trim();
        link.TitleRu = request.TitleRu?.Trim();
        link.LinkType = request.LinkType;
        link.TargetUrl = request.TargetUrl.Trim();
        link.IconKey = request.IconKey;
        link.IsCustomIcon = request.IsCustomIcon;
        link.OpenInNewTab = request.OpenInNewTab;
        link.DisplayOrder = request.DisplayOrder;
        link.IsActive = request.IsActive;

        _unitOfWork.LinkItems.Update(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToLinkDto(link);
    }

    public async Task<string> UploadLinkFileAsync(Guid tenantId, Guid linkId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.LinkItems.GetByIdAsync(linkId, cancellationToken);
        if (link == null || link.TenantId != tenantId) throw new NotFoundException("Keçid tapılmadı.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var folder = extension == ".pdf" ? "menus" : "icons";

        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, folder, tenantId, cancellationToken);

        if (extension == ".pdf")
        {
            link.TargetUrl = url;
            link.LinkType = LinkType.PdfMenu;
        }
        else
        {
            link.IconKey = url;
            link.IsCustomIcon = true;
        }

        _unitOfWork.LinkItems.Update(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return url;
    }

    public async Task DeleteLinkAsync(Guid tenantId, Guid linkId, CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.LinkItems.GetByIdAsync(linkId, cancellationToken);
        if (link == null || link.TenantId != tenantId) throw new NotFoundException("Keçid tapılmadı.");

        _unitOfWork.LinkItems.Delete(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderLinksAsync(Guid tenantId, ReorderLinksDto request, CancellationToken cancellationToken = default)
    {
        foreach (var item in request.Items)
        {
            var link = await _unitOfWork.LinkItems.GetByIdAsync(item.Id, cancellationToken);
            if (link != null && link.TenantId == tenantId)
            {
                link.DisplayOrder = item.DisplayOrder;
                _unitOfWork.LinkItems.Update(link);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InquiryDto>> GetInquiriesAsync(Guid tenantId, InquiryStatus? status = null, CancellationToken cancellationToken = default)
    {
        var inquiries = await _unitOfWork.GuestInquiries.GetTenantInquiriesAsync(tenantId, status, cancellationToken);
        return inquiries.Select(i => new InquiryDto(
            i.Id,
            i.FullName,
            i.PhoneNumber,
            i.Email,
            i.RoomNumber,
            i.Message,
            i.Status,
            i.IsEmailSent,
            i.IsTelegramSent,
            i.NotificationLog,
            i.CreatedAtUtc
        )).ToList();
    }

    public async Task<InquiryDto> UpdateInquiryStatusAsync(Guid tenantId, Guid inquiryId, InquiryStatus status, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.GuestInquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null || inquiry.TenantId != tenantId) throw new NotFoundException("Müraciət tapılmadı.");

        inquiry.Status = status;
        _unitOfWork.GuestInquiries.Update(inquiry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InquiryDto(
            inquiry.Id,
            inquiry.FullName,
            inquiry.PhoneNumber,
            inquiry.Email,
            inquiry.RoomNumber,
            inquiry.Message,
            inquiry.Status,
            inquiry.IsEmailSent,
            inquiry.IsTelegramSent,
            inquiry.NotificationLog,
            inquiry.CreatedAtUtc
        );
    }

    public async Task<byte[]> GetPngQrCodeAsync(Guid tenantId, string baseUrl, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var fullPortalUrl = $"{baseUrl.TrimEnd('/')}/api/public/{tenant.Slug}";
        var qrSetting = (await _unitOfWork.QrSettings.FindAsync(q => q.TenantId == tenantId, cancellationToken)).FirstOrDefault();

        return _qrCodeService.GeneratePngQrCode(
            fullPortalUrl,
            qrSetting?.PixelsPerModule ?? 20,
            qrSetting?.ForegroundColorHex,
            qrSetting?.BackgroundColorHex
        );
    }

    public async Task<string> GetSvgQrCodeAsync(Guid tenantId, string baseUrl, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var fullPortalUrl = $"{baseUrl.TrimEnd('/')}/api/public/{tenant.Slug}";
        var qrSetting = (await _unitOfWork.QrSettings.FindAsync(q => q.TenantId == tenantId, cancellationToken)).FirstOrDefault();

        return _qrCodeService.GenerateSvgQrCode(
            fullPortalUrl,
            qrSetting?.ForegroundColorHex,
            qrSetting?.BackgroundColorHex
        );
    }

    public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var totalScans = await _unitOfWork.Analytics.GetScanCountAsync(tenantId, cancellationToken: cancellationToken);
        var totalVCardDownloads = await _unitOfWork.Analytics.GetVCardDownloadCountAsync(tenantId, cancellationToken: cancellationToken);
        var totalInquiries = await _unitOfWork.GuestInquiries.CountAsync(i => i.TenantId == tenantId, cancellationToken);

        var links = await _unitOfWork.LinkItems.FindAsync(l => l.TenantId == tenantId, cancellationToken);
        var totalLinkClicks = links.Sum(l => l.ClickCount);

        var topLinks = links
            .OrderByDescending(l => l.ClickCount)
            .Take(10)
            .Select(l => new LinkClickStatDto(l.Id, l.TitleAz, l.ClickCount))
            .ToList();

        var deviceBreakdown = await _unitOfWork.Analytics.GetDeviceBreakdownAsync(tenantId, cancellationToken);

        return new AnalyticsSummaryDto(
            totalScans,
            totalVCardDownloads,
            totalInquiries,
            totalLinkClicks,
            topLinks,
            deviceBreakdown
        );
    }

    #region Activities
    public async Task<IReadOnlyList<HotelActivityDto>> GetActivitiesAsync(Guid tenantId, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var targetDate = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : (DateTime?)null;
        var activities = await _unitOfWork.Activities.FindAsync(
            a => a.TenantId == tenantId && (!targetDate.HasValue || a.Date.Date == targetDate.Value.Date),
            cancellationToken);

        return activities
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.StartTime)
            .Select(a => new HotelActivityDto(
                a.Id,
                a.TitleAz,
                a.TitleEn,
                a.TitleRu,
                a.Description,
                a.Location,
                a.StartTime,
                a.EndTime,
                a.Date,
                a.Category,
                a.ImageUrl,
                a.IsActive,
                a.DisplayOrder
            )).ToList();
    }

    public async Task<HotelActivityDto> CreateActivityAsync(Guid tenantId, CreateHotelActivityDto request, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var actDate = request.Date.HasValue
            ? DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc)
            : DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        var activity = new HotelActivity
        {
            TenantId = tenantId,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            Description = request.Description?.Trim(),
            Location = request.Location.Trim(),
            StartTime = request.StartTime.Trim(),
            EndTime = request.EndTime?.Trim(),
            Date = actDate,
            Category = request.Category?.Trim() ?? "party",
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        await _unitOfWork.Activities.AddAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new HotelActivityDto(
            activity.Id,
            activity.TitleAz,
            activity.TitleEn,
            activity.TitleRu,
            activity.Description,
            activity.Location,
            activity.StartTime,
            activity.EndTime,
            activity.Date,
            activity.Category,
            activity.ImageUrl,
            activity.IsActive,
            activity.DisplayOrder
        );
    }

    public async Task<HotelActivityDto> UpdateActivityAsync(Guid tenantId, Guid activityId, UpdateHotelActivityDto request, CancellationToken cancellationToken = default)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(activityId, cancellationToken);
        if (activity == null || activity.TenantId != tenantId) throw new NotFoundException("Fəaliyyət tapılmadı.");

        activity.TitleAz = request.TitleAz.Trim();
        activity.TitleEn = request.TitleEn?.Trim();
        activity.TitleRu = request.TitleRu?.Trim();
        activity.Description = request.Description?.Trim();
        activity.Location = request.Location.Trim();
        activity.StartTime = request.StartTime.Trim();
        activity.EndTime = request.EndTime?.Trim();
        if (request.Date.HasValue) activity.Date = DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc);
        activity.Category = request.Category?.Trim();
        activity.ImageUrl = request.ImageUrl;
        activity.DisplayOrder = request.DisplayOrder;
        activity.IsActive = request.IsActive;

        _unitOfWork.Activities.Update(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new HotelActivityDto(
            activity.Id,
            activity.TitleAz,
            activity.TitleEn,
            activity.TitleRu,
            activity.Description,
            activity.Location,
            activity.StartTime,
            activity.EndTime,
            activity.Date,
            activity.Category,
            activity.ImageUrl,
            activity.IsActive,
            activity.DisplayOrder
        );
    }

    public async Task DeleteActivityAsync(Guid tenantId, Guid activityId, CancellationToken cancellationToken = default)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(activityId, cancellationToken);
        if (activity == null || activity.TenantId != tenantId) throw new NotFoundException("Fəaliyyət tapılmadı.");

        _unitOfWork.Activities.Delete(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ToggleActivityStatusAsync(Guid tenantId, Guid activityId, CancellationToken cancellationToken = default)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(activityId, cancellationToken);
        if (activity == null || activity.TenantId != tenantId) throw new NotFoundException("Fəaliyyət tapılmadı.");

        activity.IsActive = !activity.IsActive;
        _unitOfWork.Activities.Update(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return activity.IsActive;
    }
    #endregion

    private static HotelProfileDto MapToProfileDto(Tenant t) =>
        new(t.Id, t.Name, t.Slug, t.BioAz, t.BioEn, t.BioRu, t.DefaultLanguage, t.LogoUrl, t.CoverImageUrl,
            t.PrimaryColor, t.ThemeMode, t.Phone, t.Email, t.WhatsApp, t.Telegram, t.Address, t.MapUrl,
            t.IsActive, t.SubscriptionStatus, t.SubscriptionEndDate);

    private static CategoryDto MapToCategoryDto(LinkCategory c) =>
        new(c.Id, c.TitleAz, c.TitleEn, c.TitleRu, c.LayoutType, c.DisplayOrder, c.IsActive,
            c.Links.OrderBy(l => l.DisplayOrder).Select(MapToLinkDto).ToList());

    private static LinkItemDto MapToLinkDto(LinkItem l) =>
        new(l.Id, l.CategoryId, l.TitleAz, l.TitleEn, l.TitleRu, l.LinkType, l.TargetUrl, l.IconKey,
            l.IsCustomIcon, l.OpenInNewTab, l.ClickCount, l.DisplayOrder, l.IsActive);
}

