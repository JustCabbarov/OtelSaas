using Microsoft.AspNetCore.Identity;
using HotelQrSaaS.Application.Common.Exceptions;
using HotelQrSaaS.Application.DTOs;
using HotelQrSaaS.Application.Interfaces.UnitOfWork;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Application.Services;

public interface ISuperAdminService
{
    Task<IReadOnlyList<TenantSummaryDto>> GetTenantsAsync(CancellationToken cancellationToken = default);
    Task<TenantSummaryDto> CreateTenantAsync(CreateTenantRequestDto request, CancellationToken cancellationToken = default);
    Task<TenantSummaryDto> UpdateSubscriptionAsync(Guid tenantId, UpdateSubscriptionRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<SuperAdminDashboardDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}

public class SuperAdminService : ISuperAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public SuperAdminService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<TenantSummaryDto>> GetTenantsAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _unitOfWork.Tenants.ListAllAsync(cancellationToken);
        var result = new List<TenantSummaryDto>();

        foreach (var t in tenants)
        {
            var linksCount = await _unitOfWork.LinkItems.CountAsync(l => l.TenantId == t.Id, cancellationToken);
            var links = await _unitOfWork.LinkItems.FindAsync(l => l.TenantId == t.Id, cancellationToken);
            var totalClicks = links.Sum(l => l.ClickCount);

            result.Add(new TenantSummaryDto(
                t.Id,
                t.Name,
                t.Slug,
                t.LogoUrl,
                t.IsActive,
                t.SubscriptionStatus,
                t.SubscriptionEndDate,
                t.SuspensionReason,
                linksCount,
                totalClicks,
                t.CreatedAtUtc
            ));
        }

        return result;
    }

    public async Task<TenantSummaryDto> CreateTenantAsync(CreateTenantRequestDto request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _unitOfWork.Tenants.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new BadRequestException($"'{slug}' adlı slug artıq başqa bir otel üçün istifadə olunub.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.AdminEmail.Trim());
        if (existingUser != null)
        {
            throw new BadRequestException($"'{request.AdminEmail}' e-poçt ünvanı ilə artıq istifadəçi mövcuddur.");
        }

        var tenant = new Tenant
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Phone = request.Phone,
            IsActive = true,
            SubscriptionStatus = SubscriptionStatus.Active,
            SubscriptionEndDate = request.SubscriptionEndDate ?? DateTime.UtcNow.AddYears(1),
            DefaultLanguage = "az",
            PrimaryColor = "#191a1a",
            ThemeMode = ThemeMode.Dark,
            PreferredNotificationChannel = NotificationChannel.Both
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);

        // Default QR code settings
        var qrSetting = new QrCodeSetting
        {
            TenantId = tenant.Id,
            ForegroundColorHex = "#000000",
            BackgroundColorHex = "#FFFFFF",
            IncludeLogo = true,
            LogoSizePercent = 18,
            PixelsPerModule = 20
        };
        await _unitOfWork.QrSettings.AddAsync(qrSetting, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create HotelAdmin user
        var adminUser = new ApplicationUser
        {
            UserName = request.AdminEmail.Trim(),
            Email = request.AdminEmail.Trim(),
            FullName = request.AdminFullName.Trim(),
            TenantId = tenant.Id,
            EmailConfirmed = true
        };

        var identityResult = await _userManager.CreateAsync(adminUser, request.AdminPassword);
        if (!identityResult.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", identityResult.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(adminUser, "HotelAdmin");

        return new TenantSummaryDto(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LogoUrl,
            tenant.IsActive,
            tenant.SubscriptionStatus,
            tenant.SubscriptionEndDate,
            tenant.SuspensionReason,
            0,
            0,
            tenant.CreatedAtUtc
        );
    }

    public async Task<TenantSummaryDto> UpdateSubscriptionAsync(Guid tenantId, UpdateSubscriptionRequestDto request, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
        {
            throw new NotFoundException("Otel tapılmadı.");
        }

        tenant.IsActive = request.IsActive;
        tenant.SubscriptionStatus = request.SubscriptionStatus;
        tenant.SubscriptionEndDate = request.SubscriptionEndDate;
        tenant.SuspensionReason = request.SuspensionReason;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var linksCount = await _unitOfWork.LinkItems.CountAsync(l => l.TenantId == tenant.Id, cancellationToken);
        var links = await _unitOfWork.LinkItems.FindAsync(l => l.TenantId == tenant.Id, cancellationToken);
        var totalClicks = links.Sum(l => l.ClickCount);

        return new TenantSummaryDto(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LogoUrl,
            tenant.IsActive,
            tenant.SubscriptionStatus,
            tenant.SubscriptionEndDate,
            tenant.SuspensionReason,
            linksCount,
            totalClicks,
            tenant.CreatedAtUtc
        );
    }

    public async Task DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
        {
            throw new NotFoundException("Otel tapılmadı.");
        }

        _unitOfWork.Tenants.Delete(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SuperAdminDashboardDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var allTenants = await _unitOfWork.Tenants.ListAllAsync(cancellationToken);
        var totalTenants = allTenants.Count;
        var activeTenants = allTenants.Count(t => t.CanServePublicTraffic());
        var suspendedTenants = totalTenants - activeTenants;

        var totalInquiries = await _unitOfWork.GuestInquiries.CountAsync(cancellationToken: cancellationToken);
        var allLinks = await _unitOfWork.LinkItems.ListAllAsync(cancellationToken);
        var totalSystemClicks = allLinks.Sum(l => l.ClickCount);

        return new SuperAdminDashboardDto(
            totalTenants,
            activeTenants,
            suspendedTenants,
            totalInquiries,
            totalSystemClicks
        );
    }
}

