using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HotelQrSaaS.Domain.Entities;

namespace HotelQrSaaS.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Slug).IsUnique();

        builder.Property(t => t.BioAz).HasMaxLength(500);
        builder.Property(t => t.BioEn).HasMaxLength(500);
        builder.Property(t => t.BioRu).HasMaxLength(500);
        builder.Property(t => t.PrimaryColor).HasMaxLength(30);

        builder.Property(t => t.Phone).HasMaxLength(100);
        builder.Property(t => t.Email).HasMaxLength(150);
        builder.Property(t => t.WhatsApp).HasMaxLength(100);
        builder.Property(t => t.Telegram).HasMaxLength(100);

        builder.Property(t => t.NotificationEmail).HasMaxLength(150);
        builder.Property(t => t.TelegramChatId).HasMaxLength(100);

        builder.HasMany(t => t.LinkCategories)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.LinkItems)
            .WithOne(l => l.Tenant)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.GuestInquiries)
            .WithOne(i => i.Tenant)
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.AnalyticsEvents)
            .WithOne(a => a.Tenant)
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.QrCodeSetting)
            .WithOne(q => q.Tenant)
            .HasForeignKey<QrCodeSetting>(q => q.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LinkCategoryConfiguration : IEntityTypeConfiguration<LinkCategory>
{
    public void Configure(EntityTypeBuilder<LinkCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.TitleAz).IsRequired().HasMaxLength(150);
        builder.Property(c => c.LayoutType).HasMaxLength(20);

        builder.HasMany(c => c.Links)
            .WithOne(l => l.Category)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LinkItemConfiguration : IEntityTypeConfiguration<LinkItem>
{
    public void Configure(EntityTypeBuilder<LinkItem> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.TitleAz).IsRequired().HasMaxLength(200);
        builder.Property(l => l.TargetUrl).IsRequired().HasMaxLength(1000);
    }
}

public class GuestInquiryConfiguration : IEntityTypeConfiguration<GuestInquiry>
{
    public void Configure(EntityTypeBuilder<GuestInquiry> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.FullName).IsRequired().HasMaxLength(150);
        builder.Property(g => g.PhoneNumber).IsRequired().HasMaxLength(50);
        builder.Property(g => g.Email).HasMaxLength(150);
        builder.Property(g => g.RoomNumber).HasMaxLength(50);
        builder.Property(g => g.Message).IsRequired().HasMaxLength(2000);
    }
}

public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.Property(a => a.DeviceType).HasMaxLength(50);
        builder.Property(a => a.Language).HasMaxLength(20);

        builder.HasIndex(a => new { a.TenantId, a.EventType, a.CreatedAtUtc });
    }
}

public class HotelActivityConfiguration : IEntityTypeConfiguration<HotelActivity>
{
    public void Configure(EntityTypeBuilder<HotelActivity> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TitleAz).IsRequired().HasMaxLength(200);
        builder.Property(a => a.TitleEn).HasMaxLength(200);
        builder.Property(a => a.TitleRu).HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.Location).IsRequired().HasMaxLength(150);
        builder.Property(a => a.StartTime).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EndTime).HasMaxLength(50);
        builder.Property(a => a.Category).HasMaxLength(50);
        builder.Property(a => a.ImageUrl).HasMaxLength(500);

        builder.HasOne(a => a.Tenant)
            .WithMany(t => t.Activities)
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.TenantId, a.Date, a.IsActive });
    }
}

