using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelQrSaaS.Application.Common;
using HotelQrSaaS.Application.DTOs;
using HotelQrSaaS.Application.Services;

namespace HotelQrSaaS.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
public class PublicHotelController : ControllerBase
{
    private readonly IPublicPortalService _publicPortalService;

    public PublicHotelController(IPublicPortalService publicPortalService)
    {
        _publicPortalService = publicPortalService;
    }

    /// <summary>
    /// Qonaq üçün otelin rəqəmsal profilini və aktiv linklərini gətirmək (QR kod oxudulduqda açılan səhifə)
    /// </summary>
    [HttpGet("api/public/{slug}")]
    public async Task<ActionResult<ApiResponse<PublicHotelPortalDto>>> GetHotelPortal(
        [FromRoute] string slug,
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _publicPortalService.GetPublicPortalAsync(slug, lang, ip, userAgent, cancellationToken);
        return Ok(ApiResponse<PublicHotelPortalDto>.Ok(result));
    }

    /// <summary>
    /// Otelin kontakt məlumatlarını bir kliklə qonağın telefonuna yükləmək (.vcf vCard)
    /// </summary>
    [HttpGet("api/public/{slug}/vcard")]
    public async Task<IActionResult> DownloadVCard(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var (bytes, fileName) = await _publicPortalService.GetVCardAsync(slug, ip, userAgent, cancellationToken);
        return File(bytes, "text/vcard; charset=utf-8", fileName);
    }

    /// <summary>
    /// Qonağın "Əlaqə / Connection" formundan otelə birbaşa sorğu və ya mesaj göndərməsi
    /// </summary>
    [HttpPost("api/public/{slug}/inquiries")]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitInquiry(
        [FromRoute] string slug,
        [FromBody] SubmitInquiryRequestDto request,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var success = await _publicPortalService.SubmitInquiryAsync(slug, request, ip, userAgent, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(success, "Müraciətiniz otelə çatdırıldı."));
    }

    /// <summary>
    /// Dinamik yönləndirmə: Menyu və ya xarici xidmət linkinə klikləri sayaraq hədəf ünvana 302 yönləndirmə
    /// </summary>
    [HttpGet("api/r/{linkId:guid}")]
    public async Task<IActionResult> RedirectLink(
        [FromRoute] Guid linkId,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var destinationUrl = await _publicPortalService.ProcessRedirectAsync(linkId, ip, userAgent, cancellationToken);

        // If it's a relative URL (like local /uploads/.../menu.pdf), resolve it
        if (!destinationUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !destinationUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
            !destinationUrl.StartsWith("tel:", StringComparison.OrdinalIgnoreCase) &&
            !destinationUrl.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            destinationUrl = $"{baseUrl.TrimEnd('/')}/{destinationUrl.TrimStart('/')}";
        }

        return Redirect(destinationUrl);
    }

    /// <summary>
    /// Qonaq QR kodu skan edəndə: Analitikanı (+1 scan) qeyd edir və dərhal Frontend səhifəsinə yönləndirir (HTTP 302)
    /// </summary>
    [HttpGet("q/{slug}")]
    [HttpGet("api/q/{slug}")]
    public async Task<IActionResult> ScanQrRedirect(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var destinationUrl = await _publicPortalService.ProcessQrScanRedirectAsync(slug, ip, userAgent, cancellationToken);

        // Əgər nisbi linkdirsə, cari host ilə birləşdir
        if (!destinationUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !destinationUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            destinationUrl = $"{baseUrl.TrimEnd('/')}/{destinationUrl.TrimStart('/')}";
        }

        return Redirect(destinationUrl);
    }

    /// <summary>
    /// Otelin dinamik QR kodunu PNG formatında gətirmək (Qonaq və ya brauzer üçün açıq endpoint)
    /// </summary>
    [HttpGet("api/public/{slug}/qr/png")]
    public async Task<IActionResult> GetQrPng(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var bytes = await _publicPortalService.GetQrPngAsync(slug, baseUrl, cancellationToken);
        return File(bytes, "image/png", $"{slug}-qr.png");
    }

    /// <summary>
    /// Otelin dinamik QR kodunu SVG vektor formatında gətirmək (Vektor çap üçün açıq endpoint)
    /// </summary>
    [HttpGet("api/public/{slug}/qr/svg")]
    public async Task<IActionResult> GetQrSvg(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var svg = await _publicPortalService.GetQrSvgAsync(slug, baseUrl, cancellationToken);
        return Content(svg, "image/svg+xml");
    }

    /// <summary>
    /// Qonaq üçün otelin gündəlik fəaliyyət və əyləncə təqvimini gətirmək (Açıq / public endpoint)
    /// </summary>
    [HttpGet("api/public/{slug}/activities")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PublicActivityDto>>>> GetActivities(
        [FromRoute] string slug,
        [FromQuery] string? lang,
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        var result = await _publicPortalService.GetActivitiesAsync(slug, lang, date, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PublicActivityDto>>.Ok(result));
    }
}

