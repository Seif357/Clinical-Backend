using Application.Dto.AuthDto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Application.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {// tode: add custom validation
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Username)
            .NotEmpty().WithMessage("username is required")
            .MinimumLength(3).WithMessage("username must be at least 3 characters long")
            .MinimumLength(20).WithMessage("username must not exceed 20 characters");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .EmailAddress().WithMessage("please provide a valid email address");
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("please provide a valid phone number");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required")
                .MinimumLength(8).WithMessage("password must be at least 8 characters long")
                .Matches(@"[A-Z]+").WithMessage("password must contain at least one uppercase letter")
                .Matches(@"[a-z]+").WithMessage("password must contain at least one lowercase letter")
                .Matches(@"\d+").WithMessage("password must contain at least one digit")
                .Matches(@"[\W_]+").WithMessage("password must contain at least one special character");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("passwords do not match");
        }
    }
}
