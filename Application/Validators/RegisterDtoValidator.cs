using Application.Dto.AuthDto;
using FluentValidation;

namespace Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.ProfessionalPracticeLicense)
            .NotEmpty()
            .When(x => x.IsDoctor)
            .WithMessage("Professional Practice License number is required for Doctor Registration.");
        RuleFor(x => x.IssuingAuthority)
            .NotEmpty()
            .When(x => x.IsDoctor)
            .WithMessage("Issuing Authority is required for Doctor Registration.");
    }
}