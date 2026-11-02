using System.Linq.Expressions;
using HotelQrSaaS.Domain.Common;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}

public interface ITenantRepository : IGenericRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, bool includeFullDetails = false, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeTenantId = null, CancellationToken cancellationToken = default);
}

public interface ILinkCategoryRepository : IGenericRepository<LinkCategory>
{
    Task<IReadOnlyList<LinkCategory>> GetCategoriesWithLinksAsync(Guid tenantId, bool activeOnly = true, CancellationToken cancellationToken = default);
}

public interface ILinkItemRepository : IGenericRepository<LinkItem>
{
    Task<LinkItem?> GetWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task IncrementClickCountAsync(Guid linkId, CancellationToken cancellationToken = default);
}

public interface IGuestInquiryRepository : IGenericRepository<GuestInquiry>
{
    Task<IReadOnlyList<GuestInquiry>> GetTenantInquiriesAsync(Guid tenantId, InquiryStatus? status = null, CancellationToken cancellationToken = default);
}

public interface IAnalyticsRepository : IGenericRepository<AnalyticsEvent>
{
    Task<int> GetScanCountAsync(Guid tenantId, DateTime? startDate = null, CancellationToken cancellationToken = default);
    Task<int> GetVCardDownloadCountAsync(Guid tenantId, DateTime? startDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetDeviceBreakdownAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

