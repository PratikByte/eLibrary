using eLibrary.Book.Application.Commands.Books;
using FluentValidation;

namespace eLibrary.Book.Application.Validators;

public sealed class UpdateBookPriceCommandValidator : AbstractValidator<UpdateBookPriceCommand>
{
    public UpdateBookPriceCommandValidator()
    {
        RuleFor(command => command.BookId)
            .GreaterThan(0);

        RuleFor(command => command.NewPrice)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);
    }
}
