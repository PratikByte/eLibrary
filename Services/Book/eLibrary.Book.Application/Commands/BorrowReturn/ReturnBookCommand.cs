using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Common.Helpers;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;

namespace eLibrary.Book.Application.Commands.BorrowReturn;

public record ReturnBookCommand(
    Guid BorrowId,
    DateOnly? ReturnDate = null,
    Guid? CurrentUserId = null,
    bool IsAdmin = false,
    string? UserName = null)
    : IRequest<ApiResponse<BorrowRecordDto>>;

public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, ApiResponse<BorrowRecordDto>>
{
    private readonly IBorrowRepository _borrowRepository;
    private readonly IBookRepository _bookRepository;

    public ReturnBookCommandHandler(IBorrowRepository borrowRepository, IBookRepository bookRepository)
    {
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
    }

    public async Task<ApiResponse<BorrowRecordDto>> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var record = await _borrowRepository.GetByIdAsync(request.BorrowId, cancellationToken);
        if (record is null)
            return ApiResponse<BorrowRecordDto>.Fail($"Borrow record with ID {request.BorrowId} not found.");

        if (!request.IsAdmin && (!request.CurrentUserId.HasValue || request.CurrentUserId.Value != record.UserId))
            return ApiResponse<BorrowRecordDto>.Fail("You are not allowed to return this book.");

        if (record.IsReturned)
            return ApiResponse<BorrowRecordDto>.Fail("This book has already been returned.");

        if (!request.ReturnDate.HasValue)
            return ApiResponse<BorrowRecordDto>.Fail("A return date is required.");

        var returnDateOnly = request.ReturnDate.Value;
        if (returnDateOnly > DateOnly.FromDateTime(DateTime.UtcNow))
            return ApiResponse<BorrowRecordDto>.Fail("The return date cannot be in the future.");

        if (returnDateOnly < DateOnly.FromDateTime(record.BorrowDate))
            return ApiResponse<BorrowRecordDto>.Fail("The return date cannot be before the borrow date.");

        var book = await _bookRepository.GetBookByIdAsync(record.BookId, cancellationToken);
        if (book is null)
            return ApiResponse<BorrowRecordDto>.Fail("Book return failed because the book could not be found.");

        var returnDate = returnDateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fineAmount = BorrowFineCalculator.Calculate(record.DueDate, returnDate);
        decimal? fine = fineAmount > 0
            ? fineAmount
            : (decimal?)null;
        var returned = await _borrowRepository.TryReturnAsync(
            record.Id,
            book.Id,
            returnDate,
            fine,
            cancellationToken);
        if (!returned)
            return ApiResponse<BorrowRecordDto>.Fail("This book has already been returned.");

        record.IsReturned = true;
        record.ReturnDate = returnDate;
        record.Fine = fine;

        var dto = BorrowBookCommandHandler.Map(record, request.UserName ?? "User", book.Title ?? "Unknown Book");
        var message = dto.Fine is > 0
            ? $"Book returned with fine ₹{dto.Fine}."
            : "Book returned successfully.";
        return ApiResponse<BorrowRecordDto>.Ok(dto, message);
    }
}