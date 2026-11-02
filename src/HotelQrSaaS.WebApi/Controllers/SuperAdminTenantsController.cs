using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelQrSaaS.Application.Common;
using HotelQrSaaS.Application.DTOs;
using HotelQrSaaS.Application.Services;

namespace HotelQrSaaS.WebApi.Controllers;

[ApiController]
[Route("api/super-admin")]
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminTenantsController : ControllerBase
{
    private readonly ISuperAdminService _superAdminService;

    public SuperAdminTenantsController(ISuperAdminService superAdminService)
    {
        _superAdminService = superAdminService;
    }

    /// <summary>
    /// Bütün otellərin siyahısını və abunəlik vəziyyətlərini gətirmək
    /// </summary>
    [HttpGet("tenants")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantSummaryDto>>>> GetTenants(CancellationToken cancellationToken)
    {
        var result = await _superAdminService.GetTenantsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TenantSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Yeni otel və onun üçün ilk HotelAdmin istifadəçisini yaratmaq
    /// </summary>
    [HttpPost("tenants")]
    public async Task<ActionResult<ApiResponse<TenantSummaryDto>>> CreateTenant(
        [FromBody] CreateTenantRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _superAdminService.CreateTenantAsync(request, cancellationToken);
        return Ok(ApiResponse<TenantSummaryDto>.Ok(result, "Otel və admin hesabı uğurla yaradıldı."));
    }

    /// <summary>
    /// Ödəniş etməyən oteli dərhal söndürmək / abunəlik statusunu dəyişmək (Kill-switch)
    /// </summary>
    [HttpPatch("tenants/{id:guid}/subscription")]
    public async Task<ActionResult<ApiResponse<TenantSummaryDto>>> UpdateSubscription(
        [FromRoute] Guid id,
        [FromBody] UpdateSubscriptionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _superAdminService.UpdateSubscriptionAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TenantSummaryDto>.Ok(result, "Otelin abunəlik statusu yeniləndi."));
    }

    /// <summary>
    /// Oteli silmək (Soft Delete)
    /// </summary>
    [HttpDelete("tenants/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTenant(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _superAdminService.DeleteTenantAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Otel uğurla silindi."));
    }

    /// <summary>
    /// SuperAdmin paneli üçün ümumi SaaS statistikası
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<SuperAdminDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _superAdminService.GetDashboardStatsAsync(cancellationToken);
        return Ok(ApiResponse<SuperAdminDashboardDto>.Ok(result));
    }
}

