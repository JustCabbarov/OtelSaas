using FluentValidation;
using HotelQrSaaS.Application.DTOs;

namespace HotelQrSaaS.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Düzgün e-poçt ünvanı daxil edin.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifrə boş ola bilməz.");
    }
}

public class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequestDto>
{
    public CreateTenantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Otel adı mütləqdir.");
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$")
            .WithMessage("Slug yalnız kiçik ingilis hərfləri, rəqəmlər və defisdən ibarət ola bilər (məsələn: hotel-resort).");
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress().WithMessage("Admin üçün düzgün e-poçt daxil edin.");
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(6).WithMessage("Şifrə ən azı 6 simvol olmalıdır.");
        RuleFor(x => x.AdminFullName).NotEmpty().WithMessage("Admin adı mütləqdir.");
    }
}

public class SubmitInquiryRequestValidator : AbstractValidator<SubmitInquiryRequestDto>
{
    public SubmitInquiryRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150).WithMessage("Ad və soyad mütləqdir.");
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(50).WithMessage("Əlaqə nömrəsi mütləqdir.");
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000).WithMessage("Müraciət mətni mütləqdir (maksimum 1000 simvol).");
    }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.TitleAz).NotEmpty().MaximumLength(100).WithMessage("Bölmə adı (AZ) mütləqdir.");
        RuleFor(x => x.LayoutType).Must(x => x == "icons" || x == "list")
            .WithMessage("Layout yalnız 'icons' və ya 'list' ola bilər.");
    }
}

public class CreateLinkItemValidator : AbstractValidator<CreateLinkItemDto>
{
    public CreateLinkItemValidator()
    {
        RuleFor(x => x.TitleAz).NotEmpty().MaximumLength(150).WithMessage("Keçid başlığı (AZ) mütləqdir.");
        RuleFor(x => x.TargetUrl).NotEmpty().WithMessage("Hədəf link və ya nömrə mütləqdir.");
    }
}

