using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HotelQrSaaS.Application.Interfaces.Services;
using HotelQrSaaS.Domain.Common;
using HotelQrSaaS.Domain.Entities;

namespace HotelQrSaaS.Persistence.Contexts;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentTenantService? _currentTenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService? currentTenantService = null) : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LinkCategory> LinkCategories => Set<LinkCategory>();
    public DbSet<LinkItem> LinkItems => Set<LinkItem>();
    public DbSet<GuestInquiry> GuestInquiries => Set<GuestInquiry>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<QrCodeSetting> QrCodeSettings => Set<QrCodeSetting>();
    public DbSet<HotelActivity> HotelActivities => Set<HotelActivity>();
    public DbSet<HotelStory> HotelStories => Set<HotelStory>();
    public DbSet<HotelHub> HotelHubs => Set<HotelHub>();
    public DbSet<CatalogCategory> CatalogCategories => Set<CatalogCategory>();
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<GuestOrder> GuestOrders => Set<GuestOrder>();
    public DbSet<GuestOrderItem> GuestOrderItems => Set<GuestOrderItem>();
    public DbSet<GuestReview> GuestReviews => Set<GuestReview>();
    public DbSet<HotelDepartmentContact> HotelDepartmentContacts => Set<HotelDepartmentContact>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global Query Filters
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // Soft delete filter
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var filter = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                builder.Entity(entityType.ClrType).HasQueryFilter(System.Linq.Expressions.Expression.Lambda(filter, parameter));
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = utcNow;
            }
        }

        // Auto-assign TenantId for ITenantEntity if not already assigned
        if (_currentTenantService?.TenantId.HasValue == true)
        {
            var tenantEntries = ChangeTracker.Entries<ITenantEntity>()
                .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty);

            foreach (var entry in tenantEntries)
            {
                entry.Entity.TenantId = _currentTenantService.TenantId.Value;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

