using Application.Dto.AI;
using Domain.Constants;
using FluentValidation;

namespace Application.Validators;

public class UploadImageValidator : AbstractValidator<UploadImageDto>
{
    public UploadImageValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Image file is required");

        RuleFor(x => x.Image.Length)
            .LessThanOrEqualTo(FileUploadSettings.MaxFileSizeBytes)
            .When(x => x.Image != null)
            .WithMessage($"File size must not exceed {FileUploadSettings.MaxFileSizeMB} MB");

        RuleFor(x => x.Image.FileName)
            .Must(HaveValidExtension)
            .When(x => x.Image != null)
            .WithMessage($"File must be one of the following types: {string.Join(", ", FileUploadSettings.AllowedExtensions)}");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 1000 characters");
    }

    private bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return FileUploadSettings.AllowedExtensions.Contains(extension);
    }
}
