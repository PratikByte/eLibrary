using EBook.CQRS.Command;
using MediatR;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Application.Interfaces.Services;
using eLibrary.Infrastructure.Services;
using eLibrary.Shared;
using eLibrary.Domain.Entities;

namespace EBook.CQRS.Command
{
    public record  BorrowBookCommand(int UserId, int BookId) : IRequest<ApiResponse<BorrowRecordDto>>;
    
}

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, ApiResponse<BorrowRecordDto>>
{
    private readonly IBorrowRepository _borrowRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BorrowBookCommandHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;

    public BorrowBookCommandHandler(IBorrowRepository borrowRepository, IBookRepository bookRepository, ILogger<BorrowBookCommandHandler> logger, INotificationService notificationService, IEmailService emailService, IUserRepository userRepository)
    {
        _borrowRepository = borrowRepository;
        _bookRepository = bookRepository;
        _logger = logger;
        _notificationService = notificationService;
        _emailService = emailService;
        _userRepository = userRepository;
    }

  
       public async Task<ApiResponse<BorrowRecordDto>> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate user
            var user = await _userRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return ApiResponse<BorrowRecordDto>.Fail($"User with ID {request.UserId} not found.");
            }

            // 2. Validate book
            var bookResponse = await _bookRepository.GetBookByIdAsync(request.BookId ,cancellationToken);
            if (bookResponse == null)
            {
                return ApiResponse<BorrowRecordDto>.Fail($"Book with ID {request.BookId} not found.");
            }

            var bookDetails = bookResponse;
            if (bookDetails.AvailableCount <= 0)
            {
                return ApiResponse<BorrowRecordDto>.Fail("Book is out of stock.");
            }

            // 3. Decrement stock
            var book = await _bookRepository.GetBookByIdAsync(request.BookId,cancellationToken);
            if (book == null)
            {
                return ApiResponse<BorrowRecordDto>.Fail("Book not found.");
            }

            book.AvailableCount--;
            await _bookRepository.UpdateAsync(book,cancellationToken);

            // 4. Save entity
            var borrowRecord = new BorrowRecords
            {
                UserId = request.UserId,
                BookId = request.BookId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                IsReturned = false
            };

            await _borrowRepository.AddAsync(borrowRecord);
            await _borrowRepository.SaveChangesAsync();

            // 5. Map to DTO
            var borrowRecordDto = new BorrowRecordDto
            {
                Id = borrowRecord.Id,
                UserId = borrowRecord.UserId,
                UserName = user.Username,
                BookId = borrowRecord.BookId,
                BookName = bookDetails.Title,
                BorrowDate = borrowRecord.BorrowDate.ToString("dd-MMM-yyyy"),
                DueDate = borrowRecord.DueDate.ToString("dd-MMM-yyyy"),
                //ReturnDate = borrowRecord.ReturnDate?.ToString("dd-MMM-yyyy"),
                //IsReturned = borrowRecord.IsReturned,
                //Fine = borrowRecord.Fine
            };

            var message = $"Book borrowed successfully. Please return by {borrowRecord.DueDate:dd-MMM-yyyy}.";
            string? warning = null;

            // Email notification is optional so the borrow flow succeeds even if SMTP is down.
            try
            {
                var (subject, body) = await _notificationService
                    .GenerateBorrowConfirmationEmail(borrowRecordDto, user.Username, bookDetails.Title);

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                warning = "Book borrowed successfully, but confirmation email could not be sent.";
                _logger.LogWarning(
                    ex,
                    "Borrow confirmation email failed for UserId {UserId}, BookId {BookId}",
                    request.UserId,
                    request.BookId);
            }

            return ApiResponse<BorrowRecordDto>.Ok(borrowRecordDto, message, warning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while borrowing book for User ID {UserId} and Book ID {BookId}", request.UserId, request.BookId);
            return ApiResponse<BorrowRecordDto>.Fail("An error occurred while processing your request.");
        }
    }



}
