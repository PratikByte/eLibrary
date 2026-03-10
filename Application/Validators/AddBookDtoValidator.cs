using eLibrary.Application.DTOs;
using FluentValidation;

namespace eLibrary.Application.Validators;

public class AddBookDtoValidator: AbstractValidator<BookDto>
{
    public AddBookDtoValidator()
    {
        RuleFor(dto => dto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long.");

        RuleFor(dto => dto.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MinimumLength(3).WithMessage("Author must be at least 3 characters long.");
    }
}

