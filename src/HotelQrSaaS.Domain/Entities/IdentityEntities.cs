using Microsoft.AspNetCore.Identity;
using HotelQrSaaS.Domain.Common;

namespace HotelQrSaaS.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ISoftDelete
{
    public string FullName { get; set; } = string.Empty;
    public Guid? TenantId { get; set; } // Null for SuperAdmin, set for HotelAdmin & HotelStaff
    public virtual Tenant? Tenant { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ReplacedByToken { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAtUtc;
}

