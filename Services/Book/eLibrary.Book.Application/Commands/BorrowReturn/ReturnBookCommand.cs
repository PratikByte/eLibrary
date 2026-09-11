using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;

namespace eLibrary.Book.Application.Commands.BorrowReturn;

public record ReturnBookCommand(Guid BorrowId, Guid? CurrentUserId = null, bool IsAdmin = false, string? UserName = null)
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

        var book = await _bookRepository.GetBookByIdAsync(record.BookId, cancellationToken);
        if (book is null)
            return ApiResponse<BorrowRecordDto>.Fail("Book return failed because the book could not be found.");

        record.IsReturned = true;
        record.ReturnDate = DateTime.UtcNow;
        if (record.ReturnDate > record.DueDate)
            record.Fine = (record.ReturnDate.Value - record.DueDate).Days * 10;

        book.AvailableCount++;
        _borrowRepository.Update(record);
        await _borrowRepository.SaveChangesAsync(cancellationToken);

        var dto = BorrowBookCommandHandler.Map(record, request.UserName ?? "User", book.Title ?? "Unknown Book");
        var message = dto.Fine is > 0
            ? $"Book returned with fine ₹{dto.Fine}."
            : "Book returned successfully.";
        return ApiResponse<BorrowRecordDto>.Ok(dto, message);
    }
}