using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using HotelQrSaaS.Application.Common;
using HotelQrSaaS.Application.Common.Exceptions;
using HotelQrSaaS.Application.DTOs;
using HotelQrSaaS.Application.Interfaces.Repositories;
using HotelQrSaaS.Application.Interfaces.Services;
using HotelQrSaaS.Application.Interfaces.UnitOfWork;
using HotelQrSaaS.Domain.Entities;
using HotelQrSaaS.Domain.Enums;

namespace HotelQrSaaS.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user == null || user.IsDeleted)
        {
            throw new UnauthorizedException("E-poçt və ya şifrə yanlışdır.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("E-poçt və ya şifrə yanlışdır.");
        }

        Tenant? tenant = null;
        if (user.TenantId.HasValue)
        {
            tenant = await _unitOfWork.Tenants.GetByIdAsync(user.TenantId.Value, cancellationToken);
            if (tenant == null || tenant.IsDeleted)
            {
                throw new UnauthorizedException("Bağlı olduğunuz otel tapılmadı və ya silinib.");
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, refreshToken, expiresAt) = await _jwtTokenService.GenerateTokensAsync(user, roles, tenant);

        user.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        });

        await _userManager.UpdateAsync(user);

        return new LoginResponseDto(
            accessToken,
            refreshToken,
            expiresAt,
            user.Email ?? string.Empty,
            user.FullName,
            roles,
            tenant?.Id,
            tenant?.Slug,
            tenant?.Name
        );
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedException("Yanlış Access Token.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Token istifadəçi məlumatı tapılmadı.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || user.IsDeleted)
        {
            throw new UnauthorizedException("İstifadəçi tapılmadı.");
        }

        var existingRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (existingRefreshToken == null || !existingRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Refresh Token etibarsızdır və ya müddəti bitib.");
        }

        existingRefreshToken.IsRevoked = true;

        Tenant? tenant = null;
        if (user.TenantId.HasValue)
        {
            tenant = await _unitOfWork.Tenants.GetByIdAsync(user.TenantId.Value, cancellationToken);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (newAccessToken, newRefreshToken, expiresAt) = await _jwtTokenService.GenerateTokensAsync(user, roles, tenant);

        user.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            ReplacedByToken = request.RefreshToken
        });

        await _userManager.UpdateAsync(user);

        return new LoginResponseDto(
            newAccessToken,
            newRefreshToken,
            expiresAt,
            user.Email ?? string.Empty,
            user.FullName,
            roles,
            tenant?.Id,
            tenant?.Slug,
            tenant?.Name
        );
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default)
    {
        var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
        {
            throw new UnauthorizedException();
        }

        var roles = await _userManager.GetRolesAsync(user);
        string? tenantSlug = null;
        if (user.TenantId.HasValue)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(user.TenantId.Value, cancellationToken);
            tenantSlug = tenant?.Slug;
        }

        return new CurrentUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            roles,
            user.TenantId,
            tenantSlug
        );
    }
}
