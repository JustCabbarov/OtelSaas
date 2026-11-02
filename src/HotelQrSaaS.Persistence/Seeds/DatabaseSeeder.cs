using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;
using HotelQrSaaS.Persistence.Contexts;

namespace HotelQrSaaS.Persistence.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // 1. Seed Roles
        string[] roles = { "SuperAdmin", "HotelAdmin", "HotelStaff" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        // 2. Seed SuperAdmin User
        const string superAdminEmail = "admin@hotelqr.com";
        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "Platform Super Admin",
                EmailConfirmed = true,
                TenantId = null // System wide
            };

            var result = await userManager.CreateAsync(superAdmin, "SuperAdmin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            }
        }

        // 3. Seed Demo Hotel ("Hotel & Resort") if no tenants exist
        if (!await context.Tenants.AnyAsync())
        {
            var demoTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Hotel & Resort",
                Slug = "hotel-resort",
                BioAz = "Hotels and Resorts ilə dünya üzrə lüks otel və kurortları kəşf edin!",
                BioEn = "Discover luxury hotels and resorts worldwide with Hotels and Resorts!",
                BioRu = "Откройте для себя роскошные отели и курорты по всему миру!",
                DefaultLanguage = "az",
                PrimaryColor = "#191a1a",
                ThemeMode = ThemeMode.Dark,
                Phone = "+994 12 599 00 00",
                Email = "info@hotelresort.az",
                WhatsApp = "+994501234567",
                Telegram = "hotelresort_bot",
                Address = "Bakı şəhəri, Neftçilər prospekti 1",
                MapUrl = "https://maps.google.com/?q=Baku",
                IsActive = true,
                SubscriptionStatus = SubscriptionStatus.Active,
                SubscriptionEndDate = DateTime.UtcNow.AddYears(1),
                PreferredNotificationChannel = NotificationChannel.Both,
                NotificationEmail = "reception@hotelresort.az",
                TelegramChatId = "123456789"
            };

            await context.Tenants.AddAsync(demoTenant);

            // Seed Demo Hotel Admin
            const string hotelAdminEmail = "admin@hotelresort.com";
            var hotelAdmin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = hotelAdminEmail,
                Email = hotelAdminEmail,
                FullName = "Hotel General Manager",
                EmailConfirmed = true,
                TenantId = demoTenant.Id
            };
            var createHotelAdmin = await userManager.CreateAsync(hotelAdmin, "Hotel123!");
            if (createHotelAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(hotelAdmin, "HotelAdmin");
            }

            // Category 1: Contacts (icons layout)
            var catContacts = new LinkCategory
            {
                Id = Guid.NewGuid(),
                TenantId = demoTenant.Id,
                TitleAz = "Contacts",
                TitleEn = "Contacts",
                TitleRu = "Контакты",
                LayoutType = "icons",
                DisplayOrder = 1,
                IsActive = true
            };
            catContacts.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Phone",
                LinkType = LinkType.Phone,
                TargetUrl = "tel:+994125990000",
                IconKey = "phone",
                DisplayOrder = 1
            });
            catContacts.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Email",
                LinkType = LinkType.Email,
                TargetUrl = "mailto:info@hotelresort.az",
                IconKey = "mail",
                DisplayOrder = 2
            });
            catContacts.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "WhatsApp",
                LinkType = LinkType.WhatsApp,
                TargetUrl = "https://wa.me/994501234567",
                IconKey = "whatsapp",
                DisplayOrder = 3
            });
            catContacts.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Telegram",
                LinkType = LinkType.Telegram,
                TargetUrl = "https://t.me/hotelresort_bot",
                IconKey = "telegram",
                DisplayOrder = 4
            });

            // Category 2: Hotel Services (list layout)
            var catLinks = new LinkCategory
            {
                Id = Guid.NewGuid(),
                TenantId = demoTenant.Id,
                TitleAz = "Links",
                TitleEn = "Services & Links",
                TitleRu = "Услуги и ссылки",
                LayoutType = "list",
                DisplayOrder = 2,
                IsActive = true
            };
            catLinks.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Otaq növləri",
                TitleEn = "Room Types",
                TitleRu = "Типы номеров",
                LinkType = LinkType.StandardUrl,
                TargetUrl = "https://hotelresort.az/rooms",
                IconKey = "bed",
                DisplayOrder = 1
            });
            catLinks.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Restoran & Bar",
                TitleEn = "Restaurant & Bar",
                TitleRu = "Ресторан и Бар",
                LinkType = LinkType.StandardUrl,
                TargetUrl = "https://hotelresort.az/restaurants",
                IconKey = "restaurant",
                DisplayOrder = 2
            });
            catLinks.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Spa & Wellness",
                TitleEn = "Spa & Wellness",
                TitleRu = "Спа и оздоровление",
                LinkType = LinkType.StandardUrl,
                TargetUrl = "https://hotelresort.az/spa",
                IconKey = "spa",
                DisplayOrder = 3
            });
            catLinks.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Review us on Google",
                TitleEn = "Review us on Google",
                TitleRu = "Оставить отзыв в Google",
                LinkType = LinkType.GoogleReview,
                TargetUrl = "https://search.google.com/local/writereview",
                IconKey = "google",
                DisplayOrder = 4
            });

            // Category 3: Social Media (icons layout)
            var catSocial = new LinkCategory
            {
                Id = Guid.NewGuid(),
                TenantId = demoTenant.Id,
                TitleAz = "Social media",
                TitleEn = "Social media",
                TitleRu = "Социальные сети",
                LayoutType = "icons",
                DisplayOrder = 3,
                IsActive = true
            };
            catSocial.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Instagram",
                LinkType = LinkType.StandardUrl,
                TargetUrl = "https://instagram.com",
                IconKey = "instagram",
                DisplayOrder = 1
            });
            catSocial.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Facebook",
                LinkType = LinkType.StandardUrl,
                TargetUrl = "https://facebook.com",
                IconKey = "facebook",
                DisplayOrder = 2
            });
            catSocial.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "YouTube",
                LinkType = LinkType.StandardUrl,
                TargetUrl = "https://youtube.com",
                IconKey = "youtube",
                DisplayOrder = 3
            });

            // Category 4: Business & Menus (list layout)
            var catBusiness = new LinkCategory
            {
                Id = Guid.NewGuid(),
                TenantId = demoTenant.Id,
                TitleAz = "Business",
                TitleEn = "Business & Menus",
                TitleRu = "Меню и Бизнес",
                LayoutType = "list",
                DisplayOrder = 4,
                IsActive = true
            };
            catBusiness.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Bar Menu",
                TitleEn = "Bar Menu",
                TitleRu = "Барное меню",
                LinkType = LinkType.Flipbook,
                TargetUrl = "https://heyzine.com/flip-book/de9bc68c4c.html",
                IconKey = "menu",
                DisplayOrder = 1
            });
            catBusiness.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Food Menu",
                TitleEn = "Food Menu",
                TitleRu = "Меню блюд",
                LinkType = LinkType.Flipbook,
                TargetUrl = "https://heyzine.com/flip-book/sample.html",
                IconKey = "menu",
                DisplayOrder = 2
            });
            catBusiness.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Tripadvisor",
                LinkType = LinkType.TripAdvisor,
                TargetUrl = "https://tripadvisor.com",
                IconKey = "tripadvisor",
                DisplayOrder = 3
            });
            catBusiness.Links.Add(new LinkItem
            {
                TenantId = demoTenant.Id,
                TitleAz = "Booking.com",
                LinkType = LinkType.Booking,
                TargetUrl = "https://booking.com",
                IconKey = "booking",
                DisplayOrder = 4
            });

            await context.LinkCategories.AddRangeAsync(catContacts, catLinks, catSocial, catBusiness);

            // Seed QrCodeSetting
            var qrSetting = new QrCodeSetting
            {
                TenantId = demoTenant.Id,
                ForegroundColorHex = "#000000",
                BackgroundColorHex = "#FFFFFF",
                IncludeLogo = true,
                LogoSizePercent = 18,
                PixelsPerModule = 20
            };
            await context.QrCodeSettings.AddAsync(qrSetting);

            // Seed Activities
            if (!context.HotelActivities.Any())
            {
                var activities = new List<HotelActivity>
                {
                    new()
                    {
                        TenantId = demoTenant.Id,
                        TitleAz = "Hovuz Partisi & DJ Performance",
                        TitleEn = "Pool Party & Live DJ Performance",
                        TitleRu = "Вечеринка у бассейна и живой DJ",
                        Description = "Canlı DJ ritmləri, meyvə və kokteyl şousu ilə unudulmaz əyləncə",
                        Location = "Əsas Açıq Hovuz Kənarı",
                        StartTime = "15:00",
                        EndTime = "18:00",
                        Date = DateTime.UtcNow.Date,
                        Category = "party",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new()
                    {
                        TenantId = demoTenant.Id,
                        TitleAz = "Canlı Caz Gecəsi",
                        TitleEn = "Live Jazz Evening & Cocktails",
                        TitleRu = "Вечер живого джаза и коктейлей",
                        Description = "Klassik və müasir caz əsərləri canlı ifada, dəniz mənzərəsi ilə",
                        Location = "Sky Lounge & Terrace Bar",
                        StartTime = "20:30",
                        EndTime = "23:00",
                        Date = DateTime.UtcNow.Date,
                        Category = "music",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new()
                    {
                        TenantId = demoTenant.Id,
                        TitleAz = "Səhər Yoqası & Meditasiya",
                        TitleEn = "Morning Yoga & Meditation",
                        TitleRu = "Утренняя йога и медитация",
                        Description = "Təcrübəli təlimçi ilə günə təmiz dəniz havasında gümrah başlanğıc",
                        Location = "Çimərlik Zonası (Beachfront)",
                        StartTime = "08:30",
                        EndTime = "09:30",
                        Date = DateTime.UtcNow.Date,
                        Category = "wellness",
                        DisplayOrder = 3,
                        IsActive = true
                    },
                    new()
                    {
                        TenantId = demoTenant.Id,
                        TitleAz = "Uşaq Əyləncəsi & Köpük Şousu",
                        TitleEn = "Kids Animation & Bubble Show",
                        TitleRu = "Детская анимация и пенное шоу",
                        Description = "Peşəkar animatorlar, yarışlar, interaktiv oyunlar və köpük partisi",
                        Location = "Kids Club & Yaşıl Park",
                        StartTime = "11:00",
                        EndTime = "12:30",
                        Date = DateTime.UtcNow.Date,
                        Category = "kids",
                        DisplayOrder = 4,
                        IsActive = true
                    }
                };

                await context.HotelActivities.AddRangeAsync(activities);
            }

            await context.SaveChangesAsync();
        }
    }
}

