using Microsoft.EntityFrameworkCore;
using HotelQrSaaS.Application.Interfaces.Repositories;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;
using HotelQrSaaS.Persistence.Contexts;

namespace HotelQrSaaS.Persistence.Repositories;

public class TenantRepository : GenericRepository<Tenant>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Tenant?> GetBySlugAsync(string slug, bool includeFullDetails = false, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        if (!includeFullDetails)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug.ToLower() == normalizedSlug, cancellationToken);
        }

        return await _dbSet
            .Include(t => t.LinkCategories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Links.OrderBy(l => l.DisplayOrder))
            .Include(t => t.QrCodeSetting)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug.ToLower() == normalizedSlug, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeTenantId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var query = _dbSet.Where(t => t.Slug.ToLower() == normalizedSlug);

        if (excludeTenantId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTenantId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}

public class LinkCategoryRepository : GenericRepository<LinkCategory>, ILinkCategoryRepository
{
    public LinkCategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LinkCategory>> GetCategoriesWithLinksAsync(Guid tenantId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(c => c.TenantId == tenantId);

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .Include(c => c.Links.Where(l => !activeOnly || l.IsActive).OrderBy(l => l.DisplayOrder))
            .OrderBy(c => c.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

public class LinkItemRepository : GenericRepository<LinkItem>, ILinkItemRepository
{
    public LinkItemRepository(ApplicationDbContext context) : base(context) { }

    public async Task<LinkItem?> GetWithCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task IncrementClickCountAsync(Guid linkId, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(l => l.Id == linkId)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.ClickCount, b => b.ClickCount + 1), cancellationToken);
    }
}

public class GuestInquiryRepository : GenericRepository<GuestInquiry>, IGuestInquiryRepository
{
    public GuestInquiryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<GuestInquiry>> GetTenantInquiriesAsync(Guid tenantId, InquiryStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(g => g.TenantId == tenantId);

        if (status.HasValue)
        {
            query = query.Where(g => g.Status == status.Value);
        }

        return await query
            .OrderByDescending(g => g.CreatedAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

public class AnalyticsRepository : GenericRepository<AnalyticsEvent>, IAnalyticsRepository
{
    public AnalyticsRepository(ApplicationDbContext context) : base(context) { }

    public async Task<int> GetScanCountAsync(Guid tenantId, DateTime? startDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.TenantId == tenantId && 
            (a.EventType == AnalyticsEventType.QrCodeScan || a.EventType == AnalyticsEventType.ProfileView));

        if (startDate.HasValue)
        {
            query = query.Where(a => a.CreatedAtUtc >= startDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetVCardDownloadCountAsync(Guid tenantId, DateTime? startDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.TenantId == tenantId && a.EventType == AnalyticsEventType.VCardDownload);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.CreatedAtUtc >= startDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetDeviceBreakdownAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.TenantId == tenantId && a.DeviceType != null)
            .GroupBy(a => a.DeviceType!)
            .Select(g => new { Device = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Device, x => x.Count, cancellationToken);
    }
}

