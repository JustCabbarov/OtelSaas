using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelQrSaaS.Application.Common;
using HotelQrSaaS.Application.Common.Exceptions;
using HotelQrSaaS.Application.DTOs;
using HotelQrSaaS.Application.Services;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.WebApi.Controllers;

[ApiController]
[Route("api/hotel-admin")]
[Authorize(Roles = "HotelAdmin,SuperAdmin")]
public class HotelAdminController : ControllerBase
{
    private readonly IHotelAdminService _hotelAdminService;

    public HotelAdminController(IHotelAdminService hotelAdminService)
    {
        _hotelAdminService = hotelAdminService;
    }

    private Guid GetTenantId(Guid? overrideTenantId = null)
    {
        // If SuperAdmin specifies a tenantId query param, allow it for support
        if (User.IsInRole("SuperAdmin") && overrideTenantId.HasValue)
        {
            return overrideTenantId.Value;
        }

        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var id))
        {
            return id;
        }

        throw new UnauthorizedException("Otel identifikatoru təyin edilməyib.");
    }

    #region Profile & Visuals
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<HotelProfileDto>>> GetProfile(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetProfileAsync(targetId, cancellationToken);
        return Ok(ApiResponse<HotelProfileDto>.Ok(result));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<HotelProfileDto>>> UpdateProfile(
        [FromBody] UpdateHotelProfileDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateProfileAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<HotelProfileDto>.Ok(result, "Otel profili uğurla yeniləndi."));
    }

    [HttpPost("profile/logo")]
    public async Task<ActionResult<ApiResponse<string>>> UploadLogo(
        IFormFile file,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) throw new BadRequestException("Fayl seçilməyib.");

        var targetId = GetTenantId(tenantId);
        using var stream = file.OpenReadStream();
        var url = await _hotelAdminService.UploadLogoAsync(targetId, stream, file.FileName, cancellationToken);

        return Ok(ApiResponse<string>.Ok(url, "Loqo uğurla yükləndi."));
    }

    [HttpPost("profile/cover")]
    public async Task<ActionResult<ApiResponse<string>>> UploadCover(
        IFormFile file,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) throw new BadRequestException("Fayl seçilməyib.");

        var targetId = GetTenantId(tenantId);
        using var stream = file.OpenReadStream();
        var url = await _hotelAdminService.UploadCoverAsync(targetId, stream, file.FileName, cancellationToken);

        return Ok(ApiResponse<string>.Ok(url, "Qapaq şəkli uğurla yükləndi."));
    }
    #endregion

    #region Notification Preferences (Email & Telegram)
    [HttpGet("notifications/settings")]
    public async Task<ActionResult<ApiResponse<NotificationSettingsDto>>> GetNotificationSettings(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetNotificationSettingsAsync(targetId, cancellationToken);
        return Ok(ApiResponse<NotificationSettingsDto>.Ok(result));
    }

    [HttpPut("notifications/settings")]
    public async Task<ActionResult<ApiResponse<NotificationSettingsDto>>> UpdateNotificationSettings(
        [FromBody] UpdateNotificationSettingsDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateNotificationSettingsAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<NotificationSettingsDto>.Ok(result, "Bildiriş tənzimləmələri yeniləndi."));
    }

    /// <summary>
    /// Otelin öz Telegram Bot Tokenini daxil edərək sistemi Telegram-a bağlaması (Avtomatik Chat ID aşkarlanması ilə)
    /// </summary>
    [HttpPost("notifications/telegram/connect")]
    public async Task<ActionResult<ApiResponse<ConnectTelegramBotResponseDto>>> ConnectTelegramBot(
        [FromBody] ConnectTelegramBotRequestDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.ConnectTelegramBotAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<ConnectTelegramBotResponseDto>.Ok(result, result.Message));
    }

    /// <summary>
    /// Otel üçün qonaq müraciətlərinin göndəriləcəyi bildiriş e-poçt ünvanını təyin etmək və sınaq mesajı göndərmək
    /// </summary>
    [HttpPost("notifications/email/connect")]
    public async Task<ActionResult<ApiResponse<ConnectEmailResponseDto>>> ConnectEmail(
        [FromBody] ConnectEmailRequestDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.ConnectEmailAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<ConnectEmailResponseDto>.Ok(result, result.Message));
    }

    [HttpPost("notifications/test")]
    public async Task<ActionResult<ApiResponse<bool>>> SendTestNotification(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var success = await _hotelAdminService.SendTestNotificationAsync(targetId, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(success, success ? "Sınaq bildirişi göndərildi." : "Bildiriş göndərilərkən xəta baş verdi."));
    }
    #endregion

    #region Categories & Links
    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetCategories(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetCategoriesAsync(targetId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.Ok(result));
    }

    [HttpPost("categories")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory(
        [FromBody] CreateCategoryDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateCategoryAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result, "Bölmə yaradıldı."));
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateCategoryAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result, "Bölmə yeniləndi."));
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteCategoryAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Bölmə silindi."));
    }

    [HttpPost("links")]
    public async Task<ActionResult<ApiResponse<LinkItemDto>>> CreateLink(
        [FromBody] CreateLinkItemDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateLinkAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<LinkItemDto>.Ok(result, "Keçid yaradıldı."));
    }

    [HttpPut("links/{id:guid}")]
    public async Task<ActionResult<ApiResponse<LinkItemDto>>> UpdateLink(
        [FromRoute] Guid id,
        [FromBody] UpdateLinkItemDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateLinkAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<LinkItemDto>.Ok(result, "Keçid yeniləndi."));
    }

    /// <summary>
    /// Menyu üçün PDF faylını və ya xüsusi link ikonunu serverə yükləmək
    /// </summary>
    [HttpPost("links/{id:guid}/upload-file")]
    public async Task<ActionResult<ApiResponse<string>>> UploadLinkFile(
        [FromRoute] Guid id,
        IFormFile file,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) throw new BadRequestException("Fayl seçilməyib.");

        var targetId = GetTenantId(tenantId);
        using var stream = file.OpenReadStream();
        var url = await _hotelAdminService.UploadLinkFileAsync(targetId, id, stream, file.FileName, cancellationToken);

        return Ok(ApiResponse<string>.Ok(url, "Fayl uğurla yükləndi."));
    }

    [HttpDelete("links/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLink(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteLinkAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Keçid silindi."));
    }

    [HttpPut("links/reorder")]
    public async Task<ActionResult<ApiResponse<bool>>> ReorderLinks(
        [FromBody] ReorderLinksDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.ReorderLinksAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Sıralama yadda saxlanıldı."));
    }
    #endregion

    #region Guest Inquiries
    [HttpGet("inquiries")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InquiryDto>>>> GetInquiries(
        [FromQuery] InquiryStatus? status,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetInquiriesAsync(targetId, status, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InquiryDto>>.Ok(result));
    }

    [HttpPatch("inquiries/{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<InquiryDto>>> UpdateInquiryStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateInquiryStatusDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateInquiryStatusAsync(targetId, id, request.Status, cancellationToken);
        return Ok(ApiResponse<InquiryDto>.Ok(result, "Müraciət statusu yeniləndi."));
    }
    #endregion

    #region QR Code & Analytics
    [HttpGet("qr/png")]
    public async Task<IActionResult> GetQrPng(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var bytes = await _hotelAdminService.GetPngQrCodeAsync(targetId, baseUrl, cancellationToken);

        return File(bytes, "image/png", "hotel-qr.png");
    }

    [HttpGet("qr/svg")]
    public async Task<IActionResult> GetQrSvg(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var svg = await _hotelAdminService.GetSvgQrCodeAsync(targetId, baseUrl, cancellationToken);

        return Content(svg, "image/svg+xml");
    }

    [HttpGet("analytics/summary")]
    public async Task<ActionResult<ApiResponse<AnalyticsSummaryDto>>> GetAnalytics(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetAnalyticsSummaryAsync(targetId, cancellationToken);
        return Ok(ApiResponse<AnalyticsSummaryDto>.Ok(result));
    }
    #endregion

    #region Hotel Activities
    /// <summary>
    /// Otelin gündəlik fəaliyyət və əyləncə tədbirlərini gətirmək (Hovuz partisi, canlı musiqi və s.)
    /// </summary>
    [HttpGet("activities")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HotelActivityDto>>>> GetActivities(
        [FromQuery] DateTime? date,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetActivitiesAsync(targetId, date, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<HotelActivityDto>>.Ok(result));
    }

    /// <summary>
    /// Yeni fəaliyyət / əyləncə tədbiri əlavə etmək
    /// </summary>
    [HttpPost("activities")]
    public async Task<ActionResult<ApiResponse<HotelActivityDto>>> CreateActivity(
        [FromBody] CreateHotelActivityDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateActivityAsync(targetId, request, cancellationToken);
        return CreatedAtAction(nameof(GetActivities), new { tenantId = targetId }, ApiResponse<HotelActivityDto>.Ok(result, "Fəaliyyət uğurla əlavə edildi."));
    }

    /// <summary>
    /// Fəaliyyət məlumatlarını yeniləmək
    /// </summary>
    [HttpPut("activities/{id:guid}")]
    public async Task<ActionResult<ApiResponse<HotelActivityDto>>> UpdateActivity(
        [FromRoute] Guid id,
        [FromBody] UpdateHotelActivityDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateActivityAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<HotelActivityDto>.Ok(result, "Fəaliyyət yeniləndi."));
    }

    /// <summary>
    /// Fəaliyyəti silmək
    /// </summary>
    [HttpDelete("activities/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteActivity(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteActivityAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Fəaliyyət silindi."));
    }

    /// <summary>
    /// Fəaliyyətin aktiv/passiv statusunu dəyişmək
    /// </summary>
    [HttpPatch("activities/{id:guid}/toggle")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActivityStatus(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var isActive = await _hotelAdminService.ToggleActivityStatusAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(isActive, isActive ? "Fəaliyyət aktiv edildi." : "Fəaliyyət deaktiv edildi."));
    }
    #endregion

    #region Stories (Top Highlights Bar)
    [HttpGet("stories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HotelStoryDto>>>> GetStories(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetStoriesAsync(targetId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<HotelStoryDto>>.Ok(result));
    }

    [HttpPost("stories")]
    public async Task<ActionResult<ApiResponse<HotelStoryDto>>> CreateStory(
        [FromBody] CreateHotelStoryDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateStoryAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<HotelStoryDto>.Ok(result, "Story uğurla yaradıldı."));
    }

    [HttpPut("stories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<HotelStoryDto>>> UpdateStory(
        [FromRoute] Guid id,
        [FromBody] UpdateHotelStoryDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateStoryAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<HotelStoryDto>.Ok(result, "Story yeniləndi."));
    }

    [HttpPost("stories/{id:guid}/image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadStoryImage(
        [FromRoute] Guid id,
        IFormFile file,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) throw new BadRequestException("Fayl seçilməyib.");
        var targetId = GetTenantId(tenantId);
        using var stream = file.OpenReadStream();
        var url = await _hotelAdminService.UploadStoryImageAsync(targetId, id, stream, file.FileName, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Şəkil yükləndi."));
    }

    [HttpDelete("stories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteStory(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteStoryAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Story silindi."));
    }
    #endregion

    #region Hubs (6 Core Hotel Tiles)
    [HttpGet("hubs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<HotelHubDto>>>> GetHubs(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetHubsAsync(targetId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<HotelHubDto>>.Ok(result));
    }

    [HttpPost("hubs")]
    public async Task<ActionResult<ApiResponse<HotelHubDto>>> CreateHub(
        [FromBody] CreateHotelHubDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateHubAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<HotelHubDto>.Ok(result, "Əsas bölmə (Hub) yaradıldı."));
    }

    [HttpPut("hubs/{id:guid}")]
    public async Task<ActionResult<ApiResponse<HotelHubDto>>> UpdateHub(
        [FromRoute] Guid id,
        [FromBody] UpdateHotelHubDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateHubAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<HotelHubDto>.Ok(result, "Əsas bölmə (Hub) yeniləndi."));
    }

    [HttpPost("hubs/{id:guid}/cover")]
    public async Task<ActionResult<ApiResponse<string>>> UploadHubCover(
        [FromRoute] Guid id,
        IFormFile file,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) throw new BadRequestException("Fayl seçilməyib.");
        var targetId = GetTenantId(tenantId);
        using var stream = file.OpenReadStream();
        var url = await _hotelAdminService.UploadHubCoverAsync(targetId, id, stream, file.FileName, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Hub şəkli yükləndi."));
    }

    [HttpDelete("hubs/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteHub(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteHubAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Hub silindi."));
    }
    #endregion

    #region Catalog Categories (Outlets & Sub-sections)
    [HttpGet("catalog/categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CatalogCategoryDto>>>> GetCatalogCategories(
        [FromQuery] Guid? hubId,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetCatalogCategoriesAsync(targetId, hubId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CatalogCategoryDto>>.Ok(result));
    }

    [HttpPost("catalog/categories")]
    public async Task<ActionResult<ApiResponse<CatalogCategoryDto>>> CreateCatalogCategory(
        [FromBody] CreateCatalogCategoryDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateCatalogCategoryAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<CatalogCategoryDto>.Ok(result, "Kataloq kateqoriyası / məkanı yaradıldı."));
    }

    [HttpPut("catalog/categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CatalogCategoryDto>>> UpdateCatalogCategory(
        [FromRoute] Guid id,
        [FromBody] UpdateCatalogCategoryDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateCatalogCategoryAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<CatalogCategoryDto>.Ok(result, "Kataloq kateqoriyası yeniləndi."));
    }

    [HttpPost("catalog/categories/{id:guid}/image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadCatalogCategoryImage(
        [FromRoute] Guid id,
        IFormFile file,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) throw new BadRequestException("Fayl seçilməyib.");
        var targetId = GetTenantId(tenantId);
        using var stream = file.OpenReadStream();
        var url = await _hotelAdminService.UploadCatalogCategoryImageAsync(targetId, id, stream, file.FileName, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Şəkil yükləndi."));
    }

    [HttpDelete("catalog/categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCatalogCategory(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteCatalogCategoryAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Kateqoriya silindi."));
    }
    #endregion

    #region Catalog Items (Priced Dishes, Spa Services, Rooms, Info Cards)
    [HttpGet("catalog/items")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CatalogItemDto>>>> GetCatalogItems(
        [FromQuery] Guid? hubId,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetCatalogItemsAsync(targetId, hubId, categoryId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CatalogItemDto>>.Ok(result));
    }

    [HttpPost("catalog/items")]
    public async Task<ActionResult<ApiResponse<CatalogItemDto>>> CreateCatalogItem(
        [FromBody] CreateCatalogItemDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateCatalogItemAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<CatalogItemDto>.Ok(result, "Məhsul / xidmət uğurla yaradıldı."));
    }

    [HttpPut("catalog/items/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CatalogItemDto>>> UpdateCatalogItem(
        [FromRoute] Guid id,
        [FromBody] UpdateCatalogItemDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateCatalogItemAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<CatalogItemDto>.Ok(result, "Məhsul / xidmət yeniləndi."));
    }

    [HttpPost("catalog/items/{id:guid}/image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadCatalogItemImage(
        [FromRoute] Guid id,
        IFormFile file,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) throw new BadRequestException("Fayl seçilməyib.");
        var targetId = GetTenantId(tenantId);
        using var stream = file.OpenReadStream();
        var url = await _hotelAdminService.UploadCatalogItemImageAsync(targetId, id, stream, file.FileName, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Məhsul şəkli yükləndi."));
    }

    [HttpDelete("catalog/items/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCatalogItem(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteCatalogItemAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Məhsul silindi."));
    }
    #endregion

    #region Guest Cart Orders (Room Service / Table / Spa)
    [HttpGet("orders")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GuestOrderDto>>>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetOrdersAsync(targetId, status, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GuestOrderDto>>.Ok(result));
    }

    [HttpPatch("orders/{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<GuestOrderDto>>> UpdateOrderStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderStatusDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateOrderStatusAsync(targetId, id, request.Status, cancellationToken);
        return Ok(ApiResponse<GuestOrderDto>.Ok(result, "Sifariş statusu yeniləndi."));
    }
    #endregion

    #region Guest Reviews (5-Star Feedback)
    [HttpGet("reviews")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GuestReviewDto>>>> GetReviews(
        [FromQuery] bool? isResolved,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetReviewsAsync(targetId, isResolved, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GuestReviewDto>>.Ok(result));
    }

    [HttpPatch("reviews/{id:guid}/resolve")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleReviewResolved(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var resolved = await _hotelAdminService.ToggleReviewResolvedAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(resolved, resolved ? "Rəy baxılmış kimi işarələndi." : "Rəy yenidən açıq statusuna qaytarıldı."));
    }
    #endregion

    #region Department Contacts
    [HttpGet("departments")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentContactDto>>>> GetDepartmentContacts(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.GetDepartmentContactsAsync(targetId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DepartmentContactDto>>.Ok(result));
    }

    [HttpPost("departments")]
    public async Task<ActionResult<ApiResponse<DepartmentContactDto>>> CreateDepartmentContact(
        [FromBody] CreateDepartmentContactDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.CreateDepartmentContactAsync(targetId, request, cancellationToken);
        return Ok(ApiResponse<DepartmentContactDto>.Ok(result, "Şöbə əlaqəsi əlavə edildi."));
    }

    [HttpPut("departments/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DepartmentContactDto>>> UpdateDepartmentContact(
        [FromRoute] Guid id,
        [FromBody] UpdateDepartmentContactDto request,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        var result = await _hotelAdminService.UpdateDepartmentContactAsync(targetId, id, request, cancellationToken);
        return Ok(ApiResponse<DepartmentContactDto>.Ok(result, "Şöbə əlaqəsi yeniləndi."));
    }

    [HttpDelete("departments/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartmentContact(
        [FromRoute] Guid id,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var targetId = GetTenantId(tenantId);
        await _hotelAdminService.DeleteDepartmentContactAsync(targetId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Şöbə əlaqəsi silindi."));
    }
    #endregion
}

