using System.Text;
using QRCoder;
using HotelQrSaaS.Application.Interfaces.Services;
using HotelQrSaaS.Domain.Entities;

namespace HotelQrSaaS.Infrastructure.QrCode;

public class QrCodeService : IQrCodeService
{
    // Bütün otellər üçün vahid şirkət loqosu - Altensor (Static Altensor Company Logo)
    private static readonly Lazy<byte[]?> StaticAltensorLogoBytes = new(() =>
    {
        try
        {
            var paths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "wwwroot", "assets", "altensor-logo.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "altensor-logo.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "assets", "altensor-logo.png"),
                @"C:\Users\Huawei\Desktop\Otel SaaS\assets\altensor-logo.png",
                @"C:\Users\Huawei\Desktop\Otel SaaS\src\HotelQrSaaS.WebApi\wwwroot\assets\altensor-logo.png"
            };

            foreach (var p in paths)
            {
                if (File.Exists(p)) return File.ReadAllBytes(p);
            }
        }
        catch { }
        return null;
    });

    public byte[] GeneratePngQrCode(string url, int pixelsPerModule = 20, string? foregroundHex = null, string? backgroundHex = null)
    {
        using var qrGenerator = new QRCodeGenerator();
        // Use ECC Level H (High 30% error recovery) for maximum tolerance
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.H);
        var qrCode = new PngByteQRCode(qrCodeData);

        var darkColor = ParseHexToBytes(foregroundHex, defaultDark: true);
        var lightColor = ParseHexToBytes(backgroundHex, defaultDark: false);

        return qrCode.GetGraphic(pixelsPerModule, darkColor, lightColor);
    }

    public string GenerateSvgQrCode(string url, string? foregroundHex = null, string? backgroundHex = null)
    {
        using var qrGenerator = new QRCodeGenerator();
        // ECC Level H for maximum error recovery with static center logo
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.H);
        var svgQrCode = new SvgQRCode(qrCodeData);

        var dark = !string.IsNullOrWhiteSpace(foregroundHex) ? foregroundHex : "#000000";
        var light = !string.IsNullOrWhiteSpace(backgroundHex) ? backgroundHex : "#FFFFFF";

        // Bütün otellərin QR kodlarının mərkəzində şirkətin rəsmi statik Altensor loqosu yerləşdirilir
        var logoBytes = StaticAltensorLogoBytes.Value;
        if (logoBytes != null && logoBytes.Length > 0)
        {
            var logo = new SvgQRCode.SvgLogo(logoBytes, iconSizePercent: 22, fillLogoBackground: true);
            return svgQrCode.GetGraphic(20, dark, light, drawQuietZones: true, sizingMode: SvgQRCode.SizingMode.WidthHeightAttribute, logo: logo);
        }

        return svgQrCode.GetGraphic(20, dark, light, drawQuietZones: true);
    }

    private static byte[] ParseHexToBytes(string? hex, bool defaultDark)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return defaultDark ? new byte[] { 0, 0, 0, 255 } : new byte[] { 255, 255, 255, 255 };
        }

        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            return new byte[]
            {
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16),
                255
            };
        }

        return defaultDark ? new byte[] { 0, 0, 0, 255 } : new byte[] { 255, 255, 255, 255 };
    }
}

public class VCardService : IVCardService
{
    public byte[] GenerateVCardBytes(Tenant tenant)
    {
        var vcardString = GenerateVCardString(tenant);
        return Encoding.UTF8.GetBytes(vcardString);
    }

    public string GenerateVCardString(Tenant tenant)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCARD");
        sb.AppendLine("VERSION:3.0");
        sb.AppendLine($"FN;CHARSET=UTF-8:{tenant.Name}");
        sb.AppendLine($"ORG;CHARSET=UTF-8:{tenant.Name}");

        if (!string.IsNullOrWhiteSpace(tenant.BioAz))
        {
            sb.AppendLine($"NOTE;CHARSET=UTF-8:{tenant.BioAz.Replace("\r\n", " ").Replace("\n", " ")}");
        }

        if (!string.IsNullOrWhiteSpace(tenant.Phone))
        {
            sb.AppendLine($"TEL;TYPE=WORK,VOICE:{tenant.Phone}");
        }

        if (!string.IsNullOrWhiteSpace(tenant.WhatsApp))
        {
            sb.AppendLine($"TEL;TYPE=CELL,VOICE:{tenant.WhatsApp}");
        }

        if (!string.IsNullOrWhiteSpace(tenant.Email))
        {
            sb.AppendLine($"EMAIL;TYPE=PREF,INTERNET:{tenant.Email}");
        }

        if (!string.IsNullOrWhiteSpace(tenant.Address))
        {
            sb.AppendLine($"ADR;TYPE=WORK;CHARSET=UTF-8:;;{tenant.Address};;;;");
        }

        if (!string.IsNullOrWhiteSpace(tenant.MapUrl))
        {
            sb.AppendLine($"URL:{tenant.MapUrl}");
        }

        sb.AppendLine("END:VCARD");
        return sb.ToString();
    }
}

