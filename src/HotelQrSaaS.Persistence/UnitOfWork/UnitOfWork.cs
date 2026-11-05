using HotelQrSaaS.Application.Interfaces.Repositories;
using HotelQrSaaS.Application.Interfaces.UnitOfWork;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Persistence.Contexts;
using HotelQrSaaS.Persistence.Repositories;

namespace HotelQrSaaS.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private ITenantRepository? _tenants;
    private ILinkCategoryRepository? _linkCategories;
    private ILinkItemRepository? _linkItems;
    private IGuestInquiryRepository? _guestInquiries;
    private IAnalyticsRepository? _analytics;
    private IGenericRepository<QrCodeSetting>? _qrSettings;
    private IGenericRepository<HotelActivity>? _activities;
    private IGenericRepository<HotelStory>? _stories;
    private IGenericRepository<HotelHub>? _hubs;
    private IGenericRepository<CatalogCategory>? _catalogCategories;
    private IGenericRepository<CatalogItem>? _catalogItems;
    private IGuestOrderRepository? _guestOrders;
    private IGenericRepository<GuestOrderItem>? _guestOrderItems;
    private IGenericRepository<GuestReview>? _guestReviews;
    private IGenericRepository<HotelDepartmentContact>? _departmentContacts;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);
    public ILinkCategoryRepository LinkCategories => _linkCategories ??= new LinkCategoryRepository(_context);
    public ILinkItemRepository LinkItems => _linkItems ??= new LinkItemRepository(_context);
    public IGuestInquiryRepository GuestInquiries => _guestInquiries ??= new GuestInquiryRepository(_context);
    public IAnalyticsRepository Analytics => _analytics ??= new AnalyticsRepository(_context);
    public IGenericRepository<QrCodeSetting> QrSettings => _qrSettings ??= new GenericRepository<QrCodeSetting>(_context);
    public IGenericRepository<HotelActivity> Activities => _activities ??= new GenericRepository<HotelActivity>(_context);
    public IGenericRepository<HotelStory> Stories => _stories ??= new GenericRepository<HotelStory>(_context);
    public IGenericRepository<HotelHub> Hubs => _hubs ??= new GenericRepository<HotelHub>(_context);
    public IGenericRepository<CatalogCategory> CatalogCategories => _catalogCategories ??= new GenericRepository<CatalogCategory>(_context);
    public IGenericRepository<CatalogItem> CatalogItems => _catalogItems ??= new GenericRepository<CatalogItem>(_context);
    public IGuestOrderRepository GuestOrders => _guestOrders ??= new GuestOrderRepository(_context);
    public IGenericRepository<GuestOrderItem> GuestOrderItems => _guestOrderItems ??= new GenericRepository<GuestOrderItem>(_context);
    public IGenericRepository<GuestReview> GuestReviews => _guestReviews ??= new GenericRepository<GuestReview>(_context);
    public IGenericRepository<HotelDepartmentContact> DepartmentContacts => _departmentContacts ??= new GenericRepository<HotelDepartmentContact>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

