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

        builder.Property(t => t.SubtitleAz).HasMaxLength(200);
        builder.Property(t => t.SubtitleEn).HasMaxLength(200);
        builder.Property(t => t.SubtitleRu).HasMaxLength(200);
        builder.Property(t => t.TaglineAz).HasMaxLength(200);
        builder.Property(t => t.TaglineEn).HasMaxLength(200);
        builder.Property(t => t.TaglineRu).HasMaxLength(200);
        builder.Property(t => t.BioAz).HasMaxLength(500);
        builder.Property(t => t.BioEn).HasMaxLength(500);
        builder.Property(t => t.BioRu).HasMaxLength(500);
        builder.Property(t => t.CurrencyCode).HasMaxLength(15);
        builder.Property(t => t.ServiceFeePercent).HasPrecision(5, 2);
        builder.Property(t => t.PrimaryColor).HasMaxLength(30);
        builder.Property(t => t.ThemePreset).HasMaxLength(30);
        builder.Property(t => t.ThemeStyle).HasMaxLength(30);

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

public class HotelStoryConfiguration : IEntityTypeConfiguration<HotelStory>
{
    public void Configure(EntityTypeBuilder<HotelStory> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TitleAz).IsRequired().HasMaxLength(150);
        builder.Property(s => s.TitleEn).HasMaxLength(150);
        builder.Property(s => s.TitleRu).HasMaxLength(150);
        builder.Property(s => s.ButtonTextAz).HasMaxLength(100);
        builder.Property(s => s.ButtonTextEn).HasMaxLength(100);
        builder.Property(s => s.ButtonTextRu).HasMaxLength(100);
        builder.Property(s => s.BadgeText).HasMaxLength(80);
        builder.Property(s => s.ImageUrl).HasMaxLength(500);
        builder.Property(s => s.VideoUrl).HasMaxLength(500);
        builder.Property(s => s.BannerImageUrl).HasMaxLength(500);
        builder.Property(s => s.ActionUrl).HasMaxLength(500);

        builder.HasOne(s => s.Tenant)
            .WithMany(t => t.Stories)
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HotelHubConfiguration : IEntityTypeConfiguration<HotelHub>
{
    public void Configure(EntityTypeBuilder<HotelHub> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Code).IsRequired().HasMaxLength(50);
        builder.Property(h => h.DisplayMode).HasMaxLength(40);
        builder.Property(h => h.TitleAz).IsRequired().HasMaxLength(150);
        builder.Property(h => h.TitleEn).HasMaxLength(150);
        builder.Property(h => h.TitleRu).HasMaxLength(150);
        builder.Property(h => h.KickerAz).HasMaxLength(80);
        builder.Property(h => h.KickerEn).HasMaxLength(80);
        builder.Property(h => h.KickerRu).HasMaxLength(80);
        builder.Property(h => h.IconKey).HasMaxLength(50);
        builder.Property(h => h.CoverImageUrl).HasMaxLength(500);

        builder.HasOne(h => h.Tenant)
            .WithMany(t => t.Hubs)
            .HasForeignKey(h => h.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CatalogCategoryConfiguration : IEntityTypeConfiguration<CatalogCategory>
{
    public void Configure(EntityTypeBuilder<CatalogCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.TitleAz).IsRequired().HasMaxLength(200);
        builder.Property(c => c.TitleEn).HasMaxLength(200);
        builder.Property(c => c.TitleRu).HasMaxLength(200);
        builder.Property(c => c.SubtitleAz).HasMaxLength(300);
        builder.Property(c => c.SubtitleEn).HasMaxLength(300);
        builder.Property(c => c.SubtitleRu).HasMaxLength(300);
        builder.Property(c => c.WorkingHours).HasMaxLength(100);
        builder.Property(c => c.InternalExtension).HasMaxLength(50);
        builder.Property(c => c.LayoutStyle).HasMaxLength(30);

        builder.HasOne(c => c.Tenant)
            .WithMany(t => t.CatalogCategories)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Hub)
            .WithMany(h => h.Categories)
            .HasForeignKey(c => c.HubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(p => p.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.TitleAz).IsRequired().HasMaxLength(250);
        builder.Property(i => i.TitleEn).HasMaxLength(250);
        builder.Property(i => i.TitleRu).HasMaxLength(250);
        builder.Property(i => i.SubtitleAz).HasMaxLength(300);
        builder.Property(i => i.SubtitleEn).HasMaxLength(300);
        builder.Property(i => i.SubtitleRu).HasMaxLength(300);
        builder.Property(i => i.BadgeAz).HasMaxLength(100);
        builder.Property(i => i.BadgeEn).HasMaxLength(100);
        builder.Property(i => i.BadgeRu).HasMaxLength(100);
        builder.Property(i => i.IconKey).HasMaxLength(60);
        builder.Property(i => i.FilterTag).HasMaxLength(50);
        builder.Property(i => i.Price).HasPrecision(18, 2);
        builder.Property(i => i.OldPrice).HasPrecision(18, 2);
        builder.Property(i => i.CostPrice).HasPrecision(18, 2);
        builder.Property(i => i.DiscountPercent).HasPrecision(5, 2);
        builder.Property(i => i.Currency).HasMaxLength(15);
        builder.Property(i => i.PortionOrDuration).HasMaxLength(100);
        builder.Property(i => i.ImageUrl).HasMaxLength(500);
        builder.Property(i => i.ActionType).HasMaxLength(50);
        builder.Property(i => i.ActionValue).HasMaxLength(500);
        builder.Property(i => i.ActionUrl).HasMaxLength(500);

        builder.HasOne(i => i.Tenant)
            .WithMany(t => t.CatalogItems)
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Hub)
            .WithMany(h => h.DirectItems)
            .HasForeignKey(i => i.HubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class GuestOrderConfiguration : IEntityTypeConfiguration<GuestOrder>
{
    public void Configure(EntityTypeBuilder<GuestOrder> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.RoomOrTableNumber).IsRequired().HasMaxLength(80);
        builder.Property(o => o.GuestName).IsRequired().HasMaxLength(150);
        builder.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Property(o => o.Currency).HasMaxLength(15);

        builder.HasOne(o => o.Tenant)
            .WithMany(t => t.GuestOrders)
            .HasForeignKey(o => o.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GuestOrderItemConfiguration : IEntityTypeConfiguration<GuestOrderItem>
{
    public void Configure(EntityTypeBuilder<GuestOrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ItemTitle).IsRequired().HasMaxLength(250);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);
    }
}

public class GuestReviewConfiguration : IEntityTypeConfiguration<GuestReview>
{
    public void Configure(EntityTypeBuilder<GuestReview> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.GuestName).HasMaxLength(150);
        builder.Property(r => r.RoomNumber).HasMaxLength(50);
        builder.Property(r => r.PhoneNumber).HasMaxLength(50);
        builder.Property(r => r.Department).HasMaxLength(80);
        builder.Property(r => r.Comment).HasMaxLength(2000);

        builder.HasOne(r => r.Tenant)
            .WithMany(t => t.GuestReviews)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HotelDepartmentContactConfiguration : IEntityTypeConfiguration<HotelDepartmentContact>
{
    public void Configure(EntityTypeBuilder<HotelDepartmentContact> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.TitleAz).IsRequired().HasMaxLength(150);
        builder.Property(d => d.TitleEn).HasMaxLength(150);
        builder.Property(d => d.TitleRu).HasMaxLength(150);
        builder.Property(d => d.PhoneNumber).HasMaxLength(80);
        builder.Property(d => d.InternalExtension).HasMaxLength(50);
        builder.Property(d => d.WhatsAppNumber).HasMaxLength(80);
        builder.Property(d => d.Email).HasMaxLength(150);
        builder.Property(d => d.WorkingHours).HasMaxLength(100);

        builder.HasOne(d => d.Tenant)
            .WithMany(t => t.DepartmentContacts)
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

