using eLibrary.Book.Application.DTOs;
using FluentValidation;

namespace eLibrary.Book.Application.Validators;

public sealed class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public CreateBookDtoValidator()
    {
        RuleFor(book => book.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200);

        RuleFor(book => book.Author)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200);

        RuleFor(book => book.Price)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);

        RuleFor(book => book.PublishedYear)
            .InclusiveBetween(1, 9999);

        RuleFor(book => book.AvailableCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(book => book.CoverImage)
            .Must(file => file is null || (file.Length > 0 && file.Length <= 5 * 1024 * 1024))
            .WithMessage("Cover image must be non-empty and no larger than 5 MB.")
            .Must(file => file is null || AllowedExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Cover image must be a JPG, PNG, or WebP file.");

        RuleFor(book => book.CoverImage)
            .Must(file => file is null || file.ContentType is "image/jpeg" or "image/png" or "image/webp")
            .WithMessage("Cover image content type must be JPEG, PNG, or WebP.");
    }
}
