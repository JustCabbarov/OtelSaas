using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using HotelQrSaaS.Application.Interfaces.Repositories;
using HotelQrSaaS.Application.Interfaces.Services;
using HotelQrSaaS.Application.Interfaces.UnitOfWork;
using HotelQrSaaS.Application.Services;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Infrastructure.Identity;
using HotelQrSaaS.Infrastructure.Notifications;
using HotelQrSaaS.Infrastructure.QrCode;
using HotelQrSaaS.Infrastructure.Storage;
using HotelQrSaaS.Persistence.Contexts;
using HotelQrSaaS.Persistence.Repositories;
using HotelQrSaaS.Persistence.Seeds;
using HotelQrSaaS.Persistence.UnitOfWork;
using HotelQrSaaS.WebApi.Middlewares;
using HotelQrSaaS.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Database & Entity Framework Core (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=hotel_qr_saas_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// 2. Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "SuperSecretHotelSaaSDigitalPlatformKey2026!#$SecureLongKey998877";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "HotelQrSaaS",
        ValidAudience = jwtSettings["Audience"] ?? "HotelQrSaaS",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// 4. Register Application & Infrastructure Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Tenant & Unit of Work
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ILinkCategoryRepository, LinkCategoryRepository>();
builder.Services.AddScoped<ILinkItemRepository, LinkItemRepository>();
builder.Services.AddScoped<IGuestInquiryRepository, GuestInquiryRepository>();
builder.Services.AddScoped<IGuestOrderRepository, GuestOrderRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IEmailNotificationSender, EmailNotificationSender>();
builder.Services.AddScoped<ITelegramNotificationSender, TelegramNotificationSender>();
builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IVCardService, VCardService>();

// Business Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISuperAdminService, SuperAdminService>();
builder.Services.AddScoped<IHotelAdminService, HotelAdminService>();
builder.Services.AddScoped<IPublicPortalService, PublicPortalService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Configure Swagger with JWT Bearer Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel QR & Concierge SaaS API",
        Version = "v1",
        Description = "Multi-Tenant Hotel Digital Concierge, Dynamic QR Code & Guest Services Platform (.NET 8)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Tokeninizi daxil edin (Bearer avtomatik tətbiq olunur)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 6. Middleware Pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

// Fix: Swagger UI regex expects OpenAPI <= 3.0.3, while Microsoft.OpenApi outputs 3.0.4
app.Use(async (context, next) =>
{
    if (context.Request.Path.Value != null && context.Request.Path.Value.EndsWith("swagger.json", StringComparison.OrdinalIgnoreCase))
    {
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next();

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        var responseBody = await reader.ReadToEndAsync();

        responseBody = responseBody.Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.1\"")
                                   .Replace("\"openapi\":\"3.0.4\"", "\"openapi\":\"3.0.1\"");

        var bytes = System.Text.Encoding.UTF8.GetBytes(responseBody);
        context.Response.Body = originalBodyStream;
        context.Response.ContentLength = bytes.Length;
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        return;
    }

    await next();
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel QR SaaS API v1");
    c.RoutePrefix = string.Empty; // Swagger UI directly at root: http://localhost:5055/
});

app.UseCors("AllowAll");

// Serve default index.html and static uploaded files (wwwroot) with explicit MIME types (.webp, .webmanifest, .mp4, etc.)
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".webp"] = "image/webp";
contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";
contentTypeProvider.Mappings[".mp4"] = "video/mp4";
contentTypeProvider.Mappings[".webm"] = "video/webm";
contentTypeProvider.Mappings[".m4v"] = "video/mp4";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 7. Auto Migration / DB Setup & Seed Data on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // If PostgreSQL is running, ensure DB schema and seed
        if (context.Database.CanConnect())
        {
            await context.Database.EnsureCreatedAsync();
            await context.Database.ExecuteSqlRawAsync("""
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "SubtitleAz" character varying(200);
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "SubtitleEn" character varying(200);
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "SubtitleRu" character varying(200);
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "TaglineAz" character varying(200);
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "TaglineEn" character varying(200);
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "TaglineRu" character varying(200);
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "StarRating" integer DEFAULT 5;
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "CurrencyCode" character varying(15) NOT NULL DEFAULT '₼';
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "ServiceFeePercent" numeric(5,2) NOT NULL DEFAULT 0;
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "ThemePreset" character varying(30) NOT NULL DEFAULT 'otel';
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "ThemeStyle" character varying(30) NOT NULL DEFAULT 'yumsaq';
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "InstagramUrl" text;
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "FacebookUrl" text;
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "YoutubeUrl" text;
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "TikTokUrl" text;
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "TripAdvisorUrl" text;
                ALTER TABLE "Tenants" ADD COLUMN IF NOT EXISTS "BookingUrl" text;

                CREATE TABLE IF NOT EXISTS "HotelStories" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                    "ExternalId" integer,
                    "TitleAz" character varying(150) NOT NULL,
                    "TitleEn" character varying(150),
                    "TitleRu" character varying(150),
                    "DescriptionAz" text,
                    "DescriptionEn" text,
                    "DescriptionRu" text,
                    "ButtonTextAz" character varying(100),
                    "ButtonTextEn" character varying(100),
                    "ButtonTextRu" character varying(100),
                    "ImageUrl" character varying(500),
                    "VideoUrl" character varying(500),
                    "BannerImageUrl" character varying(500),
                    "BadgeText" character varying(80),
                    "ActionUrl" character varying(500),
                    "DisplayOrder" integer NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    "UpdatedAtUtc" timestamp with time zone,
                    "IsDeleted" boolean NOT NULL,
                    "DeletedAtUtc" timestamp with time zone
                );
                ALTER TABLE "HotelStories" ADD COLUMN IF NOT EXISTS "ExternalId" integer;
                ALTER TABLE "HotelStories" ADD COLUMN IF NOT EXISTS "ButtonTextAz" character varying(100);
                ALTER TABLE "HotelStories" ADD COLUMN IF NOT EXISTS "ButtonTextEn" character varying(100);
                ALTER TABLE "HotelStories" ADD COLUMN IF NOT EXISTS "ButtonTextRu" character varying(100);
                ALTER TABLE "HotelStories" ADD COLUMN IF NOT EXISTS "VideoUrl" character varying(500);

                CREATE TABLE IF NOT EXISTS "HotelHubs" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                    "ExternalId" integer,
                    "Code" character varying(50) NOT NULL,
                    "HubType" integer NOT NULL,
                    "DisplayMode" character varying(40) NOT NULL DEFAULT 'categories',
                    "KickerAz" character varying(80),
                    "KickerEn" character varying(80),
                    "KickerRu" character varying(80),
                    "TitleAz" character varying(150) NOT NULL,
                    "TitleEn" character varying(150),
                    "TitleRu" character varying(150),
                    "SubtitleAz" text,
                    "SubtitleEn" text,
                    "SubtitleRu" text,
                    "IconKey" character varying(50),
                    "CoverImageUrl" character varying(500),
                    "DisplayOrder" integer NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    "UpdatedAtUtc" timestamp with time zone,
                    "IsDeleted" boolean NOT NULL,
                    "DeletedAtUtc" timestamp with time zone
                );
                ALTER TABLE "HotelHubs" ADD COLUMN IF NOT EXISTS "ExternalId" integer;
                ALTER TABLE "HotelHubs" ADD COLUMN IF NOT EXISTS "DisplayMode" character varying(40) NOT NULL DEFAULT 'categories';

                CREATE TABLE IF NOT EXISTS "CatalogCategories" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                    "ExternalId" integer,
                    "HubId" uuid NOT NULL REFERENCES "HotelHubs" ("Id") ON DELETE CASCADE,
                    "ParentCategoryId" uuid REFERENCES "CatalogCategories" ("Id") ON DELETE RESTRICT,
                    "TitleAz" character varying(200) NOT NULL,
                    "TitleEn" character varying(200),
                    "TitleRu" character varying(200),
                    "SubtitleAz" character varying(300),
                    "SubtitleEn" character varying(300),
                    "SubtitleRu" character varying(300),
                    "ImageUrl" text,
                    "IconKey" text,
                    "WorkingHours" character varying(100),
                    "InternalExtension" character varying(50),
                    "LayoutStyle" character varying(30) NOT NULL,
                    "CollapseIfSingleItem" boolean NOT NULL DEFAULT true,
                    "DisplayOrder" integer NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    "UpdatedAtUtc" timestamp with time zone,
                    "IsDeleted" boolean NOT NULL,
                    "DeletedAtUtc" timestamp with time zone
                );
                ALTER TABLE "CatalogCategories" ADD COLUMN IF NOT EXISTS "ExternalId" integer;
                ALTER TABLE "CatalogCategories" ADD COLUMN IF NOT EXISTS "CollapseIfSingleItem" boolean NOT NULL DEFAULT true;

                CREATE TABLE IF NOT EXISTS "CatalogItems" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                    "ExternalId" integer,
                    "HubId" uuid NOT NULL REFERENCES "HotelHubs" ("Id") ON DELETE CASCADE,
                    "CategoryId" uuid REFERENCES "CatalogCategories" ("Id") ON DELETE SET NULL,
                    "ItemType" integer NOT NULL,
                    "TitleAz" character varying(250) NOT NULL,
                    "TitleEn" character varying(250),
                    "TitleRu" character varying(250),
                    "SubtitleAz" character varying(300),
                    "SubtitleEn" character varying(300),
                    "SubtitleRu" character varying(300),
                    "DescriptionAz" text,
                    "DescriptionEn" text,
                    "DescriptionRu" text,
                    "BadgeAz" character varying(100),
                    "BadgeEn" character varying(100),
                    "BadgeRu" character varying(100),
                    "IconKey" character varying(60),
                    "FilterTag" character varying(50),
                    "Price" numeric(18,2),
                    "OldPrice" numeric(18,2),
                    "CostPrice" numeric(18,2) NOT NULL DEFAULT 0,
                    "DiscountPercent" numeric(5,2) NOT NULL DEFAULT 0,
                    "NoServiceFee" boolean NOT NULL DEFAULT false,
                    "PrepTimeMinutes" integer,
                    "Currency" character varying(15) NOT NULL,
                    "PortionOrDuration" character varying(100),
                    "BulletPointsAz" text,
                    "BulletPointsEn" text,
                    "BulletPointsRu" text,
                    "ImageUrl" character varying(500),
                    "IsOrderable" boolean NOT NULL,
                    "IsFeaturedInDailySpecials" boolean NOT NULL,
                    "ActionType" character varying(50),
                    "ActionValue" character varying(500),
                    "ActionLabelAz" text,
                    "ActionLabelEn" text,
                    "ActionLabelRu" text,
                    "ActionUrl" character varying(500),
                    "ModifierGroupsJson" text,
                    "DisplayOrder" integer NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    "UpdatedAtUtc" timestamp with time zone,
                    "IsDeleted" boolean NOT NULL,
                    "DeletedAtUtc" timestamp with time zone
                );
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "ExternalId" integer;
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "SubtitleAz" character varying(300);
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "SubtitleEn" character varying(300);
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "SubtitleRu" character varying(300);
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "IconKey" character varying(60);
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "CostPrice" numeric(18,2) NOT NULL DEFAULT 0;
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "DiscountPercent" numeric(5,2) NOT NULL DEFAULT 0;
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "NoServiceFee" boolean NOT NULL DEFAULT false;
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "PrepTimeMinutes" integer;
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "ActionType" character varying(50);
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "ActionValue" character varying(500);
                ALTER TABLE "CatalogItems" ADD COLUMN IF NOT EXISTS "ModifierGroupsJson" text;

                CREATE TABLE IF NOT EXISTS "GuestOrders" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                    "OrderNumber" character varying(50) NOT NULL,
                    "OrderType" integer NOT NULL,
                    "RoomOrTableNumber" character varying(80) NOT NULL,
                    "GuestName" character varying(150) NOT NULL,
                    "PhoneNumber" character varying(50) NOT NULL,
                    "Notes" text,
                    "TotalAmount" numeric(18,2) NOT NULL,
                    "Currency" character varying(15) NOT NULL,
                    "Status" integer NOT NULL,
                    "IsEmailSent" boolean NOT NULL,
                    "IsTelegramSent" boolean NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    "UpdatedAtUtc" timestamp with time zone
                );

                CREATE TABLE IF NOT EXISTS "GuestOrderItems" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "OrderId" uuid NOT NULL REFERENCES "GuestOrders" ("Id") ON DELETE CASCADE,
                    "CatalogItemId" uuid,
                    "ItemTitle" character varying(250) NOT NULL,
                    "Quantity" integer NOT NULL,
                    "UnitPrice" numeric(18,2) NOT NULL,
                    "LineTotal" numeric(18,2) NOT NULL,
                    "SpecialInstruction" text
                );

                CREATE TABLE IF NOT EXISTS "GuestReviews" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                    "Rating" integer NOT NULL,
                    "GuestName" character varying(150),
                    "RoomNumber" character varying(50),
                    "PhoneNumber" character varying(50),
                    "Department" character varying(80),
                    "Comment" character varying(2000),
                    "IsResolved" boolean NOT NULL,
                    "IsTelegramAlertSent" boolean NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    "UpdatedAtUtc" timestamp with time zone
                );

                CREATE TABLE IF NOT EXISTS "HotelDepartmentContacts" (
                    "Id" uuid NOT NULL PRIMARY KEY,
                    "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                    "TitleAz" character varying(150) NOT NULL,
                    "TitleEn" character varying(150),
                    "TitleRu" character varying(150),
                    "SubtitleAz" text,
                    "SubtitleEn" text,
                    "SubtitleRu" text,
                    "PhoneNumber" character varying(80),
                    "InternalExtension" character varying(50),
                    "WhatsAppNumber" character varying(80),
                    "Email" character varying(150),
                    "WorkingHours" character varying(100),
                    "IconKey" text,
                    "DisplayOrder" integer NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    "UpdatedAtUtc" timestamp with time zone,
                    "IsDeleted" boolean NOT NULL,
                    "DeletedAtUtc" timestamp with time zone
                );
                """);
            await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Verilənlər bazası başlanğıcında bildiriş (PostgreSQL aktiv deyilsə sonradan qoşula bilər): {Message}", ex.Message);
    }
}

app.Run();
