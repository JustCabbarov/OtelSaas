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
        Description = "JWT Authorization başlığı 'Bearer {token}' formatında daxil edilməlidir. Məsələn: 'Bearer eyJhbGciOiJIUzI1Ni...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// Serve default index.html and static uploaded files (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

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
