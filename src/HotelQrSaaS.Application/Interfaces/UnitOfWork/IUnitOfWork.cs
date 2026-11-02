using HotelQrSaaS.Application.Interfaces.Repositories;
using HotelQrSaaS.Domain.Entities;

namespace HotelQrSaaS.Application.Interfaces.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    ITenantRepository Tenants { get; }
    ILinkCategoryRepository LinkCategories { get; }
    ILinkItemRepository LinkItems { get; }
    IGuestInquiryRepository GuestInquiries { get; }
    IAnalyticsRepository Analytics { get; }
    IGenericRepository<QrCodeSetting> QrSettings { get; }
    IGenericRepository<HotelActivity> Activities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

