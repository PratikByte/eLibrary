using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using eLibrary.Book.Domain.Entities;
using MediatR;

namespace eLibrary.Book.Application.Commands.BorrowReturn;

public record BorrowBookCommand(Guid UserId, Guid BookId, string? UserName = null) : IRequest<ApiResponse<BorrowRecordDto>>;

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, ApiResponse<BorrowRecordDto>>
{
    private readonly IBorrowRepository _borrowRepository;
    private readonly IBookRepository _bookRepository;

    public BorrowBookCommandHandler(IBorrowRepository borrowRepository, IBookRepository bookRepository)
    {
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
    }

    public async Task<ApiResponse<BorrowRecordDto>> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
        if (book is null)
            return ApiResponse<BorrowRecordDto>.FailWithCode("NOT_FOUND", "Book not found.");

        if (book.AvailableCount <= 0)
            return ApiResponse<BorrowRecordDto>.FailWithCode("CONFLICT", "Book is out of stock.");

        var borrowedAt = DateTime.UtcNow;
        var record = new BorrowRecord
        {
            UserId = request.UserId,
            UserName = request.UserName ?? "User",
            BookId = request.BookId,
            BookName = book.Title ?? "Unknown Book",
            BorrowDate = borrowedAt,
            DueDate = borrowedAt.AddDays(7)
        };

        book.AvailableCount--;
        await _borrowRepository.AddAsync(record, cancellationToken);
        await _borrowRepository.SaveChangesAsync(cancellationToken);

        var dto = Map(record, request.UserName ?? "User", book.Title ?? "Unknown Book");
        return ApiResponse<BorrowRecordDto>.Ok(dto,
            $"Book borrowed successfully. Please return by {record.DueDate:dd-MMM-yyyy}.");
    }

    internal static BorrowRecordDto Map(BorrowRecord record, string userName, string bookName) => new()
    {
        Id = record.Id,
        UserId = record.UserId,
        UserName = userName,
        BookId = record.BookId,
        BookName = bookName,
        BorrowDate = record.BorrowDate.ToString("dd-MMM-yyyy"),
        DueDate = record.DueDate.ToString("dd-MMM-yyyy"),
        ReturnDate = record.ReturnDate?.ToString("dd-MMM-yyyy"),
        IsReturned = record.IsReturned,
        Fine = record.Fine
    };
}
