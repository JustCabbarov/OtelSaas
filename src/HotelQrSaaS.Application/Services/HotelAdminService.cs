using Microsoft.Extensions.Configuration;
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

    // Categories & Links (Existing PDF/Link system preserved)
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

    // Stories (Top Highlights Bar)
    Task<IReadOnlyList<HotelStoryDto>> GetStoriesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<HotelStoryDto> CreateStoryAsync(Guid tenantId, CreateHotelStoryDto request, CancellationToken cancellationToken = default);
    Task<HotelStoryDto> UpdateStoryAsync(Guid tenantId, Guid storyId, UpdateHotelStoryDto request, CancellationToken cancellationToken = default);
    Task<string> UploadStoryImageAsync(Guid tenantId, Guid storyId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<string> UploadStoryVideoAsync(Guid tenantId, Guid storyId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteStoryAsync(Guid tenantId, Guid storyId, CancellationToken cancellationToken = default);

    // Hubs (6 Core Hotel Tiles)
    Task<IReadOnlyList<HotelHubDto>> GetHubsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<HotelHubDto> CreateHubAsync(Guid tenantId, CreateHotelHubDto request, CancellationToken cancellationToken = default);
    Task<HotelHubDto> UpdateHubAsync(Guid tenantId, Guid hubId, UpdateHotelHubDto request, CancellationToken cancellationToken = default);
    Task<string> UploadHubCoverAsync(Guid tenantId, Guid hubId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteHubAsync(Guid tenantId, Guid hubId, CancellationToken cancellationToken = default);

    // Catalog Categories (Outlets & Sections: SHIRVAN, SAQI Lounge, MARAL Spa, Rooms, etc.)
    Task<IReadOnlyList<CatalogCategoryDto>> GetCatalogCategoriesAsync(Guid tenantId, Guid? hubId = null, CancellationToken cancellationToken = default);
    Task<CatalogCategoryDto> CreateCatalogCategoryAsync(Guid tenantId, CreateCatalogCategoryDto request, CancellationToken cancellationToken = default);
    Task<CatalogCategoryDto> UpdateCatalogCategoryAsync(Guid tenantId, Guid categoryId, UpdateCatalogCategoryDto request, CancellationToken cancellationToken = default);
    Task<string> UploadCatalogCategoryImageAsync(Guid tenantId, Guid categoryId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteCatalogCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default);

    // Catalog Items (Priced Dishes, Spa Treatments, Room Types, Info Cards, Daily Specials)
    Task<IReadOnlyList<CatalogItemDto>> GetCatalogItemsAsync(Guid tenantId, Guid? hubId = null, Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<CatalogItemDto> CreateCatalogItemAsync(Guid tenantId, CreateCatalogItemDto request, CancellationToken cancellationToken = default);
    Task<CatalogItemDto> UpdateCatalogItemAsync(Guid tenantId, Guid itemId, UpdateCatalogItemDto request, CancellationToken cancellationToken = default);
    Task<string> UploadCatalogItemImageAsync(Guid tenantId, Guid itemId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteCatalogItemAsync(Guid tenantId, Guid itemId, CancellationToken cancellationToken = default);

    // Guest Cart Orders (Room Service / Table / Spa)
    Task<IReadOnlyList<GuestOrderDto>> GetOrdersAsync(Guid tenantId, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<GuestOrderDto> UpdateOrderStatusAsync(Guid tenantId, Guid orderId, OrderStatus status, CancellationToken cancellationToken = default);

    // Guest Reviews (/review 5-Star Feedback)
    Task<IReadOnlyList<GuestReviewDto>> GetReviewsAsync(Guid tenantId, bool? isResolved = null, CancellationToken cancellationToken = default);
    Task<bool> ToggleReviewResolvedAsync(Guid tenantId, Guid reviewId, CancellationToken cancellationToken = default);

    // Department Contacts (Reception, Room Service ext 3, Spa, etc.)
    Task<IReadOnlyList<DepartmentContactDto>> GetDepartmentContactsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<DepartmentContactDto> CreateDepartmentContactAsync(Guid tenantId, CreateDepartmentContactDto request, CancellationToken cancellationToken = default);
    Task<DepartmentContactDto> UpdateDepartmentContactAsync(Guid tenantId, Guid contactId, UpdateDepartmentContactDto request, CancellationToken cancellationToken = default);
    Task DeleteDepartmentContactAsync(Guid tenantId, Guid contactId, CancellationToken cancellationToken = default);

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
    private readonly IConfiguration _configuration;

    public HotelAdminService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        INotificationDispatcher notificationDispatcher,
        IQrCodeService qrCodeService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _notificationDispatcher = notificationDispatcher;
        _qrCodeService = qrCodeService;
        _configuration = configuration;
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
        tenant.SubtitleAz = request.SubtitleAz?.Trim();
        tenant.SubtitleEn = request.SubtitleEn?.Trim();
        tenant.SubtitleRu = request.SubtitleRu?.Trim();
        tenant.TaglineAz = request.TaglineAz?.Trim();
        tenant.TaglineEn = request.TaglineEn?.Trim();
        tenant.TaglineRu = request.TaglineRu?.Trim();
        tenant.StarRating = request.StarRating;
        tenant.BioAz = request.BioAz;
        tenant.BioEn = request.BioEn;
        tenant.BioRu = request.BioRu;
        tenant.DefaultLanguage = request.DefaultLanguage;
        tenant.CurrencyCode = request.CurrencyCode;
        tenant.ServiceFeePercent = request.ServiceFeePercent;
        tenant.PrimaryColor = request.PrimaryColor;
        tenant.ThemeMode = request.ThemeMode;
        tenant.ThemePreset = request.ThemePreset;
        tenant.ThemeStyle = request.ThemeStyle;
        tenant.Phone = request.Phone;
        tenant.Email = request.Email;
        tenant.WhatsApp = request.WhatsApp;
        tenant.Telegram = request.Telegram;
        tenant.Address = request.Address;
        tenant.MapUrl = request.MapUrl;
        tenant.InstagramUrl = request.InstagramUrl;
        tenant.FacebookUrl = request.FacebookUrl;
        tenant.YoutubeUrl = request.YoutubeUrl;
        tenant.TikTokUrl = request.TikTokUrl;
        tenant.TripAdvisorUrl = request.TripAdvisorUrl;
        tenant.BookingUrl = request.BookingUrl;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToProfileDto(tenant);
    }

    public async Task<string> UploadLogoAsync(Guid tenantId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

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

    #region Categories & Links (Existing)
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
    #endregion

    #region Inquiries
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
    #endregion

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

    #region Stories (Top Highlights Bar)
    public async Task<IReadOnlyList<HotelStoryDto>> GetStoriesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var stories = await _unitOfWork.Stories.FindAsync(s => s.TenantId == tenantId, cancellationToken);
        return stories.OrderBy(s => s.DisplayOrder).Select(MapToStoryDto).ToList();
    }

    public async Task<HotelStoryDto> CreateStoryAsync(Guid tenantId, CreateHotelStoryDto request, CancellationToken cancellationToken = default)
    {
        var story = new HotelStory
        {
            TenantId = tenantId,
            ExternalId = request.ExternalId,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            DescriptionAz = request.DescriptionAz?.Trim(),
            DescriptionEn = request.DescriptionEn?.Trim(),
            DescriptionRu = request.DescriptionRu?.Trim(),
            ButtonTextAz = request.ButtonTextAz?.Trim(),
            ButtonTextEn = request.ButtonTextEn?.Trim(),
            ButtonTextRu = request.ButtonTextRu?.Trim(),
            ImageUrl = request.ImageUrl,
            VideoUrl = request.VideoUrl,
            BannerImageUrl = request.BannerImageUrl,
            BadgeText = request.BadgeText?.Trim(),
            ActionUrl = request.ActionUrl?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.Stories.AddAsync(story, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToStoryDto(story);
    }

    public async Task<HotelStoryDto> UpdateStoryAsync(Guid tenantId, Guid storyId, UpdateHotelStoryDto request, CancellationToken cancellationToken = default)
    {
        var story = await _unitOfWork.Stories.GetByIdAsync(storyId, cancellationToken);
        if (story == null || story.TenantId != tenantId) throw new NotFoundException("Story tapılmadı.");

        story.ExternalId = request.ExternalId;
        story.TitleAz = request.TitleAz.Trim();
        story.TitleEn = request.TitleEn?.Trim();
        story.TitleRu = request.TitleRu?.Trim();
        story.DescriptionAz = request.DescriptionAz?.Trim();
        story.DescriptionEn = request.DescriptionEn?.Trim();
        story.DescriptionRu = request.DescriptionRu?.Trim();
        story.ButtonTextAz = request.ButtonTextAz?.Trim();
        story.ButtonTextEn = request.ButtonTextEn?.Trim();
        story.ButtonTextRu = request.ButtonTextRu?.Trim();
        story.ImageUrl = request.ImageUrl;
        story.VideoUrl = request.VideoUrl;
        story.BannerImageUrl = request.BannerImageUrl;
        story.BadgeText = request.BadgeText?.Trim();
        story.ActionUrl = request.ActionUrl?.Trim();
        story.DisplayOrder = request.DisplayOrder;
        story.IsActive = request.IsActive;

        _unitOfWork.Stories.Update(story);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToStoryDto(story);
    }

    public async Task<string> UploadStoryImageAsync(Guid tenantId, Guid storyId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var story = await _unitOfWork.Stories.GetByIdAsync(storyId, cancellationToken);
        if (story == null || story.TenantId != tenantId) throw new NotFoundException("Story tapılmadı.");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is ".mp4" or ".webm" or ".mov" or ".m4v")
        {
            return await UploadStoryVideoAsync(tenantId, storyId, fileStream, fileName, cancellationToken);
        }

        _fileStorage.DeleteFile(story.ImageUrl);
        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, "stories", tenantId, cancellationToken);
        story.ImageUrl = url;
        story.BannerImageUrl ??= url;

        _unitOfWork.Stories.Update(story);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    public async Task<string> UploadStoryVideoAsync(Guid tenantId, Guid storyId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var story = await _unitOfWork.Stories.GetByIdAsync(storyId, cancellationToken);
        if (story == null || story.TenantId != tenantId) throw new NotFoundException("Story tapılmadı.");

        _fileStorage.DeleteFile(story.VideoUrl);
        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, "stories", tenantId, cancellationToken);
        story.VideoUrl = url;

        _unitOfWork.Stories.Update(story);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    public async Task DeleteStoryAsync(Guid tenantId, Guid storyId, CancellationToken cancellationToken = default)
    {
        var story = await _unitOfWork.Stories.GetByIdAsync(storyId, cancellationToken);
        if (story == null || story.TenantId != tenantId) throw new NotFoundException("Story tapılmadı.");

        _unitOfWork.Stories.Delete(story);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #endregion

    #region Hubs (6 Core Hotel Tiles)
    public async Task<IReadOnlyList<HotelHubDto>> GetHubsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var hubs = await _unitOfWork.Hubs.FindAsync(h => h.TenantId == tenantId, cancellationToken);
        return hubs.OrderBy(h => h.DisplayOrder).Select(MapToHubDto).ToList();
    }

    public async Task<HotelHubDto> CreateHubAsync(Guid tenantId, CreateHotelHubDto request, CancellationToken cancellationToken = default)
    {
        var hub = new HotelHub
        {
            TenantId = tenantId,
            ExternalId = request.ExternalId,
            Code = request.Code.Trim().ToLowerInvariant(),
            HubType = request.HubType,
            DisplayMode = request.DisplayMode,
            KickerAz = request.KickerAz?.Trim(),
            KickerEn = request.KickerEn?.Trim(),
            KickerRu = request.KickerRu?.Trim(),
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            SubtitleAz = request.SubtitleAz?.Trim(),
            SubtitleEn = request.SubtitleEn?.Trim(),
            SubtitleRu = request.SubtitleRu?.Trim(),
            IconKey = request.IconKey?.Trim(),
            CoverImageUrl = request.CoverImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.Hubs.AddAsync(hub, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToHubDto(hub);
    }

    public async Task<HotelHubDto> UpdateHubAsync(Guid tenantId, Guid hubId, UpdateHotelHubDto request, CancellationToken cancellationToken = default)
    {
        var hub = await _unitOfWork.Hubs.GetByIdAsync(hubId, cancellationToken);
        if (hub == null || hub.TenantId != tenantId) throw new NotFoundException("Hub tapılmadı.");

        hub.ExternalId = request.ExternalId;
        hub.Code = request.Code.Trim().ToLowerInvariant();
        hub.HubType = request.HubType;
        hub.DisplayMode = request.DisplayMode;
        hub.KickerAz = request.KickerAz?.Trim();
        hub.KickerEn = request.KickerEn?.Trim();
        hub.KickerRu = request.KickerRu?.Trim();
        hub.TitleAz = request.TitleAz.Trim();
        hub.TitleEn = request.TitleEn?.Trim();
        hub.TitleRu = request.TitleRu?.Trim();
        hub.SubtitleAz = request.SubtitleAz?.Trim();
        hub.SubtitleEn = request.SubtitleEn?.Trim();
        hub.SubtitleRu = request.SubtitleRu?.Trim();
        hub.IconKey = request.IconKey?.Trim();
        hub.CoverImageUrl = request.CoverImageUrl;
        hub.DisplayOrder = request.DisplayOrder;
        hub.IsActive = request.IsActive;

        _unitOfWork.Hubs.Update(hub);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToHubDto(hub);
    }

    public async Task<string> UploadHubCoverAsync(Guid tenantId, Guid hubId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var hub = await _unitOfWork.Hubs.GetByIdAsync(hubId, cancellationToken);
        if (hub == null || hub.TenantId != tenantId) throw new NotFoundException("Hub tapılmadı.");

        _fileStorage.DeleteFile(hub.CoverImageUrl);
        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, "hubs", tenantId, cancellationToken);
        hub.CoverImageUrl = url;

        _unitOfWork.Hubs.Update(hub);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    public async Task DeleteHubAsync(Guid tenantId, Guid hubId, CancellationToken cancellationToken = default)
    {
        var hub = await _unitOfWork.Hubs.GetByIdAsync(hubId, cancellationToken);
        if (hub == null || hub.TenantId != tenantId) throw new NotFoundException("Hub tapılmadı.");

        _unitOfWork.Hubs.Delete(hub);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #endregion

    #region Catalog Categories (Outlets & Sections)
    public async Task<IReadOnlyList<CatalogCategoryDto>> GetCatalogCategoriesAsync(Guid tenantId, Guid? hubId = null, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.CatalogCategories.FindAsync(
            c => c.TenantId == tenantId && (!hubId.HasValue || c.HubId == hubId.Value),
            cancellationToken);

        return categories.OrderBy(c => c.DisplayOrder).Select(MapToCatalogCategoryDto).ToList();
    }

    public async Task<CatalogCategoryDto> CreateCatalogCategoryAsync(Guid tenantId, CreateCatalogCategoryDto request, CancellationToken cancellationToken = default)
    {
        var hub = await _unitOfWork.Hubs.GetByIdAsync(request.HubId, cancellationToken);
        if (hub == null || hub.TenantId != tenantId) throw new NotFoundException("Hub tapılmadı.");

        var category = new CatalogCategory
        {
            TenantId = tenantId,
            ExternalId = request.ExternalId,
            HubId = request.HubId,
            ParentCategoryId = request.ParentCategoryId,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            SubtitleAz = request.SubtitleAz?.Trim(),
            SubtitleEn = request.SubtitleEn?.Trim(),
            SubtitleRu = request.SubtitleRu?.Trim(),
            ImageUrl = request.ImageUrl,
            IconKey = request.IconKey?.Trim(),
            WorkingHours = request.WorkingHours?.Trim(),
            InternalExtension = request.InternalExtension?.Trim(),
            LayoutStyle = request.LayoutStyle,
            CollapseIfSingleItem = request.CollapseIfSingleItem,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.CatalogCategories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToCatalogCategoryDto(category);
    }

    public async Task<CatalogCategoryDto> UpdateCatalogCategoryAsync(Guid tenantId, Guid categoryId, UpdateCatalogCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.CatalogCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category == null || category.TenantId != tenantId) throw new NotFoundException("Kateqoriya tapılmadı.");

        category.ExternalId = request.ExternalId;
        category.HubId = request.HubId;
        category.ParentCategoryId = request.ParentCategoryId;
        category.TitleAz = request.TitleAz.Trim();
        category.TitleEn = request.TitleEn?.Trim();
        category.TitleRu = request.TitleRu?.Trim();
        category.SubtitleAz = request.SubtitleAz?.Trim();
        category.SubtitleEn = request.SubtitleEn?.Trim();
        category.SubtitleRu = request.SubtitleRu?.Trim();
        category.ImageUrl = request.ImageUrl;
        category.IconKey = request.IconKey?.Trim();
        category.WorkingHours = request.WorkingHours?.Trim();
        category.InternalExtension = request.InternalExtension?.Trim();
        category.LayoutStyle = request.LayoutStyle;
        category.CollapseIfSingleItem = request.CollapseIfSingleItem;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        _unitOfWork.CatalogCategories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToCatalogCategoryDto(category);
    }

    public async Task<string> UploadCatalogCategoryImageAsync(Guid tenantId, Guid categoryId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.CatalogCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category == null || category.TenantId != tenantId) throw new NotFoundException("Kateqoriya tapılmadı.");

        _fileStorage.DeleteFile(category.ImageUrl);
        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, "catalog-categories", tenantId, cancellationToken);
        category.ImageUrl = url;

        _unitOfWork.CatalogCategories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    public async Task DeleteCatalogCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.CatalogCategories.GetByIdAsync(categoryId, cancellationToken);
        if (category == null || category.TenantId != tenantId) throw new NotFoundException("Kateqoriya tapılmadı.");

        _unitOfWork.CatalogCategories.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #endregion

    #region Catalog Items (Menu Products, Spa Services, Rooms, Guide Cards)
    public async Task<IReadOnlyList<CatalogItemDto>> GetCatalogItemsAsync(
        Guid tenantId,
        Guid? hubId = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.CatalogItems.FindAsync(
            i => i.TenantId == tenantId &&
                 (!hubId.HasValue || i.HubId == hubId.Value) &&
                 (!categoryId.HasValue || i.CategoryId == categoryId.Value),
            cancellationToken);

        return items.OrderBy(i => i.DisplayOrder).Select(MapToCatalogItemDto).ToList();
    }

    public async Task<CatalogItemDto> CreateCatalogItemAsync(Guid tenantId, CreateCatalogItemDto request, CancellationToken cancellationToken = default)
    {
        var hub = await _unitOfWork.Hubs.GetByIdAsync(request.HubId, cancellationToken);
        if (hub == null || hub.TenantId != tenantId) throw new NotFoundException("Hub tapılmadı.");

        var item = new CatalogItem
        {
            TenantId = tenantId,
            ExternalId = request.ExternalId,
            HubId = request.HubId,
            CategoryId = request.CategoryId,
            ItemType = request.ItemType,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            SubtitleAz = request.SubtitleAz?.Trim(),
            SubtitleEn = request.SubtitleEn?.Trim(),
            SubtitleRu = request.SubtitleRu?.Trim(),
            DescriptionAz = request.DescriptionAz?.Trim(),
            DescriptionEn = request.DescriptionEn?.Trim(),
            DescriptionRu = request.DescriptionRu?.Trim(),
            BadgeAz = request.BadgeAz?.Trim(),
            BadgeEn = request.BadgeEn?.Trim(),
            BadgeRu = request.BadgeRu?.Trim(),
            IconKey = request.IconKey?.Trim(),
            FilterTag = request.FilterTag?.Trim(),
            Price = request.Price,
            OldPrice = request.OldPrice,
            CostPrice = request.CostPrice,
            DiscountPercent = request.DiscountPercent,
            NoServiceFee = request.NoServiceFee,
            PrepTimeMinutes = request.PrepTimeMinutes,
            Currency = request.Currency,
            PortionOrDuration = request.PortionOrDuration?.Trim(),
            BulletPointsAz = request.BulletPointsAz,
            BulletPointsEn = request.BulletPointsEn,
            BulletPointsRu = request.BulletPointsRu,
            ImageUrl = request.ImageUrl,
            IsOrderable = request.IsOrderable,
            IsFeaturedInDailySpecials = request.IsFeaturedInDailySpecials,
            ActionType = request.ActionType?.Trim(),
            ActionValue = request.ActionValue?.Trim(),
            ActionLabelAz = request.ActionLabelAz?.Trim(),
            ActionLabelEn = request.ActionLabelEn?.Trim(),
            ActionLabelRu = request.ActionLabelRu?.Trim(),
            ActionUrl = request.ActionUrl?.Trim(),
            ModifierGroupsJson = request.ModifierGroupsJson,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.CatalogItems.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToCatalogItemDto(item);
    }

    public async Task<CatalogItemDto> UpdateCatalogItemAsync(Guid tenantId, Guid itemId, UpdateCatalogItemDto request, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.CatalogItems.GetByIdAsync(itemId, cancellationToken);
        if (item == null || item.TenantId != tenantId) throw new NotFoundException("Məhsul tapılmadı.");

        item.ExternalId = request.ExternalId;
        item.HubId = request.HubId;
        item.CategoryId = request.CategoryId;
        item.ItemType = request.ItemType;
        item.TitleAz = request.TitleAz.Trim();
        item.TitleEn = request.TitleEn?.Trim();
        item.TitleRu = request.TitleRu?.Trim();
        item.SubtitleAz = request.SubtitleAz?.Trim();
        item.SubtitleEn = request.SubtitleEn?.Trim();
        item.SubtitleRu = request.SubtitleRu?.Trim();
        item.DescriptionAz = request.DescriptionAz?.Trim();
        item.DescriptionEn = request.DescriptionEn?.Trim();
        item.DescriptionRu = request.DescriptionRu?.Trim();
        item.BadgeAz = request.BadgeAz?.Trim();
        item.BadgeEn = request.BadgeEn?.Trim();
        item.BadgeRu = request.BadgeRu?.Trim();
        item.IconKey = request.IconKey?.Trim();
        item.FilterTag = request.FilterTag?.Trim();
        item.Price = request.Price;
        item.OldPrice = request.OldPrice;
        item.CostPrice = request.CostPrice;
        item.DiscountPercent = request.DiscountPercent;
        item.NoServiceFee = request.NoServiceFee;
        item.PrepTimeMinutes = request.PrepTimeMinutes;
        item.Currency = request.Currency;
        item.PortionOrDuration = request.PortionOrDuration?.Trim();
        item.BulletPointsAz = request.BulletPointsAz;
        item.BulletPointsEn = request.BulletPointsEn;
        item.BulletPointsRu = request.BulletPointsRu;
        item.ImageUrl = request.ImageUrl;
        item.IsOrderable = request.IsOrderable;
        item.IsFeaturedInDailySpecials = request.IsFeaturedInDailySpecials;
        item.ActionType = request.ActionType?.Trim();
        item.ActionValue = request.ActionValue?.Trim();
        item.ActionLabelAz = request.ActionLabelAz?.Trim();
        item.ActionLabelEn = request.ActionLabelEn?.Trim();
        item.ActionLabelRu = request.ActionLabelRu?.Trim();
        item.ActionUrl = request.ActionUrl?.Trim();
        item.ModifierGroupsJson = request.ModifierGroupsJson;
        item.DisplayOrder = request.DisplayOrder;
        item.IsActive = request.IsActive;

        _unitOfWork.CatalogItems.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToCatalogItemDto(item);
    }

    public async Task<string> UploadCatalogItemImageAsync(Guid tenantId, Guid itemId, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.CatalogItems.GetByIdAsync(itemId, cancellationToken);
        if (item == null || item.TenantId != tenantId) throw new NotFoundException("Məhsul tapılmadı.");

        _fileStorage.DeleteFile(item.ImageUrl);
        var url = await _fileStorage.SaveFileAsync(fileStream, fileName, "catalog-items", tenantId, cancellationToken);
        item.ImageUrl = url;

        _unitOfWork.CatalogItems.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return url;
    }

    public async Task DeleteCatalogItemAsync(Guid tenantId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.CatalogItems.GetByIdAsync(itemId, cancellationToken);
        if (item == null || item.TenantId != tenantId) throw new NotFoundException("Məhsul tapılmadı.");

        _unitOfWork.CatalogItems.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #endregion

    #region Guest Orders (Cart & Room Service)
    public async Task<IReadOnlyList<GuestOrderDto>> GetOrdersAsync(Guid tenantId, OrderStatus? status = null, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.GuestOrders.GetTenantOrdersAsync(tenantId, status, cancellationToken);
        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<GuestOrderDto> UpdateOrderStatusAsync(Guid tenantId, Guid orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.GuestOrders.GetOrderWithItemsAsync(orderId, cancellationToken);
        if (order == null || order.TenantId != tenantId) throw new NotFoundException("Sifariş tapılmadı.");

        order.Status = status;
        _unitOfWork.GuestOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToOrderDto(order);
    }
    #endregion

    #region Guest Reviews
    public async Task<IReadOnlyList<GuestReviewDto>> GetReviewsAsync(Guid tenantId, bool? isResolved = null, CancellationToken cancellationToken = default)
    {
        var reviews = await _unitOfWork.GuestReviews.FindAsync(
            r => r.TenantId == tenantId && (!isResolved.HasValue || r.IsResolved == isResolved.Value),
            cancellationToken);

        return reviews
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new GuestReviewDto(
                r.Id, r.Rating, r.GuestName, r.RoomNumber, r.PhoneNumber,
                r.Department, r.Comment, r.IsResolved, r.IsTelegramAlertSent, r.CreatedAtUtc
            )).ToList();
    }

    public async Task<bool> ToggleReviewResolvedAsync(Guid tenantId, Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.GuestReviews.GetByIdAsync(reviewId, cancellationToken);
        if (review == null || review.TenantId != tenantId) throw new NotFoundException("Rəy tapılmadı.");

        review.IsResolved = !review.IsResolved;
        _unitOfWork.GuestReviews.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return review.IsResolved;
    }
    #endregion

    #region Department Contacts
    public async Task<IReadOnlyList<DepartmentContactDto>> GetDepartmentContactsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var contacts = await _unitOfWork.DepartmentContacts.FindAsync(d => d.TenantId == tenantId, cancellationToken);
        return contacts.OrderBy(d => d.DisplayOrder).Select(MapToDepartmentDto).ToList();
    }

    public async Task<DepartmentContactDto> CreateDepartmentContactAsync(Guid tenantId, CreateDepartmentContactDto request, CancellationToken cancellationToken = default)
    {
        var dept = new HotelDepartmentContact
        {
            TenantId = tenantId,
            TitleAz = request.TitleAz.Trim(),
            TitleEn = request.TitleEn?.Trim(),
            TitleRu = request.TitleRu?.Trim(),
            SubtitleAz = request.SubtitleAz?.Trim(),
            SubtitleEn = request.SubtitleEn?.Trim(),
            SubtitleRu = request.SubtitleRu?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            InternalExtension = request.InternalExtension?.Trim(),
            WhatsAppNumber = request.WhatsAppNumber?.Trim(),
            Email = request.Email?.Trim(),
            WorkingHours = request.WorkingHours?.Trim(),
            IconKey = request.IconKey?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.DepartmentContacts.AddAsync(dept, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDepartmentDto(dept);
    }

    public async Task<DepartmentContactDto> UpdateDepartmentContactAsync(Guid tenantId, Guid contactId, UpdateDepartmentContactDto request, CancellationToken cancellationToken = default)
    {
        var dept = await _unitOfWork.DepartmentContacts.GetByIdAsync(contactId, cancellationToken);
        if (dept == null || dept.TenantId != tenantId) throw new NotFoundException("Şöbə əlaqəsi tapılmadı.");

        dept.TitleAz = request.TitleAz.Trim();
        dept.TitleEn = request.TitleEn?.Trim();
        dept.TitleRu = request.TitleRu?.Trim();
        dept.SubtitleAz = request.SubtitleAz?.Trim();
        dept.SubtitleEn = request.SubtitleEn?.Trim();
        dept.SubtitleRu = request.SubtitleRu?.Trim();
        dept.PhoneNumber = request.PhoneNumber?.Trim();
        dept.InternalExtension = request.InternalExtension?.Trim();
        dept.WhatsAppNumber = request.WhatsAppNumber?.Trim();
        dept.Email = request.Email?.Trim();
        dept.WorkingHours = request.WorkingHours?.Trim();
        dept.IconKey = request.IconKey?.Trim();
        dept.DisplayOrder = request.DisplayOrder;
        dept.IsActive = request.IsActive;

        _unitOfWork.DepartmentContacts.Update(dept);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDepartmentDto(dept);
    }

    public async Task DeleteDepartmentContactAsync(Guid tenantId, Guid contactId, CancellationToken cancellationToken = default)
    {
        var dept = await _unitOfWork.DepartmentContacts.GetByIdAsync(contactId, cancellationToken);
        if (dept == null || dept.TenantId != tenantId) throw new NotFoundException("Şöbə əlaqəsi tapılmadı.");

        _unitOfWork.DepartmentContacts.Delete(dept);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    #endregion

    #region QR & Analytics
    public async Task<byte[]> GetPngQrCodeAsync(Guid tenantId, string baseUrl, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null) throw new NotFoundException("Otel tapılmadı.");

        var fullPortalUrl = ResolveQrUrl(tenant, baseUrl);
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

        var fullPortalUrl = ResolveQrUrl(tenant, baseUrl);
        var qrSetting = (await _unitOfWork.QrSettings.FindAsync(q => q.TenantId == tenantId, cancellationToken)).FirstOrDefault();

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
        var apiBaseUrl = _configuration["FrontendSettings:ApiBaseUrl"];

        if (directLink && !string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            return $"{frontendBaseUrl.TrimEnd('/')}/hotel/{tenant.Slug}";
        }

        var effectiveBaseUrl = !string.IsNullOrWhiteSpace(apiBaseUrl) ? apiBaseUrl.TrimEnd('/') : baseUrl.TrimEnd('/');
        return $"{effectiveBaseUrl}/q/{tenant.Slug}";
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

        var orders = await _unitOfWork.GuestOrders.FindAsync(o => o.TenantId == tenantId && o.Status != OrderStatus.Cancelled, cancellationToken);
        var totalOrders = orders.Count;
        var totalOrderRevenue = orders.Sum(o => o.TotalAmount);

        var reviews = await _unitOfWork.GuestReviews.FindAsync(r => r.TenantId == tenantId, cancellationToken);
        var totalReviews = reviews.Count;
        var avgRating = totalReviews > 0 ? Math.Round(reviews.Average(r => r.Rating), 2) : 5.0;

        return new AnalyticsSummaryDto(
            totalScans,
            totalVCardDownloads,
            totalInquiries,
            totalLinkClicks,
            topLinks,
            deviceBreakdown,
            totalOrders,
            totalOrderRevenue,
            totalReviews,
            avgRating
        );
    }
    #endregion

    private static HotelProfileDto MapToProfileDto(Tenant t) =>
        new(t.Id, t.Name, t.Slug, t.BioAz, t.BioEn, t.BioRu, t.DefaultLanguage, t.LogoUrl, t.CoverImageUrl,
            t.PrimaryColor, t.ThemeMode, t.Phone, t.Email, t.WhatsApp, t.Telegram, t.Address, t.MapUrl,
            t.IsActive, t.SubscriptionStatus, t.SubscriptionEndDate, t.SubtitleAz, t.SubtitleEn, t.SubtitleRu, t.StarRating,
            t.TaglineAz, t.TaglineEn, t.TaglineRu, t.CurrencyCode, t.ServiceFeePercent, t.ThemePreset, t.ThemeStyle,
            t.InstagramUrl, t.FacebookUrl, t.YoutubeUrl, t.TikTokUrl, t.TripAdvisorUrl, t.BookingUrl);

    private static CategoryDto MapToCategoryDto(LinkCategory c) =>
        new(c.Id, c.TitleAz, c.TitleEn, c.TitleRu, c.LayoutType, c.DisplayOrder, c.IsActive,
            c.Links.OrderBy(l => l.DisplayOrder).Select(MapToLinkDto).ToList());

    private static LinkItemDto MapToLinkDto(LinkItem l) =>
        new(l.Id, l.CategoryId, l.TitleAz, l.TitleEn, l.TitleRu, l.LinkType, l.TargetUrl, l.IconKey,
            l.IsCustomIcon, l.OpenInNewTab, l.ClickCount, l.DisplayOrder, l.IsActive);

    private static HotelStoryDto MapToStoryDto(HotelStory s) =>
        new(s.Id, s.TitleAz, s.TitleEn, s.TitleRu, s.DescriptionAz, s.DescriptionEn, s.DescriptionRu,
            s.ImageUrl, s.BannerImageUrl, s.BadgeText, s.ActionUrl, s.DisplayOrder, s.IsActive,
            s.ExternalId, s.VideoUrl, s.ButtonTextAz, s.ButtonTextEn, s.ButtonTextRu);

    private static HotelHubDto MapToHubDto(HotelHub h) =>
        new(h.Id, h.Code, h.HubType, h.KickerAz, h.KickerEn, h.KickerRu, h.TitleAz, h.TitleEn, h.TitleRu,
            h.SubtitleAz, h.SubtitleEn, h.SubtitleRu, h.IconKey, h.CoverImageUrl, h.DisplayOrder, h.IsActive,
            h.ExternalId, h.DisplayMode);

    private static CatalogCategoryDto MapToCatalogCategoryDto(CatalogCategory c) =>
        new(c.Id, c.HubId, c.ParentCategoryId, c.TitleAz, c.TitleEn, c.TitleRu, c.SubtitleAz, c.SubtitleEn, c.SubtitleRu,
            c.ImageUrl, c.IconKey, c.WorkingHours, c.InternalExtension, c.LayoutStyle, c.DisplayOrder, c.IsActive,
            c.ExternalId, c.CollapseIfSingleItem);

    private static CatalogItemDto MapToCatalogItemDto(CatalogItem i) =>
        new(i.Id, i.HubId, i.CategoryId, i.ItemType, i.TitleAz, i.TitleEn, i.TitleRu, i.DescriptionAz, i.DescriptionEn, i.DescriptionRu,
            i.BadgeAz, i.BadgeEn, i.BadgeRu, i.FilterTag, i.Price, i.OldPrice, i.Currency, i.PortionOrDuration,
            i.BulletPointsAz, i.BulletPointsEn, i.BulletPointsRu, i.ImageUrl, i.IsOrderable, i.IsFeaturedInDailySpecials,
            i.ActionLabelAz, i.ActionLabelEn, i.ActionLabelRu, i.ActionUrl, i.DisplayOrder, i.IsActive,
            i.ExternalId, i.SubtitleAz, i.SubtitleEn, i.SubtitleRu, i.IconKey, i.ActionType, i.ActionValue,
            i.DiscountPercent, i.CostPrice, i.PrepTimeMinutes, i.NoServiceFee, i.ModifierGroupsJson);

    private static GuestOrderDto MapToOrderDto(GuestOrder o) =>
        new(o.Id, o.OrderNumber, o.OrderType, o.RoomOrTableNumber, o.GuestName, o.PhoneNumber, o.Notes,
            o.TotalAmount, o.Currency, o.Status, o.IsEmailSent, o.IsTelegramSent, o.CreatedAtUtc,
            o.Items.Select(i => new GuestOrderItemDto(
                i.Id, i.CatalogItemId, i.ItemTitle, i.Quantity, i.UnitPrice, i.LineTotal, i.SpecialInstruction
            )).ToList());

    private static DepartmentContactDto MapToDepartmentDto(HotelDepartmentContact d) =>
        new(d.Id, d.TitleAz, d.TitleEn, d.TitleRu, d.SubtitleAz, d.SubtitleEn, d.SubtitleRu,
            d.PhoneNumber, d.InternalExtension, d.WhatsAppNumber, d.Email, d.WorkingHours, d.IconKey, d.DisplayOrder, d.IsActive);
}
