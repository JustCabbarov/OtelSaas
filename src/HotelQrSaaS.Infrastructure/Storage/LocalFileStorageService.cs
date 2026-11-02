using Microsoft.AspNetCore.Hosting;
using HotelQrSaaS.Application.Common.Exceptions;
using HotelQrSaaS.Application.Interfaces.Services;

namespace HotelQrSaaS.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg", ".pdf"
    };

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string folder,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException($"Yalnız bu fayl formatlarına icazə verilir: {string.Join(", ", AllowedExtensions)}");
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var targetDirectory = Path.Combine(webRoot, "uploads", tenantId.ToString("N"), folder);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(targetDirectory, uniqueFileName);

        using (var outputStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(outputStream, cancellationToken);
        }

        // Return browser-friendly relative URL path
        return $"/uploads/{tenantId:N}/{folder}/{uniqueFileName}";
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var cleanRelativePath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(webRoot, cleanRelativePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
        catch
        {
            // Logging can be added; silent delete prevents crashing business flow
        }
    }
}

