using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using HotelQrSaaS.Application.Interfaces.Services;

namespace HotelQrSaaS.WebApi.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;
    private string? _tenantSlug;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        ExtractTenantFromClaims();
    }

    public Guid? TenantId => _tenantId;
    public string? TenantSlug => _tenantSlug;

    public void SetTenant(Guid tenantId, string slug)
    {
        _tenantId = tenantId;
        _tenantSlug = slug;
    }

    private void ExtractTenantFromClaims()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return;

        var tenantIdClaim = user.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var id))
        {
            _tenantId = id;
        }

        var slugClaim = user.FindFirst("tenant_slug")?.Value;
        if (!string.IsNullOrEmpty(slugClaim))
        {
            _tenantSlug = slugClaim;
        }
    }
}

